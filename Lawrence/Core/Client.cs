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
public struct AckedMetadata {
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
    private uint API_VERSION = 11;
    private uint API_VERSION_MIN = 11;
    
    /// <summary>
    /// When true, this client is waiting to connect, and is not yet part of the regular OnTick loop
    /// </summary>
    public bool WaitingToConnect = true;

    public uint? DataStreamKey;

    IClientHandler? _clientHandler;

    private string? _username = null;
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
            var ack = _acked[packetHeader.RequiresAck];
            if (ack.AckCycle != packetHeader.AckCycle) {
                ack.AckCycle = packetHeader.AckCycle;
                ack.Packet = packet;
                ack.ResendTimer = 120;
            }

            _acked[packetHeader.RequiresAck] = ack;
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
            
            byte[] packetBody = packet.Skip(index).Take((int)packetHeader.Size)
                .ToArray();

            index += (int)packetHeader.Size;
            
            if (packetHeader.TimeSent < _lastTimeSent) {
                // This packet is older than the last one we received. We don't want to process it.
                Logger.Log($"Player({ID}) sent an old packet. Ignoring.");
                continue;
            }
            
            _lastTimeSent = packetHeader.TimeSent;

            // Check if handshake is complete, otherwise start completing it. 

            // TODO: Implement some sort of protection against anyone spoofing others.
            //       Ideally the handshake should start a session that the client can
            //          easily use to identify itself. Ideally without much computational
            //          overhead. 
            if ((!_handshakeCompleted || WaitingToConnect) && !AllowedAnonymous.Contains(packetHeader.PacketType)) {
                // Client has sent a packet that is not a handshake packet.
                // We tell the client we don't know it and it should reset state and
                // start handshake. 
                SendPacket(new MPPacketHeader { PacketType = MPPacketType.MP_PACKET_IDKU }, null);
                return;
            }

            // Get size of packet body 
            var packetSize = 0;
            if (packetHeader.Size > 0 && packetHeader.Size < 1024 * 8) {
                packetSize = (int)packetHeader.Size;
            }

            // If this packet requires ack and is not RPC, we send ack before processing.
            // If this is RPC we only send ack here if a previous ack has been sent and cached.
            if (packetHeader.PacketType != MPPacketType.MP_PACKET_ACK && packetHeader.RequiresAck != 0) {
                // We don't ack ack messages
                // If this is an RPC packet and we've already processed and cached it, we use the cached response. 
                if ((packetHeader.Flags & MPPacketFlags.MP_PACKET_FLAG_RPC) != 0 &&
                    _acked[packetHeader.RequiresAck].AckCycle == packetHeader.AckCycle) {
                    _server.SendTo(_acked[packetHeader.RequiresAck].Packet, _endpoint);
                } else if ((packetHeader.Flags & MPPacketFlags.MP_PACKET_FLAG_RPC) == 0) {
                    // If it's not RPC, we just ack the packet and process the packet
                    MPPacketHeader ack = new MPPacketHeader {
                        PacketType = MPPacketType.MP_PACKET_ACK,
                        Flags = 0,
                        Size = 0,
                        RequiresAck = packetHeader.RequiresAck,
                        AckCycle = packetHeader.AckCycle
                    };

                    SendPacket(ack, null);
                }
            }

            switch (packetHeader.PacketType) {
                case MPPacketType.MP_PACKET_CONNECT: {
                    if (!WaitingToConnect) {
                        Logger.Error("Player tried to connect twice.");
                        return;
                    }
                    
                    var responsePacket = new Packet(MPPacketType.MP_PACKET_ACK, packetHeader.RequiresAck,
                        packetHeader.AckCycle);
                    
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
                    if (Packet.BytesToStruct<MPPacketConnect>(packetBody, _endianness) is not { } connectPacket) {
                        throw new NetworkParsingException("Failed to parse connect packet.");
                    }
                    
                    string username = connectPacket.GetUsername(packetBody);
                    
                    // Check that the API versions are compatible
                    if (connectPacket.Version < API_VERSION_MIN) {
                        Logger.Log($"{username} tried to connect with old version {connectPacket.Version}. Minimum version: {API_VERSION_MIN}. Latest: {API_VERSION}");
                        // Client is outdated, tell them to update.
                        MPPacketConnectResponse connectResponse = new MPPacketConnectResponse {
                            status = connectPacket.Version == 0 ? 
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
                            if (c.GetUserid() == connectPacket.UserId && c.GetEndpoint().Address.Equals(GetEndpoint().Address)) {
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
                    _userid = connectPacket.UserId;
                    
                    SendPacket(responsePacket);
                    
                    Game.Game.Shared().OnPlayerConnect(this);

                    WaitingToConnect = false;

                    Logger.Log($"New player {username} connected!");
                    Lawrence.ForceDirectorySync();

                    DataStreamKey = (uint)Guid.NewGuid().GetHashCode();
                    SendPacket(Packet.MakeOpenDataStreamPacket(DataStreamKey.Value));

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
                        response.Header.AckCycle = 0;
                        response.Header.RequiresAck = 0;
                    }

                    SendPacket(response);

                    break;
                }
                case MPPacketType.MP_PACKET_ACK:
                    foreach (var unacked in _unacked.ToArray()) {
                        if (unacked.AckCycle == packetHeader.AckCycle &&
                            unacked.AckIndex == packetHeader.RequiresAck) {
                            _unacked.Remove(unacked);
                            unacked.AckCallback?.Invoke();
                            break;
                        }
                    }

                    break;
                case MPPacketType.MP_PACKET_TIME_SYNC: {
                    MPPacketTimeResponse response = new MPPacketTimeResponse() {
                        ClientSendTime = (ulong)packetHeader.TimeSent,
                        ServerSendTime = Server.Time()
                    };

                    MPPacketHeader header = new MPPacketHeader {
                        PacketType = MPPacketType.MP_PACKET_ACK,
                        Size = (uint)Marshal.SizeOf<MPPacketTimeResponse>(),
                        RequiresAck = packetHeader.RequiresAck,
                        AckCycle = packetHeader.AckCycle
                    };

                    SendPacket(header,
                        Packet.StructToBytes(response, _endianness));

                    break;
                }
                case MPPacketType.MP_PACKET_MOBY_UPDATE: {
                    if (Packet.BytesToStruct<MPPacketMobyUpdate>(packetBody, _endianness) is { } update) {
                        _clientHandler?.UpdateMoby(update);
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_MOBY_CREATE: {
                    if (Packet.BytesToStruct<MPPacketMobyCreate>(packetBody, _endianness) is not { } create) {
                        throw new NetworkParsingException("Failed to parse moby create packet.");
                    }
                    
                    var newMoby = _clientHandler?.CreateMoby(create.OClass, create.SpawnId);
                    
                    if (newMoby == null) {
                        Logger.Error($"Client({ID}) failed to create moby [oClass:{create.OClass}].");
                        return;
                    }
                    
                    newMoby.modeBits = create.ModeBits;

                    if (create.Flags.HasFlag(MPMobyFlags.MP_MOBY_FLAG_ATTACHED_TO)) {
                        var parent = create.ParentUuid == 0 ?
                            _clientHandler?.Moby() :
                            GetSyncMobyByInternalId(create.ParentUuid);
                        
                        if (parent != null) {
                            newMoby.AttachedTo = parent;
                            newMoby.PositionBone = create.PositionBone;
                            newMoby.TransformBone = create.TransformBone;

                            if (create.OClass == 173) {
                                newMoby.PositionBone = (byte)(create.PositionBone == 6 ? 22 : 23);
                                newMoby.TransformBone = (byte)(create.PositionBone == 6 ? 22 : 23);
                            }
                        } else {
                            Logger.Error($"Player({ID}) tried to attach moby [oClass:{create.OClass}] to a parent [{create.ParentUuid}] that doesn't exist.");
                        }
                    }

                    if (_clientHandler?.Moby() is { } moby) {
                        newMoby.MakeSynced(moby);
                    }

                    var internalId = CreateSyncMoby(newMoby);
                    
                    SendPacket(Packet.MakeMobyCreateResponsePacket(internalId, packetHeader.RequiresAck, packetHeader.AckCycle));
                    
                    // Logger.Log($"Player({ID}) created moby (oClass: {create.OClass}) with internal ID {internalId}.");

                    break;
                }
                case MPPacketType.MP_PACKET_MOBY_CREATE_FAILURE: {
                    if (Packet.BytesToStruct<MPPacketMobyCreateFailure>(packetBody, _endianness) is not {} createFailure) {
                        throw new NetworkParsingException("Failed to parse moby create failure packet.");
                    }

                    switch (createFailure.Reason) {
                        case MPMobyCreateFailureReason.UNKNOWN:
                            Logger.Error($"Couldn't create moby for Player({ID}): unknown error.");
                            break;
                        case MPMobyCreateFailureReason.NOT_READY: return;
                        case MPMobyCreateFailureReason.ALREADY_EXISTS:
                            Logger.Error($"Couldn't create moby for Player({ID}): already exists.");
                            break;
                        case MPMobyCreateFailureReason.MAX_MOBYS:
                            Logger.Error($"Couldn't create moby for Player({ID}): out of moby space.");
                            break;
                        case MPMobyCreateFailureReason.UPDATE_NON_EXISTENT:
                            Logger.Error($"Couldn't create moby for Player({ID}): tried to update a moby that doesn't exist.");
                            break;
                        case MPMobyCreateFailureReason.SUCCESS:
                            SetMobyCreated(createFailure.Uuid);
                            return;
                    }
                    
                    ClearMoby(createFailure.Uuid);
                    
                    break;
                }
                case MPPacketType.MP_PACKET_MOBY_DELETE: {
                    if (Packet.BytesToStruct<MPPacketMobyDelete>(packetBody, _endianness) is not { } delete) {
                        throw new NetworkParsingException("Failed to parse moby delete packet.");
                    }

                    var moby = GetSyncMobyByInternalId((ushort)delete.Uuid);
                    if (moby != null) {
                        _clientHandler?.DeleteMoby(moby);
                    } else {
                        Logger.Error($"Player [{ID}] tried to delete a moby that doesn't exist in its moby table.");
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_MOBY_DAMAGE: {
                    if (Packet.BytesToStruct<MPPacketMobyDamage>(packetBody, _endianness) is not {} damage) {
                        throw new NetworkParsingException("Failed to parse moby damage packet.");
                    }

                    var collider = damage.Uuid == 0
                        ? _clientHandler?.Moby()
                        : GetMobyByInternalId(damage.Uuid);

                    var collidee = damage.CollidedWithUuid == 0
                        ? _clientHandler?.Moby()
                        : GetMobyByInternalId(damage.CollidedWithUuid);

                    if (!damage.Flags.HasFlag(MPDamageFlags.GameMoby)) {
                        // This is an attack against a server-spawned moby, like another player or other entity.
                        if (collider is {} _ && collidee is {} __) {
                            _clientHandler?.OnDamage(collider, collidee, damage.SourceOClass, damage.Damage);
                        }
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_SET_STATE: {
                    if (Packet.BytesToStruct<MPPacketSetState>(packetBody, _endianness) is not { } state) {
                        throw new NetworkParsingException("Failed to parse set state packet.");
                    }

                    if (state.StateType == MPStateType.MP_STATE_TYPE_GAME) {
                        _clientHandler?.GameStateChanged((GameState)state.Value);
                    }

                    if (state.StateType == MPStateType.MP_STATE_TYPE_COLLECTED_GOLD_BOLT) {
                        _clientHandler?.CollectedGoldBolt((int)state.Offset, (int)state.Value);
                    }

                    if (state.StateType == MPStateType.MP_STATE_TYPE_UNLOCK_ITEM) {
                        // TODO: Clean up this bitwise magic into readable flags
                        uint item = state.Value & 0xFFFF;
                        bool equip = (state.Value >> 16) == 1;
                        
                        _clientHandler?.UnlockItem((int)item, equip);
                    }

                    if (state.StateType == MPStateType.MP_STATE_TYPE_UNLOCK_LEVEL) {
                        _clientHandler?.OnUnlockLevel((int)state.Value);
                    }

                    if (state.StateType == MPStateType.MP_STATE_TYPE_GIVE_BOLTS) {
                        _clientHandler?.OnGiveBolts((int)state.Value, state.Offset);
                    }
                        
                    if (state.StateType == MPStateType.MP_STATE_TYPE_UNLOCK_SKILLPOINT) {
                        _clientHandler?.OnUnlockSkillpoint((byte)state.Value);
                    }

                    if (state.StateType == MPStateType.MP_STATE_START_IN_LEVEL_MOVIE) {
                        _clientHandler?.OnStartInLevelMovie(state.Value, state.Offset);
                    }

                    if (state.StateType == MPStateType.MP_STATE_STANDING_ON_MOBY) {
                        Moby? moby = null;
                        if (state.Value > 0) {
                            moby = GetMobyByInternalId((ushort)state.Value);
                        }
                        
                        _clientHandler?.OnPlayerStandingOnMoby(moby);
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_QUERY_GAME_SERVERS: {
                    if (Lawrence.DirectoryMode()) {
                        if (packetHeader.Size < Marshal.SizeOf<MPPacketQueryGameServers>()) {
                            Logger.Error(
                                $"(Player {ID}) tried to query us with outdated version.");

                            packetHeader.PacketType = MPPacketType.MP_PACKET_ACK;
                            packetHeader.Size = 0x2d;
                            packetHeader.TimeSent = (long)Server.Time();
                            
                            // We didn't use to have a version field on the query packet, and also no way to tell a
                            // querying client that it is outdated. The byte array is a query response with 1 server named
                            // "Your multiplayer mod is outdated." for the old client. 
                            SendPacket(packetHeader, [
                                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x21,0x59,0x6f,0x75,0x72,0x20,
                                0x6d,0x75,0x6c,0x74,0x69,0x70,0x6c,0x61,0x79,0x65,0x72,0x20,0x6d,0x6f,0x64,0x20,0x69,
                                0x73,0x20,0x6f,0x75,0x74,0x64,0x61,0x74,0x65,0x64,0x2e
                            ]);
                            
                            return;
                        }

                        if (Lawrence.Directory()?.Servers() is { } servers) {
                            SendPacket(Packet.MakeQueryServerResponsePacket(servers, packetHeader.RequiresAck,
                                packetHeader.AckCycle));
                        }
                    }
                    else {
                        Logger.Error(
                            $"(Player {ID}) tried to query us as directory, but we're not a directory server.");
                    }

                    Disconnect();

                    break;
                }
                case MPPacketType.MP_PACKET_REGISTER_SERVER: {
                    if (Lawrence.DirectoryMode()) {
                        if (Packet.BytesToStruct<MPPacketRegisterServer>(packetBody, _endianness) is not
                            { } serverInfo) {
                            throw new NetworkParsingException("Failed to parse register server packet.");
                        }

                        string name = serverInfo.GetName(packetBody);

                        uint ip = serverInfo.Ip != 0 ? serverInfo.Ip : (uint)GetEndpoint().Address.Address;

                        IPAddress address = new IPAddress(ip);

                        Lawrence.Directory()?.RegisterServer(
                            address.ToString(), 
                            serverInfo.Port, 
                            name, 
                            serverInfo.MaxPlayers, 
                            serverInfo.PlayerCount, 
                            serverInfo.GetDescription(packetBody), 
                            serverInfo.GetOwner(packetBody)
                        );
                    }
                    
                    Disconnect();

                    break;
                }
                case MPPacketType.MP_PACKET_CONTROLLER_INPUT: {
                    // FIXME: Should react properly to held and released buttons. Only handles held and tapped buttons right now, not released buttons.
                    if (Packet.BytesToStruct<MPPacketControllerInput>(packetBody, _endianness) is not { } input) {
                        throw new NetworkParsingException("Failed to parse controller input packet.");
                    }
                    
                    if ((input.Flags & MPControllerInputFlags.MP_CONTROLLER_FLAGS_HELD) != 0) {
                        _clientHandler?.ControllerInputHeld((ControllerInput)input.Input);
                    }

                    if ((input.Flags & MPControllerInputFlags.MP_CONTROLLER_FLAGS_PRESSED) != 0) {
                        ControllerInput pressedButtons = (ControllerInput)input.Input;

                        _clientHandler?.ControllerInputTapped(pressedButtons);
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_PLAYER_RESPAWNED: {
                    if (Packet.BytesToStruct<MPPacketSpawned>(packetBody, _endianness) is { } spawned) {
                        _clientHandler?.PlayerRespawned(spawned.SpawnId, spawned.LevelId);
                    }

                    break;
                }
                case MPPacketType.MP_PACKET_MONITORED_VALUE_CHANGED: {
                    if (Packet.BytesToStruct<MPPacketMonitoredValueChanged>(packetBody, _endianness) is not {} valueChanged) {
                        throw new NetworkParsingException("Failed to parse monitored value changed packet.");
                    }
                    
                    _clientHandler?.OnHybridMobyValueChange(
                        valueChanged.Uid,
                        valueChanged.Flags == 1 ? MonitoredValueType.Attribute : MonitoredValueType.PVar,
                        valueChanged.Offset,
                        valueChanged.Size,
                        BitConverter.GetBytes(valueChanged.OldValue),
                        BitConverter.GetBytes(valueChanged.NewValue)
                    );
                    
                    break;
                }
                case MPPacketType.MP_PACKET_MONITORED_ADDRESS_CHANGED: {
                    if (Packet.BytesToStruct<MPPacketMonitoredAddressChanged>(packetBody, _endianness) is not
                        { } addressChanged) {
                        throw new NetworkParsingException("Failed to parse monitored address changed packet.");
                    }
                    
                    _clientHandler?.OnMonitoredAddressChanged(
                        addressChanged.Address,
                        (byte)addressChanged.Size,
                        BitConverter.GetBytes(addressChanged.OldValue),
                        BitConverter.GetBytes(addressChanged.NewValue)
                    );
                    break;
                }
                case MPPacketType.MP_PACKET_LEVEL_FLAG_CHANGED: {
                    if (Packet.BytesToStruct<MPPacketLevelFlagsChanged>(packetBody, _endianness) is not {} flagChanged) {
                        throw new NetworkParsingException("Failed to parse level flag changed packet.");
                    }

                    for (var i = 0; i < flagChanged.Flags; i++) {
                        var offset = Marshal.SizeOf<MPPacketLevelFlagsChanged>() + Marshal.SizeOf<MPPacketLevelFlag>() * i;
                        if (Packet.BytesToStruct<MPPacketLevelFlag>(packetBody.Skip(offset).ToArray(), _endianness) is not {} levelFlag) {
                            throw new NetworkParsingException($"Failed to parse level flag {i} of {flagChanged.Flags}.");
                        }
                        
                        _clientHandler?.OnLevelFlagChanged(
                            flagChanged.Type,
                            flagChanged.Level,
                            levelFlag.Size,
                            levelFlag.Index,
                            levelFlag.Value
                        );
                    }
                    
                    break;
                }
                case MPPacketType.MP_PACKET_UI_EVENT: {
                    if (Packet.BytesToStruct<MPPacketUIEvent>(packetBody, _endianness) is not {} uiEvent) {
                        throw new NetworkParsingException("Failed to parse UI event packet.");
                    }
                    
                    byte[] extraData = uiEvent.GetExtraData(packetBody);

                    _clientHandler?.UIEvent(uiEvent.EventType, uiEvent.ElementId, uiEvent.Data, extraData);
                    
                    break;
                }
                default: {
                    Logger.Error(
                        $"(Player {ID}) sent unknown (possibly malformed) packet {packetHeader.PacketType} with size: {packetSize}.");
                    break;
                }
            }
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
