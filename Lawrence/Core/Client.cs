using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Force.Crc32;

using Lawrence.Game;

namespace Lawrence.Core;

// Metadata structure for packet acknowledgement data
public struct AckedMetadata {
    public byte AckCycle;
    public byte AckIndex;
    public byte[] Packet;
    public int ResendTimer;
    public long Timestamp;
}

public interface IClientHandler {
    uint CreateMoby();
    void UpdateMoby(MPPacketMobyUpdate updatePacket);
    void Collision(Moby collider, Moby collidee, bool aggressive = false);
    void ControllerInputTapped(ControllerInput input);
    void ControllerInputHeld(ControllerInput input);
    void ControllerInputReleased(ControllerInput input);
    void Delete();
    Moby Moby();
    void PlayerRespawned();
    void GameStateChanged(GameState state);
    void CollectedGoldBolt(int planet, int number);
    void UnlockItem(int item, bool equip);
    void OnUnlockLevel(int level);
    void OnUnlockSkillpoint(byte skillpoint);
    void OnDisconnect();
    void OnHybridMobyValueChange(ushort uid, MonitoredValueType type, ushort offset, ushort size, byte[] oldValue, byte[] newValue);
    void OnLevelFlagChanged(ushort type, byte level, byte size, ushort index, uint value);
}

public partial class Client {
    /// <summary>
    /// Packet types in this list are allowed to be sent before a handshake
    /// </summary>
    private static readonly List<MPPacketType> AllowedAnonymous = new List<MPPacketType> {
        MPPacketType.MP_PACKET_CONNECT,
        MPPacketType.MP_PACKET_SYN_LE,
        MPPacketType.MP_PACKET_SYN,
        MPPacketType.MP_PACKET_QUERY_GAME_SERVERS, // Only used in directory mode.
        MPPacketType.MP_PACKET_REGISTER_SERVER,
        MPPacketType.MP_PACKET_TIME_SYNC
    };
    
    // Which API version we're currently on and which is the minimum version we support. 
    uint API_VERSION = 4;
    uint API_VERSION_MIN = 4;
    
    /// <summary>
    /// When true, this client is waiting to connect, and is not yet part of the regular OnTick loop
    /// </summary>
    public bool WaitingToConnect = true;

    IClientHandler _clientHandler;

    private string _username = null;
    private int _userid = 0;

    readonly IPEndPoint _endpoint;
    bool _handshakeCompleted = false;

    public uint ID = 0;

    long _lastContact = 0;

    bool _disconnected = true;
    private bool _processedFirstPacket = false;
    
    private Packet.Endianness _endianness = Packet.Endianness.BigEndian;
    
    byte _ackCycle = 1;
    byte _ackIndex = 1;
    private readonly AckedMetadata[] _acked = new AckedMetadata[256];
    private readonly List<AckedMetadata> _unacked = new();

    private readonly Server _server;
    
    public Client(IPEndPoint endpoint, uint id, Server server) {
        this.ID = id;
        _endpoint = endpoint;
        _server = server;
        
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

    public string GetUsername() {
        return _username;
    }

    public int GetUserid() {
        return _userid;
    }

    // Amount of seconds since we last saw activity on this client.
    public long GetInactiveSeconds() {
        return DateTimeOffset.Now.ToUnixTimeSeconds() - this._lastContact;
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

    public void SendPacket(MPPacketHeader packetHeader, byte[] packetBody) {
        packetHeader.timeSent = (long)Game.Game.Shared().Time();

        var bodyLen = 0;
        if (packetBody != null) {
            bodyLen = packetBody.Length;
        }
        byte[] packet = new byte[Marshal.SizeOf<MPPacketHeader>() + bodyLen];

        // Fill ack fields if necessary
        if (packetHeader.requiresAck == 255 && packetHeader.ackCycle == 255) {
            (packetHeader.requiresAck, packetHeader.ackCycle) = NextAck();
        }

        Packet.StructToBytes(packetHeader, _endianness).CopyTo(packet, 0);
        if (packetBody != null) {
            packetBody.CopyTo(packet, Marshal.SizeOf<MPPacketHeader>());
        }

        // Cache ack response packets
        if (packetHeader.ptype == MPPacketType.MP_PACKET_ACK && packetHeader.requiresAck != 0) {
            var ack = _acked[packetHeader.requiresAck];
            if (ack.AckCycle != packetHeader.ackCycle) {
                ack.AckCycle = packetHeader.ackCycle;
                ack.Packet = packet;
                ack.ResendTimer = 120;
            }

            _acked[packetHeader.requiresAck] = ack;
        }

        // Cache unacked request packets
        if (packetHeader.ptype != MPPacketType.MP_PACKET_ACK && packetHeader.requiresAck != 0) {
            if (_unacked.Count >= 256) {
                //Console.WriteLine($"Player {ID} has more than 256 unacked packets. We should probably boot this client.");
                _unacked.Clear();
            }

            _unacked.Add(new AckedMetadata { Packet = packet, AckIndex = packetHeader.requiresAck, AckCycle = packetHeader.ackCycle, ResendTimer = 30, Timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() });
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

    // Parse and process a packet
    public void ParsePacket(byte[] packet) {
        int index = 0;

        while (index < packet.Length && packet.Length - index >= Marshal.SizeOf<MPPacketHeader>()) {
            // Start out by reading the header
            MPPacketHeader packetHeader =
                Packet.MakeHeader(packet.Skip(index).Take(Marshal.SizeOf<MPPacketHeader>()).ToArray(), _endianness);
            index += Marshal.SizeOf<MPPacketHeader>();

            if (packetHeader.size > packet.Length - index) {
                // Try to read in other endianness
                if (_endianness == Packet.Endianness.BigEndian) {
                    _endianness = Packet.Endianness.LittleEndian;
                }
                else {
                    _endianness = Packet.Endianness.BigEndian;
                }
                
                // Reset index
                index -= Marshal.SizeOf<MPPacketHeader>();
                packetHeader = Packet.MakeHeader(packet.Skip(index).Take(Marshal.SizeOf<MPPacketHeader>()).ToArray(), _endianness);
                
                if (packetHeader.size > packet.Length - index) {
                    // Still too big, throw exception
                    throw new Exception("Bad packet");
                }
            }
            
            byte[] packetBody = packet.Skip(index).Take((int)packetHeader.size)
                .ToArray();

            index += (int)packetHeader.size;

            // Check if handshake is complete, otherwise start completing it. 

            // TODO: Implement some sort of protection against anyone spoofing others.
            //       Ideally the handshake should start a session that the client can
            //          easily use to identify itself. Ideally without much computational
            //          overhead. 
            if ((!_handshakeCompleted || WaitingToConnect) && !AllowedAnonymous.Contains(packetHeader.ptype)) {
                // Client has sent a packet that is not a handshake packet.
                // We tell the client we don't know it and it should reset state and
                // start handshake. 
                SendPacket(new MPPacketHeader { ptype = MPPacketType.MP_PACKET_IDKU }, null);
                return;
            }

            // Get size of packet body 
            var packetSize = 0;
            if (packetHeader.size > 0 && packetHeader.size < 1024 * 8) {
                packetSize = (int)packetHeader.size;
            }

            // If this packet requires ack and is not RPC, we send ack before processing.
            // If this is RPC we only send ack here if a previous ack has been sent and cached.
            if (packetHeader.ptype != MPPacketType.MP_PACKET_ACK && packetHeader.requiresAck != 0) {
                // We don't ack ack messages
                // If this is an RPC packet and we've already processed and cached it, we use the cached response. 
                if ((packetHeader.flags & MPPacketFlags.MP_PACKET_FLAG_RPC) != 0 &&
                    _acked[packetHeader.requiresAck].AckCycle == packetHeader.ackCycle) {
                    _server.SendTo(_acked[packetHeader.requiresAck].Packet, _endpoint);
                }
                else if ((packetHeader.flags & MPPacketFlags.MP_PACKET_FLAG_RPC) == 0) {
                    // If it's not RPC, we just ack the packet and process the packet
                    MPPacketHeader ack = new MPPacketHeader {
                        ptype = MPPacketType.MP_PACKET_ACK,
                        flags = 0,
                        size = 0,
                        requiresAck = packetHeader.requiresAck,
                        ackCycle = packetHeader.ackCycle
                    };

                    SendPacket(ack, null);
                }
            }

            switch (packetHeader.ptype) {
                case MPPacketType.MP_PACKET_CONNECT: {
                    if (!WaitingToConnect) {
                        Console.Error.WriteLine("Player tried to connect twice.");
                        return;
                    }
                    
                    var responsePacket = new Packet(MPPacketType.MP_PACKET_ACK, packetHeader.requiresAck,
                        packetHeader.ackCycle);
                    
                    if (packetSize < (uint)Marshal.SizeOf<MPPacketConnect>()) {
                        // Legacy client, we tell it to fuck off with an unknown error code, since it doesn't know about
                        //  the "outdated version" return code. 
                        Logger.Log("Legacy client tried to connect.");
                        
                        MPPacketConnectResponse connectResponse = new MPPacketConnectResponse {
                            status = MPPacketConnectResponseStatus.ERROR_UNKNOWN
                        };
                        
                        responsePacket.AddBodyPart(connectResponse);
                        
                        SendPacket(responsePacket);
                        
                        Disconnect();
                        return; 
                    }

                    // Decode the packet
                    MPPacketConnect connectPacket = Packet.BytesToStruct<MPPacketConnect>(packetBody, _endianness);
                    string username = connectPacket.GetUsername(packetBody);
                    
                    // Check that the API versions are compatible
                    if (connectPacket.version < API_VERSION_MIN) {
                        Logger.Log($"{username} tried to connect with old version {connectPacket.version}. Minimum version: {API_VERSION_MIN}. Latest: {API_VERSION}");
                        // Client is outdated, tell them to update.
                        MPPacketConnectResponse connectResponse = new MPPacketConnectResponse {
                            status = connectPacket.version == 0 ? 
                                MPPacketConnectResponseStatus.ERROR_UNKNOWN : // Legacy alpha doesn't support ERROR_OUTDATED
                                MPPacketConnectResponseStatus.ERROR_OUTDATED
                        };
                        
                        responsePacket.AddBodyPart(connectResponse);

                        SendPacket(responsePacket);
                        
                        Disconnect();
                        
                        return; 
                    }

                    foreach (Client c in _server.Clients()) {
                        if (c != this && c.GetUsername() == connectPacket.GetUsername(packetBody)) {
                            if (c.GetUserid() == connectPacket.userid && c.GetEndpoint().Address.Equals(GetEndpoint().Address)) {
                                c.Disconnect();
                                break;
                            }
                        
                            MPPacketConnectResponse connectResponse = new MPPacketConnectResponse {
                                status = MPPacketConnectResponseStatus.ERROR_USER_ALREADY_CONNECTED
                            };
                            
                            responsePacket.AddBodyPart(connectResponse);
                            
                            SendPacket(responsePacket);
                            
                            Disconnect();
                            
                            return;
                        }
                    }

                    MPPacketConnectResponse responseBody = new MPPacketConnectResponse {
                        status = MPPacketConnectResponseStatus.SUCCESS
                    };
                    
                    responsePacket.AddBodyPart(responseBody);

                    _username = username;
                    _userid = connectPacket.userid;
                    
                    SendPacket(responsePacket);
                    
                    Game.Game.Shared().OnPlayerConnect(this);

                    WaitingToConnect = false;

                    Logger.Log($"New player {username} connected!");
                    Lawrence.ForceDirectorySync();

                    break;
                }
                case MPPacketType.MP_PACKET_DISCONNECTED: {
                    Disconnect();

                    Logger.Log("Player disconnected.");
                    Lawrence.ForceDirectorySync();

                    break;
                }
                case MPPacketType.MP_PACKET_SYN_LE:
                    if (!_processedFirstPacket) {
                        _endianness = Packet.Endianness.LittleEndian;

                        Logger.Log($"Player is little endian.");

                        goto case MPPacketType.MP_PACKET_SYN;
                    }
                    
                    Logger.Log("Player tried to change endianness after handshake.");
                    
                    Disconnect();

                    break;
                case MPPacketType.MP_PACKET_SYN: {
                    var response = Packet.MakeAckPacket();
                    
                    if (!_handshakeCompleted) {
                        _handshakeCompleted = true;
                    }
                    else {
                        response.Header.ackCycle = 0;
                        response.Header.requiresAck = 0;
                    }

                    SendPacket(response);

                    break;
                }
                case MPPacketType.MP_PACKET_ACK:
                    foreach (var unacked in _unacked.ToArray()) {
                        if (unacked.AckCycle == packetHeader.ackCycle &&
                            unacked.AckIndex == packetHeader.requiresAck) {
                            _unacked.Remove(unacked);
                            break;
                        }
                    }

                    break;
                case MPPacketType.MP_PACKET_TIME_SYNC: {
                    MPPacketTimeResponse response = new MPPacketTimeResponse() {
                        clientSendTime = (ulong)packetHeader.timeSent,
                        serverSendTime = Game.Game.Shared().Time()
                    };

                    MPPacketHeader header = new MPPacketHeader {
                        ptype = MPPacketType.MP_PACKET_ACK,
                        size = (uint)Marshal.SizeOf<MPPacketTimeResponse>(),
                        requiresAck = packetHeader.requiresAck,
                        ackCycle = packetHeader.ackCycle
                    };

                    SendPacket(header,
                        Packet.StructToBytes(response, _endianness));

                    break;
                }
                case MPPacketType.MP_PACKET_MOBY_UPDATE: {
                    MPPacketMobyUpdate update =
                        Packet.BytesToStruct<MPPacketMobyUpdate>(packetBody, _endianness);

                    _clientHandler.UpdateMoby(update);

                    break;
                }
                case MPPacketType.MP_PACKET_MOBY_CREATE: {
                    uint createdUUID = _clientHandler.CreateMoby();

                    MPPacketMobyCreate createPacket = new MPPacketMobyCreate {
                        uuid = createdUUID
                    };

                    MPPacketHeader header = new MPPacketHeader {
                        ptype = MPPacketType.MP_PACKET_ACK,
                        flags = MPPacketFlags.MP_PACKET_FLAG_RPC,
                        size = (uint)Marshal.SizeOf<MPPacketMobyCreate>(),
                        requiresAck = packetHeader.requiresAck,
                        ackCycle = packetHeader.ackCycle
                    };

                    Logger.Log($"Player({this.ID}) created moby (uuid: {createdUUID})");

                    SendPacket(header,
                        Packet.StructToBytes(createPacket, _endianness));

                    break;
                }
                case MPPacketType.MP_PACKET_MOBY_COLLISION: {
                    MPPacketMobyCollision collision =
                        Packet.BytesToStruct<MPPacketMobyCollision>(packetBody, _endianness);

                    Moby collider = collision.uuid == 0
                        ? _clientHandler.Moby()
                        : GetMobyByInternalId(collision.uuid);

                    Moby collidee = collision.collidedWith == 0
                        ? _clientHandler.Moby()
                        : GetMobyByInternalId(collision.collidedWith);

                    _clientHandler.Collision(collider, collidee, collision.flags > 0);
                    break;
                }
                case MPPacketType.MP_PACKET_SET_STATE: {
                    MPPacketSetState state =
                        Packet.BytesToStruct<MPPacketSetState>(packetBody, _endianness);

                    if (state.stateType == MPStateType.MP_STATE_TYPE_GAME) {
                        _clientHandler.GameStateChanged((GameState)state.value);
                    }

                    if (state.stateType == MPStateType.MP_STATE_TYPE_COLLECTED_GOLD_BOLT) {
                        Logger.Log($"Player got bolt #{state.value}");
                        _clientHandler.CollectedGoldBolt((int)state.offset, (int)state.value);
                    }

                    if (state.stateType == MPStateType.MP_STATE_TYPE_UNLOCK_ITEM) {
                        // TODO: Clean up this bitwise magic into readable flags
                        uint item = state.value & 0xFFFF;
                        bool equip = (state.value >> 16) == 1;
                        
                        Logger.Log($"Player got item #{item}: equip: {equip}");
                        _clientHandler.UnlockItem((int)item, equip);
                    }

                    if (state.stateType == MPStateType.MP_STATE_TYPE_UNLOCK_LEVEL) {
                        Logger.Log($"Player unlocked level #{state.value}");
                        _clientHandler.OnUnlockLevel((int)state.value);
                    }

                    if (state.stateType == MPStateType.MP_STATE_TYPE_UNLOCK_SKILLPOINT) {
                        Logger.Log($"Player unlocked skillpoint #{state.value}");
                        _clientHandler.OnUnlockSkillpoint((byte)state.value);
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_QUERY_GAME_SERVERS: {
                    if (Lawrence.DirectoryMode()) {
                        List<ServerItem> servers = Lawrence.Directory().Servers();

                        SendPacket(Packet.MakeQueryServerResponsePacket(servers, packetHeader.requiresAck,
                            packetHeader.ackCycle));
                    }
                    else {
                        Logger.Error(
                            $"(Player {ID}) tried to query us as directory, but we're not a directory server.");
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_REGISTER_SERVER: {
                    if (Lawrence.DirectoryMode()) {
                        MPPacketRegisterServer serverInfo =
                            Packet.BytesToStruct<MPPacketRegisterServer>(packetBody, _endianness);

                        string name = serverInfo.GetName(packetBody);

                        uint ip = serverInfo.ip != 0 ? serverInfo.ip : (uint)GetEndpoint().Address.Address;

                        IPAddress address = new IPAddress(ip);

                        Lawrence.Directory().RegisterServer(address.ToString(), serverInfo.port, name, serverInfo.maxPlayers, serverInfo.playerCount);
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_CONTROLLER_INPUT: {
                    // FIXME: Should react properly to held and released buttons. Only handles held and tapped buttons right now, not released buttons. 

                    MPPacketControllerInput input =
                        Packet.BytesToStruct<MPPacketControllerInput>(packetBody, _endianness);
                    if ((input.flags & MPControllerInputFlags.MP_CONTROLLER_FLAGS_HELD) != 0) {
                        _clientHandler.ControllerInputHeld((ControllerInput)input.input);
                    }

                    if ((input.flags & MPControllerInputFlags.MP_CONTROLLER_FLAGS_PRESSED) != 0) {
                        ControllerInput pressedButtons = (ControllerInput)input.input;

                        _clientHandler.ControllerInputTapped(pressedButtons);
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_PLAYER_RESPAWNED: {
                    _clientHandler.PlayerRespawned();
                    break;
                }
                case MPPacketType.MP_PACKET_MONITORED_VALUE_CHANGED: {
                    var valueChanged = Packet.BytesToStruct<MPPacketMonitoredValueChanged>(packetBody, _endianness);
                    _clientHandler.OnHybridMobyValueChange(
                        valueChanged.uid,
                        valueChanged.flags == 1 ? MonitoredValueType.Attribute : MonitoredValueType.PVar,
                        valueChanged.offset,
                        valueChanged.size,
                        BitConverter.GetBytes(valueChanged.oldValue),
                        BitConverter.GetBytes(valueChanged.newValue)
                    );
                    
                    break;
                }
                case MPPacketType.MP_PACKET_LEVEL_FLAG_CHANGED: {
                    var flagChanged = Packet.BytesToStruct<MPPacketLevelFlagChanged>(packetBody, _endianness);
                    _clientHandler.OnLevelFlagChanged(
                        flagChanged.type,
                        flagChanged.level,
                        flagChanged.size,
                        flagChanged.index,
                        flagChanged.value
                    );
                    break;
                }
                default: {
                    Logger.Error(
                        $"(Player {ID}) sent unknown (possibly malformed) packet {packetHeader.ptype} with size: {packetSize}.");
                    break;
                }
            }
        }
    }

    private readonly Mutex _recvLock;
    private readonly bool _resetBuffer = false;
    private List<byte[]> _recvBuffer = new(400);
    private readonly Dictionary<uint, long> _lastHashes = new(10);
    public void ReceiveData(byte[] data) {
        if (_disconnected) {
            return;
        }

        long timeNow = (long)Game.Game.Shared().Time();

        _lastContact = timeNow / 1000;

        _recvLock.WaitOne();

        if (_resetBuffer) {
            _recvBuffer = new List<byte[]>(100);
        }

        uint currentHash = Crc32Algorithm.Compute(data);

        // Throw away duplicate packets in the last 200 milliseconds
        if (_lastHashes.ContainsKey(currentHash) && _lastHashes[currentHash] > timeNow - 200) {
            _recvLock.ReleaseMutex();
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

        _recvBuffer.Add(data);

        _recvLock.ReleaseMutex();

        if (_recvBuffer.Count > 399) {
            _recvBuffer = new List<byte[]>(400);
        }
    }

    private List<byte[]> DrainPackets() {
        // We take at max 50 packets out of the buffer
        int takePackets = Math.Min(50, _recvBuffer.Count);

        if (takePackets <= 0) {
            // No packets in buffer
            return null;
        }

        // Make sure the networking receive thread isn't working with the buffer
        _recvLock.WaitOne();

        // Drain packets from buffer
        List<byte[]> packets = _recvBuffer.Take(takePackets).ToList();
        _recvBuffer.RemoveRange(0, takePackets);

        _recvLock.ReleaseMutex();

        return packets;
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

#region Handling Mobys
partial class Client {
    private ushort _lastDeletedId = 0;
    
    private struct MobyData {
        public Guid Id;
        public long LastUpdate;
        public Moby MobyRef;
    }

    private readonly ConcurrentDictionary<Guid, ushort> _mobysTable = new ConcurrentDictionary<Guid, ushort>();
    private readonly ConcurrentDictionary<ushort, MobyData> _mobys = new ConcurrentDictionary<ushort, MobyData>();

    public void UpdateMoby(Moby moby) {
        var internalId = GetOrCreateInternalId(moby);
        
        if (internalId == 0) {
            Logger.Error("A player has run out of moby space.");
            return;
        }

        _mobys[internalId] = new MobyData { Id = moby.GUID(), LastUpdate = Game.Game.Shared().Ticks(), MobyRef = moby };
        SendPacket(Packet.MakeMobyUpdatePacket(internalId, moby));
        SendPacket(Packet.MakeMobyUpdateExtended(internalId, new [] { new Packet.UpdateMobyValue(0x38, moby.Color.ToUInt()) }));
    }

    private ushort GetOrCreateInternalId(Moby moby) {
        if (_mobysTable.TryGetValue(moby.GUID(), out var internalId)) {
            return internalId;
        }

        // Find next available ID
        for (ushort i = 1; i <= 4096; i++) {
            if (!_mobys.ContainsKey(i) && _lastDeletedId != i) {
                _mobysTable[moby.GUID()] = i;
                return i;
            }
        }

        return 0;
    }

    private Moby GetMobyByInternalId(ushort internalId) {
        if (!_mobys.TryGetValue(internalId, out var mobyData)) {
            return null; // No Moby found with the given internalId.
        }

        // Check if Moby is stale.
        long currentTicks = Game.Game.Shared().Ticks();
        if (mobyData.LastUpdate < currentTicks - 120) {
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
            if (pair.Value.LastUpdate < currentTicks - 10) {
                SendPacket(Packet.MakeDeleteMobyPacket(pair.Key));
                _mobys.TryRemove(pair.Key, out _);
                _mobysTable.TryRemove(pair.Value.Id, out _);
            }
        }
    }

    public void DeleteMoby(Moby moby) {
        if (_mobysTable.TryGetValue(moby.GUID(), out var internalId)) {
            // If found, delete it from the game
            SendPacket(Packet.MakeDeleteMobyPacket(internalId));
            // And remove it from our dictionaries
            _mobys.TryRemove(internalId, out _);
            _mobysTable.TryRemove(moby.GUID(), out _);
            _lastDeletedId = internalId;
        } else {
            Logger.Error($"Trying to delete a moby that does not exist: {moby.GUID()}");
        }
    }
}
#endregion
