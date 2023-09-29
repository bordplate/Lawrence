using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Force.Crc32;

namespace Lawrence {
    // Metadata structure for packet acknowledgement data
    public struct AckedMetadata {
        public byte ackCycle;
        public byte ackIndex;
        public byte[] packet;
        public int resendTimer;
        public long timestamp;
    }

    public enum ControllerInput {
        L2 = 1,
        R2 = 2,
        L1 = 4,
        R1 = 8,
        Triangle = 16,
        Circle = 32,
        Cross = 64,
        Square = 128,
        Select = 256,
        L3 = 512,
        R3 = 1024,
        Start = 2048,
        Up = 4096,
        Right = 8192,
        Down = 16384,
        Left = 32768
    }

    public enum GameState {
        PlayerControl = 0x0,
        Movie = 0x1,
        CutScene = 0x2,
        Menu = 0x3,
        Prompt = 0x4,
        Vendor = 0x5,
        Loading = 0x6,
        Cinematic = 0x7,
        UnkFF = 0xff,
    }

    public interface IClientHandler {
        abstract uint CreateMoby();
        abstract void UpdateMoby(MPPacketMobyUpdate updatePacket);
        abstract void Collision(Moby collider, Moby collidee, bool aggressive = false);
        abstract void ControllerInputTapped(ControllerInput input);
        abstract void ControllerInputHeld(ControllerInput input);
        abstract void ControllerInputReleased(ControllerInput input);
        abstract void Delete();
        abstract Moby Moby();
        abstract void PlayerRespawned();
        abstract void GameStateChanged(GameState state);
        abstract void CollectedGoldBolt(int planet, int number);
        abstract void UnlockItem(int item);
    }

    public partial class Client {
        /// <summary>
        /// Packet types in this list are allowed to be sent before a handshake
        /// </summary>
        private static readonly List<MPPacketType> _allowedAnonymous = new List<MPPacketType> {
            MPPacketType.MP_PACKET_CONNECT,
            MPPacketType.MP_PACKET_SYN,
            MPPacketType.MP_PACKET_QUERY_GAME_SERVERS, // Only used in directory mode.
            MPPacketType.MP_PACKET_REGISTER_SERVER,
            MPPacketType.MP_PACKET_TIME_SYNC
        };
        
        // Which API version we're currently on and which is the minimum version we support. 
        uint API_VERSION = 1;
        uint API_VERSION_MIN = 1;
        
        /// <summary>
        /// When true, this client is waiting to connect, and is not yet part of the regular OnTick loop
        /// </summary>
        public bool WaitingToConnect = true;

        IClientHandler _clientHandler;

        private string _username = null;
        private int _userid = 0;
        
        IPEndPoint endpoint;
        bool handshakeCompleted = false;

        public uint ID = 0;

        long lastContact = 0;

        bool disconnected = true;

        public GameState gameState = (GameState)0;

        AckedMetadata[] acked = new AckedMetadata[256];

        byte ackCycle = 1;
        byte ackIndex = 1;
        List<AckedMetadata> unacked = new List<AckedMetadata>();

        public Client(IPEndPoint endpoint, uint ID) {
            this.ID = ID;
            this.endpoint = endpoint;
            this.lastContact = DateTimeOffset.Now.ToUnixTimeSeconds();
            this.disconnected = false;

            recvLock = new Mutex();
        }

        public void SetHandler(IClientHandler handler) {
            _clientHandler = handler;
        }

        public IPEndPoint GetEndpoint() {
            return endpoint;
        }

        public string GetUsername() {
            return _username;
        }

        public int GetUserid() {
            return _userid;
        }

        // Amount of seconds since we last saw activity on this client.
        public long GetInactiveSeconds() {
            return DateTimeOffset.Now.ToUnixTimeSeconds() - this.lastContact;
        }

        public int UnackedPacketsCount() {
            return unacked.Count;
        }

        public bool IsDisconnected() {
            return disconnected;
        }

        public bool IsActive() {
            return !disconnected && handshakeCompleted;
        }

        public uint GameState() {
            return (uint)gameState;
        }

        (byte, byte) NextAck() {
            if (ackIndex >= 254) {
                ackIndex = 0;
                ackCycle++;
            }

            if (ackCycle >= 254) {
                ackCycle = 0;
            }

            return (++ackIndex, ackCycle);
        }
        
        private List<byte> buffer = new List<byte>();
        private const int bufferSize = 1024;

        public void SendPacket(MPPacketHeader packetHeader, byte[] packetBody) {
            packetHeader.timeSent = (long)Game.Shared().Time();
    
            var bodyLen = 0;
            if (packetBody != null) {
                bodyLen = packetBody.Length;
            }
            byte[] packet = new byte[Marshal.SizeOf<MPPacketHeader>() + bodyLen];

            // Fill ack fields if necessary
            if (packetHeader.requiresAck == 255 && packetHeader.ackCycle == 255) {
                (packetHeader.requiresAck, packetHeader.ackCycle) = NextAck();
            }

            Packet.StructToBytes<MPPacketHeader>(packetHeader, Packet.Endianness.BigEndian).CopyTo(packet, 0);
            if (packetBody != null) {
                packetBody.CopyTo(packet, Marshal.SizeOf<MPPacketHeader>());
            }

            // Cache ack response packets
            if (packetHeader.ptype == MPPacketType.MP_PACKET_ACK && packetHeader.requiresAck != 0) {
                var ack = acked[packetHeader.requiresAck];
                if (ack.ackCycle != packetHeader.ackCycle) {
                    ack.ackCycle = packetHeader.ackCycle;
                    ack.packet = packet;
                    ack.resendTimer = 120;
                }

                acked[packetHeader.requiresAck] = ack;
            }

            // Cache unacked request packets
            if (packetHeader.ptype != MPPacketType.MP_PACKET_ACK && packetHeader.requiresAck != 0) {
                if (unacked.Count >= 256) {
                    //Console.WriteLine($"Player {ID} has more than 256 unacked packets. We should probably boot this client.");
                    unacked.Clear();
                }

                unacked.Add(new AckedMetadata { packet = packet, ackIndex = packetHeader.requiresAck, ackCycle = packetHeader.ackCycle, resendTimer = 30, timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() });
            }

            // Add packet to buffer
            buffer.AddRange(packet);

            // Check if buffer size reached
            if (buffer.Count >= bufferSize) {
                // Send the buffer
                Flush();
            }
        }
        
        public void Flush() {
            if (buffer.Count > 0) {
                Lawrence.SendTo(buffer.ToArray(), endpoint);
                buffer.Clear();
            }
        }

        public void SendPacket((MPPacketHeader packetHeader, byte[] packetBody) packet) {
            (MPPacketHeader header, byte[] body) = packet;

            SendPacket(header, body);
        }

        // Parse and process a packet
        public void ParsePacket(byte[] packet) {
            int index = 0;

            while (index < packet.Length && packet.Length - index >= Marshal.SizeOf<MPPacketHeader>()) {
                // Start out by reading the header
                MPPacketHeader packetHeader =
                    Packet.makeHeader(packet.Skip(index).Take(Marshal.SizeOf<MPPacketHeader>()).ToArray());
                index += Marshal.SizeOf<MPPacketHeader>();

                if (packetHeader.size > packet.Length - index) {
                    throw new Exception("Bad packet");
                }
                
                byte[] packetBody = packet.Skip(index).Take((int)packetHeader.size)
                    .ToArray();

                index += (int)packetHeader.size;

                // Check if handshake is complete, otherwise start completing it. 

                // TODO: Implement some sort of protection against anyone spoofing others.
                //       Ideally the handshake should start a session that the client can
                //          easily use to identify itself. Ideally without much computational
                //          overhead. 
                if ((!handshakeCompleted || WaitingToConnect) && !_allowedAnonymous.Contains(packetHeader.ptype)) {
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
                        acked[packetHeader.requiresAck].ackCycle == packetHeader.ackCycle) {
                        Lawrence.SendTo(acked[packetHeader.requiresAck].packet, endpoint);
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
                        MPPacketHeader responseHeader = new MPPacketHeader {
                            ptype = MPPacketType.MP_PACKET_ACK,
                            size = (uint)Marshal.SizeOf<MPPacketConnectResponse>(),
                            requiresAck = packetHeader.requiresAck,
                            ackCycle = packetHeader.ackCycle
                        };

                        if (packetSize < (uint)Marshal.SizeOf<MPPacketConnect>()) {
                            // Legacy client, we tell it to fuck off with an unknown error code, since it doesn't know about
                            //  the "outdated version" return code. 
                            Logger.Log("Legacy client tried to connect.");
                            
                            MPPacketConnectResponse response = new MPPacketConnectResponse {
                                status = MPPacketConnectResponseStatus.ERROR_UNKNOWN
                            };
                                    
                            SendPacket(responseHeader, Packet.StructToBytes(response, Packet.Endianness.BigEndian));
                            this.Disconnect();
                            return; 
                        }

                        // Decode the packet
                        MPPacketConnect connectPacket = Packet.BytesToStruct<MPPacketConnect>(packetBody, Packet.Endianness.BigEndian);
                        string username = connectPacket.GetUsername(packetBody);
                        
                        // Check that the API versions are compatible
                        if (connectPacket.version < API_VERSION_MIN) {
                            Logger.Log($"{username} tried to connect with old version {connectPacket.version}. Minimum version: {API_VERSION_MIN}. Latest: {API_VERSION}");
                            // Client is outdated, tell them to update.
                            MPPacketConnectResponse response = new MPPacketConnectResponse {
                                status = connectPacket.version == 0 ? 
                                    MPPacketConnectResponseStatus.ERROR_UNKNOWN : // Legacy alpha doesn't support ERROR_OUTDATED
                                    MPPacketConnectResponseStatus.ERROR_OUTDATED
                            };

                            SendPacket(responseHeader, Packet.StructToBytes(response, Packet.Endianness.BigEndian));
                            this.Disconnect();
                            return; 
                        }

                        foreach (Client c in Lawrence.GetClients()) {
                            if (c != this && c.GetUsername() == connectPacket.GetUsername(packetBody)) {
                                if (c.GetUserid() == connectPacket.userid && c.GetEndpoint().Address.Equals(GetEndpoint().Address)) {
                                    c.Disconnect();
                                    break;
                                } else {
                                    MPPacketConnectResponse response = new MPPacketConnectResponse {
                                        status = MPPacketConnectResponseStatus.ERROR_USER_ALREADY_CONNECTED
                                    };
                                    
                                    SendPacket(responseHeader, Packet.StructToBytes(response, Packet.Endianness.BigEndian));
                                    this.Disconnect();
                                    return; 
                                }
                            }
                        }

                        MPPacketConnectResponse responseBody = new MPPacketConnectResponse {
                            status = MPPacketConnectResponseStatus.SUCCESS
                        };

                        _username = username;
                        _userid = connectPacket.userid;
                        
                        SendPacket(responseHeader, Packet.StructToBytes(responseBody, Packet.Endianness.BigEndian));
                        
                        Game.Shared().OnPlayerConnect(this);

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
                    case MPPacketType.MP_PACKET_SYN: {
                        if (!handshakeCompleted) {
                            handshakeCompleted = true;

                            SendPacket(Packet.MakeAckPacket());
                        }
                        else {
                            SendPacket(
                                new MPPacketHeader
                                    { ptype = MPPacketType.MP_PACKET_ACK, ackCycle = 0, requiresAck = 0 }, null);
                        }

                        break;
                    }
                    case MPPacketType.MP_PACKET_ACK:
                        foreach (var unacked in this.unacked.ToArray()) {
                            if (unacked.ackCycle == packetHeader.ackCycle &&
                                unacked.ackIndex == packetHeader.requiresAck) {
                                this.unacked.Remove(unacked);
                                break;
                            }
                        }

                        break;
                    case MPPacketType.MP_PACKET_TIME_SYNC: {
                        MPPacketTimeResponse response = new MPPacketTimeResponse() {
                            clientSendTime = (ulong)packetHeader.timeSent,
                            serverSendTime = Game.Shared().Time()
                        };

                        MPPacketHeader header = new MPPacketHeader {
                            ptype = MPPacketType.MP_PACKET_ACK,
                            size = (uint)Marshal.SizeOf<MPPacketTimeResponse>(),
                            requiresAck = packetHeader.requiresAck,
                            ackCycle = packetHeader.ackCycle
                        };

                        SendPacket(header,
                            Packet.StructToBytes<MPPacketTimeResponse>(response, Packet.Endianness.BigEndian));

                        break;
                    }
                    case MPPacketType.MP_PACKET_MOBY_UPDATE: {
                        MPPacketMobyUpdate update =
                            Packet.BytesToStruct<MPPacketMobyUpdate>(packetBody, Packet.Endianness.BigEndian);

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
                            Packet.StructToBytes<MPPacketMobyCreate>(createPacket, Packet.Endianness.BigEndian));

                        break;
                    }
                    case MPPacketType.MP_PACKET_MOBY_COLLISION: {
                        MPPacketMobyCollision collision =
                            Packet.BytesToStruct<MPPacketMobyCollision>(packetBody, Packet.Endianness.BigEndian);

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
                            Packet.BytesToStruct<MPPacketSetState>(packetBody, Packet.Endianness.BigEndian);

                        if (state.stateType == MPStateType.MP_STATE_TYPE_GAME) {
                            gameState = (GameState)state.value;

                            _clientHandler.GameStateChanged(gameState);
                        }

                        if (state.stateType == MPStateType.MP_STATE_TYPE_COLLECTED_GOLD_BOLT) {
                            Logger.Log($"Player got bolt #{state.value}");
                            _clientHandler.CollectedGoldBolt((int)state.offset, (int)state.value);
                        }

                        if(state.stateType == MPStateType.MP_STATE_TYPE_UNLOCK_ITEM) {
                            Logger.Log($"Player got item #{state.value}");
                            _clientHandler.UnlockItem((int)state.value);
                        }

                        break;
                    }
                    case MPPacketType.MP_PACKET_QUERY_GAME_SERVERS: {
                        if (Lawrence.DirectoryMode()) {
                            List<Server> servers = Lawrence.Directory().Servers();

                            SendPacket(Packet.MakeQuerySerserResponsePacket(servers, packetHeader.requiresAck,
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
                                Packet.BytesToStruct<MPPacketRegisterServer>(packetBody, Packet.Endianness.BigEndian);

                            string name = serverInfo.GetName(packetBody);

                            uint ip = serverInfo.ip != 0 ? serverInfo.ip : (uint)this.GetEndpoint().Address.Address;

                            IPAddress address = new IPAddress(ip);

                            Lawrence.Directory().RegisterServer(address.ToString(), serverInfo.port, name, serverInfo.maxPlayers, serverInfo.playerCount);
                        }

                        break;
                    }
                    case MPPacketType.MP_PACKET_CONTROLLER_INPUT: {
                        // FIXME: Should react properly to held and released buttons. Only handles held and tapped buttons right now, not released buttons. 

                        MPPacketControllerInput input =
                            Packet.BytesToStruct<MPPacketControllerInput>(packetBody, Packet.Endianness.BigEndian);
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
                    default: {
                        Logger.Error(
                            $"(Player {ID}) sent unknown (possibly malformed) packet {packetHeader.ptype} with size: {packetSize}.");
                        break;
                    }
                }
            }
        }

        Mutex recvLock;
        bool resetBuffer = false;
        List<byte[]> recvBuffer = new List<byte[]>(400);
        Dictionary<uint, long> lastHashes = new Dictionary<uint, long>(10);
        public void ReceiveData(byte[] data) {
            if (disconnected) {
                return;
            }

            long timeNow = (long)Game.Shared().Time();

            this.lastContact = timeNow / 1000;

            recvLock.WaitOne();

            if (resetBuffer) {
                recvBuffer = new List<byte[]>(100);
            }

            uint currentHash = Crc32Algorithm.Compute(data);

            // Throw away duplicate packets in the last 200 milliseconds
            if (lastHashes.ContainsKey(currentHash) && lastHashes[currentHash] > timeNow - 200) {
                recvLock.ReleaseMutex();
                return;
            }

            // We store the crc32 sum of the last 10 packets
            // So we can discard future packets if they are the same sum
            // We send a lot of duplicate packets and we should be able to endure
            // any packet loss. So this should help us not process a bunch of redundant packets.
            if (lastHashes.Count >= 10) {
                lastHashes.Remove(0);
            }

            lastHashes[currentHash] = timeNow;

            recvBuffer.Add(data);

            recvLock.ReleaseMutex();

            if (recvBuffer.Count > 399) {
                recvBuffer = new List<byte[]>(400);
            }
        }

        List<byte[]> DrainPackets() {
            // We take at max 50 packets out of the buffer
            int takePackets = Math.Min(50, recvBuffer.Count);

            if (takePackets <= 0) {
                // No packets in buffer
                return null;
            }

            // Make sure the networking receive thread isn't working with the buffer
            recvLock.WaitOne();

            // Drain packets from buffer
            List<byte[]> packets = recvBuffer.Take(takePackets).ToList();
            recvBuffer.RemoveRange(0, takePackets);

            recvLock.ReleaseMutex();

            return packets;
        }

        public void Tick() {
            if (disconnected) {
                return;
            }

            Flush();

            var packets = DrainPackets();
            if (packets != null) {
                foreach (var packet in packets) {
                    try {
                        ParsePacket(packet);
                    } catch (Exception e) {
                        Logger.Error($"Encountered an exception in client", e);
                    }
                }
            }

            // Resend unacked packets
            int index = 0;
            long timeNow = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
            foreach (var unacked in this.unacked.ToArray()) {
                var _unacked = unacked;
                if (_unacked.resendTimer > 0) {
                    _unacked.resendTimer--;
                } else {
                    if (timeNow - unacked.timestamp > 10) {
                        Logger.Trace($"Player {ID} has stale packet they never ack, waited: {timeNow - unacked.timestamp}: ({unacked.ackIndex}/{unacked.ackCycle})");
                    }
                    
                    Lawrence.SendTo(_unacked.packet, this.GetEndpoint());

                    _unacked.resendTimer = 60;
                }

                this.unacked[index] = _unacked;
                index++;
            }
            
            Flush();
        }

        public void Disconnect() {
            if (disconnected) {
                return;
            }

            if (_clientHandler != null) {
                _clientHandler.Delete();
                _clientHandler = null;
            }

            // Send a disconnect packet. Don't really care if they receive it.
            SendPacket(Packet.MakeDisconnectPacket());
            disconnected = true;
        }
    }
    
    #region Handling Mobys
    partial class Client {
        private ushort lastDeletedId = 0;
        
        private struct MobyData {
            public Guid Id;
            public long LastUpdate;
            public Moby MobyRef;
        }

        private ConcurrentDictionary<Guid, ushort> _mobysTable = new ConcurrentDictionary<Guid, ushort>();
        private ConcurrentDictionary<ushort, MobyData> _mobys = new ConcurrentDictionary<ushort, MobyData>();

        public void UpdateMoby(Moby moby) {
            ushort internalId = GetOrCreateInternalId(moby);
            if (internalId == 0) {
                Logger.Error("A player has run out of moby space.");
                return;
            }

            _mobys[internalId] = new MobyData { Id = moby.GUID(), LastUpdate = Game.Shared().Ticks(), MobyRef = moby };
            SendPacket(Packet.MakeMobyUpdatePacket(internalId, moby));
        }

        private ushort GetOrCreateInternalId(Moby moby) {
            ushort internalId;
            if (_mobysTable.TryGetValue(moby.GUID(), out internalId)) {
                return internalId;
            }

            // Find next available ID
            for (ushort i = 1; i <= ushort.MaxValue; i++) {
                if (!_mobys.ContainsKey(i) && lastDeletedId != i) {
                    _mobysTable[moby.GUID()] = i;
                    return i;
                }
            }

            return 0;
        }

        private Moby GetMobyByInternalId(ushort internalId) {
            MobyData mobyData;
            if (!_mobys.TryGetValue(internalId, out mobyData)) {
                return null; // No Moby found with the given internalId.
            }

            // Check if Moby is stale.
            long currentTicks = Game.Shared().Ticks();
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
            long currentTicks = Game.Shared().Ticks();
            foreach (var pair in _mobys) {
                if (pair.Value.LastUpdate < currentTicks - 10) {
                    SendPacket(Packet.MakeDeleteMobyPacket(pair.Key));
                    _mobys.TryRemove(pair.Key, out _);
                    _mobysTable.TryRemove(pair.Value.Id, out _);
                }
            }
        }

        public void DeleteMoby(Moby moby) {
            ushort internalId;
            if (_mobysTable.TryGetValue(moby.GUID(), out internalId)) {
                // If found, delete it from the game
                SendPacket(Packet.MakeDeleteMobyPacket(internalId));
                // And remove it from our dictionaries
                _mobys.TryRemove(internalId, out _);
                _mobysTable.TryRemove(moby.GUID(), out _);
                lastDeletedId = internalId;
            } else {
                Logger.Error($"Trying to delete a moby that does not exist: {moby.GUID()}");
            }
        }
    }
    #endregion
}
