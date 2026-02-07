using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Force.Crc32;

using Lawrence.Game;

namespace Lawrence.Core;

// Metadata structure for packet acknowledgement data
public record struct AckedMetadata {
    public byte AckCycle;
    public byte AckIndex;
    public byte[] Packet;
    public int ResendTimer;
    public long Timestamp;
    public Action? AckCallback;
}

public interface IClientHandler {
    Moby CreateMoby(ushort oClass, byte spawnId);
    void UpdateMoby(MPPacketMobyUpdate updatePacket);
    void OnDamage(Moby source, Moby target, ushort sourceOClass, float damage);
    void ControllerInputTapped(ControllerInput input);
    void ControllerInputHeld(ControllerInput input);
    void ControllerInputReleased(ControllerInput input);
    void Delete();
    Moby Moby();
    void PlayerRespawned(byte spawnId, ushort levelId);
    void GameStateChanged(GameState state);
    void CollectedGoldBolt(int planet, int number);
    void UnlockItem(int item, bool equip);
    void OnStartInLevelMovie(uint movie, uint levelId);
    void OnUnlockLevel(int level);
    void OnUnlockSkillpoint(byte skillpoint);
    void OnDisconnect();
    void OnHybridMobyValueChange(ushort uid, MonitoredValueType type, ushort offset, ushort size, byte[] oldValue, byte[] newValue);
    void OnMonitoredAddressChanged(uint address, byte size, byte[] oldValue, byte[] newValue);
    void OnLevelFlagChanged(ushort type, byte level, byte size, ushort index, uint value);
    void OnGiveBolts(int boltDiff, uint totalBolts);
    bool HasMoby(Moby moby);
    void DeleteMoby(Moby moby);
    void UIEvent(MPUIElementEventType eventType, ushort elementId, uint data, byte[] extraData);
    void OnPlayerStandingOnMoby(Moby? moby);
}

public partial class Client {
    // Which API version we're currently on and which is the minimum version we support. 
    private static uint API_VERSION = 12;
    private static uint API_VERSION_MIN = 12;
    
    /// <summary>
    /// When true, this client is waiting to connect, and is not yet part of the regular OnTick loop
    /// </summary>
    public bool WaitingToConnect = true;

    public uint? DataStreamKey;

    private IClientHandler? _clientHandler;
    public IClientHandler? ClientHandler => _clientHandler;

    private string? _username = null;
    private int _userid = 0;

    private readonly IPEndPoint _endpoint;
    private bool _handshakeCompleted = false;
    public bool HandshakeCompleted => _handshakeCompleted;

    public uint ID = 0;

    private long _lastContact = 0;

    private bool _disconnected = true;
    private bool _processedFirstPacket = false;
    
    private Packet.Endianness _endianness = Packet.Endianness.BigEndian;
    public Packet.Endianness Endianness => _endianness;
    
    private byte _ackCycle = 1;
    private byte _ackIndex = 1;
    public readonly AckedMetadata[] Acked = new AckedMetadata[256];
    private readonly List<AckedMetadata> _unacked = new();

    private readonly Server _server;
    public Server Server => _server;
    
    private readonly PacketRouter _router;
    
    public Client(IPEndPoint endpoint, uint id, Server server) {
        ID = id;
        _endpoint = endpoint;
        _server = server;
        _router = new PacketRouter(this);
        
        _lastContact = DateTimeOffset.Now.ToUnixTimeSeconds();
        _disconnected = false;

        _recvLock = new Mutex();
    }

    public void SetHandler(IClientHandler handler) {
        _clientHandler = handler;
    }

    public IPEndPoint GetEndpoint() {
        return _endpoint;
    }

    public string? GetUsername() {
        return _username;
    }

    public int GetUserid() {
        return _userid;
    }

    // Amount of seconds since we last saw activity on this client.
    public long GetInactiveSeconds() {
        return DateTimeOffset.Now.ToUnixTimeSeconds() - _lastContact;
    }

    public int UnackedPacketsCount() {
        return _unacked.Count;
    }

    public bool IsDisconnected() {
        return _disconnected;
    }

    public bool IsActive() {
        return !_disconnected && _handshakeCompleted;
    }

    (byte, byte) NextAck() {
        if (_ackIndex >= 254) {
            _ackIndex = 0;
            _ackCycle++;
        }

        if (_ackCycle >= 254) {
            _ackCycle = 0;
        }

        return (++_ackIndex, _ackCycle);
    }
    
    private readonly List<byte> _buffer = new();
    private const int BufferSize = 1024;

    public void SendPacket(MPPacketHeader packetHeader, byte[]? packetBody, Action? ackCallback = null) {
        packetHeader.TimeSent = (long)Server.Time();

        var bodyLen = 0;
        if (packetBody != null) {
            bodyLen = packetBody.Length;
        }
        byte[] packet = new byte[Marshal.SizeOf<MPPacketHeader>() + bodyLen];

        // Fill ack fields if necessary
        if (packetHeader.RequiresAck == 255 && packetHeader.AckCycle == 255) {
            (packetHeader.RequiresAck, packetHeader.AckCycle) = NextAck();
        }

        Packet.StructToBytes(packetHeader, _endianness).CopyTo(packet, 0);
        if (packetBody != null) {
            packetBody.CopyTo(packet, Marshal.SizeOf<MPPacketHeader>());
        }

        // Cache ack response packets
        if (packetHeader.PacketType == MPPacketType.MP_PACKET_ACK && packetHeader.RequiresAck != 0) {
            var ack = Acked[packetHeader.RequiresAck];
            if (ack.AckCycle != packetHeader.AckCycle) {
                ack.AckCycle = packetHeader.AckCycle;
                ack.Packet = packet;
                ack.ResendTimer = 120;
            }

            Acked[packetHeader.RequiresAck] = ack;
        }

        // Cache unacked request packets
        if (packetHeader.PacketType != MPPacketType.MP_PACKET_ACK && packetHeader.RequiresAck != 0) {
            if (_unacked.Count >= 256) {
                //Console.WriteLine($"Player {ID} has more than 256 unacked packets. We should probably boot this client.");
                _unacked.Clear();
            }

            _unacked.Add(new AckedMetadata {
                Packet = packet, 
                AckIndex = packetHeader.RequiresAck, 
                AckCycle = packetHeader.AckCycle, 
                ResendTimer = 30, 
                Timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
                AckCallback = ackCallback
            });
        }

        // Add packet to buffer
        _buffer.AddRange(packet);

        // Check if buffer size reached
        if (_buffer.Count >= BufferSize) {
            // Send the buffer
            Flush();
        }
    }
    
    public void Flush() {
        if (_buffer.Count > 0) {
            _server.SendTo(_buffer.ToArray(), _endpoint);
            _buffer.Clear();
        }
    }

    public void SendPacket(Packet packet) {
        (MPPacketHeader header, byte[] body) = packet.GetBytes(_endianness);
        
        SendPacket(header, body);
    }

    public void SendPacket(Packet packet, Action ackCallback) {
        (MPPacketHeader header, byte[] body) = packet.GetBytes(_endianness);
        
        SendPacket(header, body, ackCallback);
    }

    private long _lastTimeSent = 0;

    // Parse and process a packet
    public void ParsePacket(byte[] packet) {
        int index = 0;

        while (index < packet.Length && packet.Length - index >= Marshal.SizeOf<MPPacketHeader>()) {
            // Start out by reading the header
            if (Packet.MakeHeader(packet.Skip(index).Take(Marshal.SizeOf<MPPacketHeader>()).ToArray(), _endianness)
                is not { } packetHeader) {
                throw new Exception("Bad packet");
            }
            
            index += Marshal.SizeOf<MPPacketHeader>();

            if (packetHeader.Size > packet.Length - index) {
                // Try to read in other endianness
                if (_endianness == Packet.Endianness.BigEndian) {
                    _endianness = Packet.Endianness.LittleEndian;
                } else {
                    _endianness = Packet.Endianness.BigEndian;
                }
                
                // Reset index
                index -= Marshal.SizeOf<MPPacketHeader>();
                if (Packet.MakeHeader(packet.Skip(index).Take(Marshal.SizeOf<MPPacketHeader>()).ToArray(), _endianness) is not { } resetPacketHeader) {
                    throw new Exception("Bad packet");
                }

                packetHeader = resetPacketHeader;
                
                if (packetHeader.Size > packet.Length - index) {
                    // Still too big, throw exception
                    throw new Exception("Bad packet");
                }
            }
            
            byte[] packetBody = packet.Skip(index).Take((int)packetHeader.Size).ToArray();

            index += (int)packetHeader.Size;
            
            if (packetHeader.TimeSent < _lastTimeSent) {
                // This packet is older than the last one we received. We don't want to process it.
                Logger.Log($"Player({ID}) sent an old packet. Ignoring.");
                continue;
            }
            
            _lastTimeSent = packetHeader.TimeSent;

            _router.RoutePacket(packetHeader, packetBody);
        }
    }

    private readonly Mutex _recvLock;
    private readonly bool _resetBuffer = false;
    private List<byte[]> _recvBuffer = new(4096);
    private readonly Dictionary<uint, long> _lastHashes = new(10);
    
    public void ReceiveData(byte[] data) {
        if (_disconnected) {
            return;
        }

        long timeNow = (long)Server.Time();

        _lastContact = timeNow / 1000;

        uint currentHash = Crc32Algorithm.Compute(data);
        
        if (_resetBuffer) {
            _recvLock.WaitOne();
            _recvBuffer = new List<byte[]>(4096);
            _recvLock.ReleaseMutex();
        }

        // Throw away duplicate packets in the last 200 milliseconds
        if (_lastHashes.ContainsKey(currentHash) && _lastHashes[currentHash] > timeNow - 200) {
            return;
        }

        // We store the crc32 sum of the last 10 packets
        // So we can discard future packets if they are the same sum
        // We send a lot of duplicate packets and we should be able to endure
        // any packet loss. So this should help us not process a bunch of redundant packets.
        if (_lastHashes.Count >= 10) {
            _lastHashes.Remove(0);
        }

        _lastHashes[currentHash] = timeNow;

        _recvLock.WaitOne();
        _recvBuffer.Add(data);
        _recvLock.ReleaseMutex();

        if (_recvBuffer.Count > 399) {
            _recvBuffer = new List<byte[]>(400);
        }
    }

    private List<byte[]>? DrainPackets() {
        // Make sure the networking receive thread isn't working with the buffer
        _recvLock.WaitOne();

        try {
            // We take at max 100 packets out of the buffer
            int takePackets = Math.Min(100, _recvBuffer.Count);

            if (takePackets <= 0) {
                // No packets in buffer
                return null;
            }

            // Drain packets from buffer
            List<byte[]> packets = _recvBuffer.Take(takePackets).ToList();
            _recvBuffer.RemoveRange(0, takePackets);

            if (_recvBuffer.Count > 0) {
                Logger.Trace($"[{GetUsername()}] There's still {_recvBuffer.Count} packets in the buffer.");
            }
            
            return packets;
        } finally {
            _recvLock.ReleaseMutex();
        }

        return null;
    }

    public void Tick() {
        if (_disconnected) {
            return;
        }

        Flush();

        var packets = DrainPackets();
        if (packets != null) {
            foreach (var packet in packets) {
                try {
                    ParsePacket(packet);

                    if (!_processedFirstPacket) {
                        _processedFirstPacket = true;
                    }
                } catch (Exception e) {
                    Logger.Error($"Encountered an exception in client", e);
                }
            }
        }

        // Resend unacked packets
        int index = 0;
        long timeNow = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
        foreach (var unacked in this._unacked.ToArray()) {
            var _unacked = unacked;
            if (_unacked.ResendTimer > 0) {
                _unacked.ResendTimer--;
            } else {
                _server.SendTo(_unacked.Packet, this.GetEndpoint());

                _unacked.ResendTimer = 60;
            }

            this._unacked[index] = _unacked;
            index++;
        }
        
        Flush();
    }

    public void Disconnect() {
        if (_disconnected) {
            return;
        }

        if (_clientHandler != null) {
            _clientHandler.OnDisconnect();
            
            _clientHandler.Delete();
            _clientHandler = null;
        }

        // Send a disconnect packet. Don't really care if they receive it.
        SendPacket(Packet.MakeDisconnectPacket());
        _disconnected = true;
    }

    public void ShowErrorMessage(string message) {
        SendPacket(Packet.MakeErrorMessagePacket(message));
    }
}

partial class Client {
    public class File(MPFileType fileType, List<byte> data) {
        public MPFileType FileType = fileType;
        public List<byte> Data = data;
    }
    
    private TcpClient? _tcpClient;
    private List<byte>? _downloadedFile;
    private bool _downloadedFileReady = false;
    private MPFileType _downloadedFileType;

    public File? GetDownloadedFile() {
        if (_downloadedFileReady && _downloadedFile != null) {
            _downloadedFileReady = false;
            return new File(_downloadedFileType, _downloadedFile);
        }

        return null;
    }

    public void SetDataClient(TcpClient tcpClient) {
        _tcpClient = tcpClient;
    }

    public void SendFile(File file) {
        Task.Run(() => {
            if (_tcpClient == null) {
                return;
            }

            try {

                Logger.Log($"Sending file {file.FileType} with {file.Data.Count / 1024}KB to {GetUsername()}");

                var packet = Packet.MakeFileUploadPacket(file.FileType, (uint)file.Data.Count);

                var (header, body) = packet.GetBytes(_endianness);

                var bytes = new List<byte>();
                bytes.AddRange(Packet.StructToBytes(header, _endianness));
                bytes.AddRange(body);

                _tcpClient.Client.Send(bytes.ToArray());

                int sent = 0;

                while (sent < file.Data.Count) {
                    int chunkSize = 4096;
                    if (chunkSize > file.Data.Count - sent) {
                        chunkSize = file.Data.Count - sent;
                    }

                    sent += _tcpClient.Client.Send(file.Data.Skip(sent).Take(chunkSize).ToArray());
                }

                Logger.Log($"Sent {file.Data.Count / 1024}KB to {GetUsername()}");
            } catch (SocketException e) {
                _disconnected = true;
                _tcpClient.Close();
            }
        });
    }

    public async Task ReceiveFromDataStream() {
        if (_tcpClient == null) {
            return;
        }

        while (!_disconnected) {
            var packetHeader = new byte[Marshal.SizeOf<MPPacketHeader>()];
            if (await _tcpClient.Client.ReceiveAsync(packetHeader) > 0 && Packet.MakeHeader(packetHeader) is {} header) {
                var bodyBytes = new byte[header.Size];
                if (await _tcpClient.Client.ReceiveAsync(bodyBytes) < 0) {
                    continue;
                }

                switch (header.PacketType) {
                    case MPPacketType.MP_PACKET_UPLOAD_FILE: {
                        if (Packet.BytesToStruct<MPPacketFileUpload>(bodyBytes, _endianness) is not { } fileUpload) {
                            continue;
                        }
                        
                        Logger.Log($"Receiving {fileUpload.FileSize/1024}KB file from {GetUsername()}");

                        var file = new byte[fileUpload.FileSize];
                        var received = 0;
                        var chunk = 4096;

                        do {
                            if (chunk > fileUpload.FileSize - received) {
                                chunk = (int)(fileUpload.FileSize - received);
                            }

                            var chunkArray = new byte[chunk];
                            
                            var r = await _tcpClient.Client.ReceiveAsync(chunkArray);
                            
                            Array.Copy(chunkArray, 0, file, received, r);

                            received += r;
                        } while (received < fileUpload.FileSize);

                        _downloadedFile = file.ToList();
                        _downloadedFileType = fileUpload.FileType;
                        _downloadedFileReady = true;
                        
                        Logger.Log("File received maybe successfully!");
                        
                        break;
                    }
                    default: {
                        Logger.Error($"{GetUsername()} sent invalid packet type {header.PacketType} over TCP.");
                        break;
                    }
                }
            }
        }

        Logger.Log($"Closing data stream for {GetUsername()}");
        _tcpClient.Close();
    }
}

public class NetworkParsingException(string message) : Exception(message) { }

#region Handling Mobys
partial class Client {
    private ushort _lastDeletedId = 0;
    
    private struct MobyData {
        public Guid Id;
        public long LastUpdate;
        public Moby MobyRef;
        public bool ClientCreated;
    }

    private readonly ConcurrentDictionary<Guid, ushort> _mobysTable = new();
    private readonly ConcurrentDictionary<ushort, MobyData> _mobys = new();
    
    private readonly ConcurrentDictionary<ushort, MobyData> _syncMobys = new();

    public ushort CreateSyncMoby(Moby moby) {
        // Find next available ID
        for (ushort i = 1; i <= 4096; i++) {
            if (!_syncMobys.ContainsKey(i)) {
                _syncMobys[i] = new MobyData { Id = moby.GUID(), LastUpdate = Game.Game.Shared().Ticks(), MobyRef = moby };
                
                return i;
            }
        }

        return 0;
    }
    
    public Moby? GetSyncMobyByInternalId(ushort internalId) {
        if (_syncMobys.TryGetValue((ushort)internalId, out var mobyData)) {
            return mobyData.MobyRef;
        }

        return null;
    }
    
    public void DeleteSyncMoby(ushort internalId) {
        if (_syncMobys.TryRemove(internalId, out var mobyData)) {
            mobyData.MobyRef.Delete();
        }
    }

    public void DeleteSyncMoby(Moby moby) {
        foreach (var pair in _syncMobys) {
            if (pair.Value.MobyRef == moby) {
                _syncMobys.TryRemove(pair.Key, out _);
                return;
            }
        }
    }

    public void UpdateMoby(Moby moby) {
        if (moby.SyncOwner == _clientHandler?.Moby()) {
            Logger.Error("We're trying to send updates to a player about their own synced moby.");
            return;
        }
        
        var internalId = GetOrCreateInternalId(moby);
        
        if (internalId == 0) {
            return;
        }
        
        if (!_mobys[internalId].ClientCreated) {
            ushort parentInternalId = 0;
            if (moby.AttachedTo != null) {
                parentInternalId = GetOrCreateInternalId(moby.AttachedTo);
                if (parentInternalId == 0) {
                    Logger.Error($"Player({_clientHandler?.Moby().GUID()}) trying to attach a moby to a parent that does not exist: {moby.AttachedTo.GUID()}");
                }
            }
            
            SendPacket(Packet.MakeCreateMobyPacket(internalId, moby, parentInternalId));
            _mobys[internalId] = new MobyData { Id = moby.GUID(), LastUpdate = Game.Game.Shared().Ticks(), MobyRef = moby, ClientCreated = false };
            SendPacket(Packet.MakeMobyUpdatePacket(internalId, moby));
            SendPacket(Packet.MakeMobyUpdateExtended(internalId, [new Packet.UpdateMobyValue(0x38, moby.Color.ToUInt())]));
        
            return;
        }

        _mobys[internalId] = new MobyData { Id = moby.GUID(), LastUpdate = Game.Game.Shared().Ticks(), MobyRef = moby, ClientCreated = true };
        SendPacket(Packet.MakeMobyUpdatePacket(internalId, moby));
        SendPacket(Packet.MakeMobyUpdateExtended(internalId, [new Packet.UpdateMobyValue(0x38, moby.Color.ToUInt())]));
    }
    
    public void ClearInternalMobyCache() {
        _mobys.Clear();
        _mobysTable.Clear();
    }

    private ushort GetOrCreateInternalId(Moby moby) {
        if (_mobysTable.TryGetValue(moby.GUID(), out var internalId)) {
            return internalId;
        }

        ushort parentInternalId = 0;
        if (moby.AttachedTo != null) {
            parentInternalId = GetOrCreateInternalId(moby.AttachedTo);
            if (parentInternalId == 0) {
                Logger.Error($"Player({_clientHandler?.Moby().GUID()}) trying to attach a moby to a parent that does not exist: {moby.AttachedTo.GUID()}");
                return 0;
            }
        }
        
        // Find next available ID
        for (ushort i = 1; i <= 4096; i++) {
            if (!_mobys.ContainsKey(i) && _lastDeletedId != i) {
                SendPacket(Packet.MakeCreateMobyPacket(i, moby, parentInternalId));
                
                _mobysTable[moby.GUID()] = i;
                _mobys[i] = new MobyData { Id = moby.GUID(), LastUpdate = Game.Game.Shared().Ticks(), MobyRef = moby };
                return i;
            }
        }

        return 0;
    }

    public ushort GetInternalIdForMoby(Moby moby) {
        if (_mobysTable.TryGetValue(moby.GUID(), out var internalId)) {
            return internalId;
        }

        return 0;
    }
    
    private ushort AssignInternalId(Moby moby) {
        if (_mobysTable.TryGetValue(moby.GUID(), out var internalId)) {
            return internalId;
        }
        
        // Find next available ID
        for (ushort i = 1; i <= 4096; i++) {
            if (!_mobys.ContainsKey(i) && _lastDeletedId != i) {
                _mobysTable[moby.GUID()] = i;
                _mobys[i] = new MobyData { Id = moby.GUID(), LastUpdate = Game.Game.Shared().Ticks(), MobyRef = moby };
                
                return i;
            }
        }

        return 0;
    }

    public Moby? GetMobyByInternalId(ushort internalId) {
        if (!_mobys.TryGetValue(internalId, out var mobyData)) {
            return null; // No Moby found with the given internalId.
        }

        if (_clientHandler == null) {
            return null;
        }

        // Check if Moby is stale.
        long currentTicks = Game.Game.Shared().Ticks();
        if (!_clientHandler.HasMoby(mobyData.MobyRef) && mobyData.LastUpdate < currentTicks - 120) {
            // Moby is stale, delete it and return null.
            DeleteMoby(mobyData.MobyRef);
            return null;
        }

        // Moby is not stale, return it.
        return mobyData.MobyRef;
    }

    public bool HasMoby(Moby moby) {
        return _mobysTable.TryGetValue(moby.GUID(), out _);
    }

    public void CleanupStaleMobys() {
        long currentTicks = Game.Game.Shared().Ticks();
        foreach (var pair in _mobys) {
            if (pair.Value.LastUpdate < currentTicks - 300) {
                Logger.Log($"Cleaning up stale moby with internal ID {pair.Key}.");
                
                if (pair.Value.MobyRef.SyncOwner != _clientHandler?.Moby() && pair.Value.ClientCreated) {
                    SendPacket(Packet.MakeDeleteMobyPacket(pair.Key));
                }

                _mobys.TryRemove(pair.Key, out _);
                _mobysTable.TryRemove(pair.Value.Id, out _);
            }
        }
    }

    public void DeleteMoby(Moby moby) {
        if (_mobysTable.TryGetValue(moby.GUID(), out var internalId)) {
            // If found, delete it from the game if our client is not the sync owner
            if (moby.SyncOwner != _clientHandler?.Moby() && _mobys[internalId].ClientCreated) {
                SendPacket(Packet.MakeDeleteMobyPacket(internalId));
            }

            // And remove it from our dictionaries
            _mobys.TryRemove(internalId, out _);
            _mobysTable.TryRemove(moby.GUID(), out _);
            _lastDeletedId = internalId;
        }
    }

    public void SetMobyCreated(ushort id) {
        if (_mobys.TryGetValue(id, out var mobyData)) {
            _mobys[id] = mobyData with { LastUpdate = Game.Game.Shared().Ticks(), ClientCreated = true };
        }
    }

    public void ClearMoby(ushort id) {
        if (_mobys.TryGetValue(id, out var mobyData)) {
            _mobysTable.TryRemove(mobyData.Id, out _);
            _mobys.TryRemove(id, out _);
        }
    }

    public string DumpMobys() {
        if (_clientHandler?.Moby() is Player player) {
            string dump = $"Player({player.Username()}) Mobys:";

            foreach (var pair in _mobys) {
                dump += "\n";

                var moby = pair.Value.MobyRef;
                var owner = (Player?)moby.AttachedTo;
                var ownerUsername = owner == null ? "None" : owner.Username();

                dump += $"\tInternal ID: {pair.Key}, GUID: {pair.Value.Id}, LastUpdate: {pair.Value.LastUpdate}\n";
                dump += $"\t\t- oClass {moby.oClass}\n";
                dump += $"\t\t- Owner: {ownerUsername}\n";
                dump += $"\t\t- SyncSpawnId: {moby.SyncSpawnId}\n";

                if (moby.AttachedTo != null) {
                    ushort attachedToId = 0;
                    _mobysTable.TryGetValue(moby.AttachedTo.GUID(), out attachedToId);
                    dump += $"\t\t- Attached to: {attachedToId}\n";

                    dump += $"\t\t\t- oClass: {moby.AttachedTo.oClass}\n";
                    dump += $"\t\t\t- SyncSpawnId: {moby.AttachedTo.SyncSpawnId}\n";
                }
            }

            return dump;
        }
        
        return "Invalid client handler or Player moby.";
    }
}
#endregion
