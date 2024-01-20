using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Lawrence
{
    public enum MPPacketType : ushort
    {
        MP_PACKET_CONNECT = 1,      // Not used
        MP_PACKET_SYN = 2,
        MP_PACKET_ACK = 3,
        MP_PACKET_MOBY_UPDATE = 4,
        MP_PACKET_MOBY_EXTENDED = 18,
        MP_PACKET_IDKU = 5,
        MP_PACKET_MOBY_CREATE = 6,
        MP_PACKET_DISCONNECTED = 7,
        MP_PACKET_MOBY_DELETE = 8,
        MP_PACKET_MOBY_COLLISION = 9,
        MP_PACKET_SET_STATE = 10,
        MP_PACKET_SET_HUD_TEXT = 11,
        MP_PACKET_QUERY_GAME_SERVERS = 12,
        MP_PACKET_CONTROLLER_INPUT = 13,
        MP_PACKET_TIME_SYNC = 14,
        MP_PACKET_PLAYER_RESPAWNED = 15,
        MP_PACKET_REGISTER_SERVER = 16,
        MP_PACKET_TOAST_MESSAGE = 17,
    }

    public enum MPStateType : uint
    {
        MP_STATE_TYPE_DAMAGE = 1,
        MP_STATE_TYPE_PLAYER =  2,
        MP_STATE_TYPE_POSITION = 3,
        MP_STATE_TYPE_PLANET = 4,
        MP_STATE_TYPE_GAME = 5,
        MP_STATE_TYPE_ITEM = 6,
        MP_STATE_TYPE_SET_RESPAWN = 7,
        MP_STATE_TYPE_COLLECTED_GOLD_BOLT = 8,
        MP_STATE_TYPE_BLOCK_GOLD_BOLT = 9,
        MP_STATE_TYPE_PLAYER_INPUT = 10,
        MP_STATE_TYPE_ARBITRARY = 11,
        MP_STATE_TYPE_UNLOCK_ITEM = 12,
        MP_STATE_TYPE_GIVE_BOLTS = 13,
        MP_STATE_TYPE_UNLOCK_LEVEL = 14
    }

    public enum MPPacketFlags : ushort
    {
        MP_PACKET_FLAG_RPC = 0x1
    }

    public enum MPMobyFlags : ushort
    {
        MP_MOBY_FLAG_ACTIVE = 0x1,
        MP_MOBY_NO_COLLISION = 0x2,
        MP_MOBY_FLAG_ORIG_UDPATE_FUNC = 0x4,
    }

    public enum MPControllerInputFlags : ushort
    {
        MP_CONTROLLER_FLAGS_PRESSED = 0x1,
        MP_CONTROLLER_FLAGS_HELD = 0x2,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketHeader
    {
        public MPPacketType ptype;
        public MPPacketFlags flags;
        public UInt32 size;
        public long timeSent;
        public byte requiresAck;
        public byte ackCycle;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketConnect {
        public Int32 userid;
        public UInt32 version;
        public byte passcode1;
        public byte passcode2;
        public byte passcode3;
        public byte passcode4;
        public byte passcode5;
        public byte passcode6;
        public byte passcode7;
        public byte passcode8;
        public UInt16 nickLength;

        public string GetUsername(byte[] packetBody) {
            byte[] usernameBytes = packetBody.Skip(Marshal.SizeOf(this)).ToArray();
            
            return Encoding.ASCII.GetString(usernameBytes);
        }
    }

    public enum MPPacketConnectResponseStatus : Int32 {
        ERROR_UNKNOWN = 0,
        SUCCESS = 1,
        ERROR_USER_ALREADY_CONNECTED = 2,
        ERROR_NOT_ALLOWED = 3,
        ERROR_OUTDATED = 4,
        ERROR_WRONG_PASSCODE = 5
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketConnectResponse {
        public MPPacketConnectResponseStatus status;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketMobyUpdate
    {
        [FieldOffset(0x0)] public ushort uuid;
        [FieldOffset(0x2)] public ushort parent;
        [FieldOffset(0x4)] public MPMobyFlags mpFlags;
        [FieldOffset(0x6)] public ushort oClass;
        [FieldOffset(0x8)] public ushort level;
        [FieldOffset(0xa)] public Int32 animationID;
        [FieldOffset(0xe)] public Int32 animationDuration;
        [FieldOffset(0x12)] public float x;
        [FieldOffset(0x16)] public float y;
        [FieldOffset(0x1a)] public float z;
        [FieldOffset(0x1e)] public float rotX;
        [FieldOffset(0x22)] public float rotY;
        [FieldOffset(0x26)] public float rotZ;
        [FieldOffset(0x2a)] public float scale;
        [FieldOffset(0x2e)] public byte alpha;
        [FieldOffset(0x2f)] public sbyte padding;
        [FieldOffset(0x30)] public ushort modeBits;
        [FieldOffset(0x32)] public ushort state;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketMobyExtended {
        public UInt16 uuid;
        public UInt16 numValues;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketMobyExtendedPayload {
        public UInt16 offset;
        public UInt32 value;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketMobyCreate
    {
        public UInt32 uuid;
        public UInt32 flags;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketMobyCollision
    {
        [FieldOffset(0x0)] public uint flags;
        [FieldOffset(0x4)] public ushort uuid;
        [FieldOffset(0x6)] public ushort collidedWith;
        [FieldOffset(0x8)] public float x;
        [FieldOffset(0xc)] public float y;
        [FieldOffset(0x10)] public float z;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketSetState
    {
        public MPStateType stateType;
        public uint offset;
        public uint value;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketSetStateFloat
    {
        public MPStateType stateType;
        public uint offset;
        public float value;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketBolts
    {
        public MPStateType stateType;
        public uint value;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketSetHUDText
    {
        public ushort id;
        public ushort x;
        public ushort y;
        public ushort flags;
        public uint color;
        public ushort box_height;
        public ushort box_width;
        public float size;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketQueryResponseServer
    {
        [FieldOffset(0x0)] public uint ip;
        [FieldOffset(0x4)] public ushort port;
        [FieldOffset(0x6)] public ushort maxPlayers;
        [FieldOffset(0x8)] public ushort playerCount;
        [FieldOffset(0xa)] public ushort nameLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MPPacketRegisterServer {
        public uint ip;
        public ushort port;
        public ushort maxPlayers;
        public ushort playerCount;
        public ushort nameLength;
        
        public string GetName(byte[] packetBody) {
            byte[] nameBytes = packetBody.Skip(Marshal.SizeOf(this)).ToArray();
            
            return Encoding.UTF8.GetString(nameBytes);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketControllerInput
    {
        [FieldOffset(0x0)] public ushort input;
        [FieldOffset(0x2)] public MPControllerInputFlags flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack =  1)]
    public struct MPPacketTimeResponse {
        public ulong clientSendTime;
        public ulong serverSendTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MPPacketToastMessage {
        public UInt32 messageType;
        public UInt32 duration;
    }

    public class Packet
    {
        public Packet()
        {
        }

        public static (MPPacketHeader, byte[]) MakeAckPacket()
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_ACK;

            return (header, null);
        }

        public static byte[] MakeIDKUPacket()
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_IDKU;

            return headerToBytes(header);
        }

        public static (MPPacketHeader, byte[]) MakeDisconnectPacket()
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_DISCONNECTED;
            header.size = 0;

            return (header, null);
        }

        public static (MPPacketHeader, byte[]) MakeDamagePacket(uint damage)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetState bonkState = new MPPacketSetState();
            bonkState.stateType = MPStateType.MP_STATE_TYPE_PLAYER;
            bonkState.value = 0x16;

            // MPPacketSetState damageState = new MPPacketSetState();
            // damageState.stateType = MPStateType.MP_STATE_TYPE_DAMAGE;
            // damageState.value = damage;  // Damage player by 1 health

            var size = Marshal.SizeOf(bonkState);// + Marshal.SizeOf(damageState);
            header.size = (uint)size;
            List<byte> bytes = new List<byte>();
            
            bytes.AddRange(StructToBytes<MPPacketSetState>(bonkState, Endianness.BigEndian));
            // bytes.AddRange(StructToBytes<MPPacketSetState>(damageState, Endianness.BigEndian));

            return (header, bytes.ToArray());
        }

        public static (MPPacketHeader, byte[]) MakeSetHUDTextPacket(ushort id, string text, ushort x, ushort y, uint color, uint states)
        {
            if (text.Length >= 50)
            {
                return (new MPPacketHeader(), null);
            }

            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_HUD_TEXT;
            header.requiresAck = 255;
            header.ackCycle = 255;

            // hudText.flags field contains multiple separate pieces of information. Each next one is shifted left until they don't interfere with the previous one
            uint TextElementFlag = 1; // <2 bits> Drop shadow
            uint FlagsSetFlag = 1 << 2; // <1 bit>
            uint GameStateFlags = states << 3; // <8 bits>

            MPPacketSetHUDText hudText = new MPPacketSetHUDText();
            hudText.x = x;
            hudText.y = y;
            hudText.color = color;
            hudText.flags = (ushort)(TextElementFlag | GameStateFlags | FlagsSetFlag);
            hudText.id = id;

            header.size = (uint)Marshal.SizeOf(hudText) + 50;

            //List<byte> buffer = new List<byte>((int)header.size);
            byte[] buffer = new byte[(int)header.size];
            StructToBytes<MPPacketSetHUDText>(hudText, Endianness.BigEndian).ToList().CopyTo(buffer, 0);
            //text.ToList().CopyTo(buffer, Marshal.SizeOf(hudText));
            Encoding.ASCII.GetBytes(text).CopyTo(buffer, Marshal.SizeOf(hudText));
            
            return (header, buffer);
        }
        // TODO: change flag settings to reserve 2 bits for the current drop_shadow/delete options (since they are 1 and 2).
        // TODO: change flag settings to then reserve 3 bits for game state.
        // TODO: add game state argument for make to say on which gamestate it should be shown/should be moved to. maybe default it to something
        public static (MPPacketHeader, byte[]) MakeDeleteHUDTextPacket(ushort id)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_HUD_TEXT;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetHUDText hudText = new MPPacketSetHUDText();
            hudText.id = id;
            hudText.flags = 2;  // Delete

            header.size = (uint)Marshal.SizeOf(hudText) + 50;

            byte[] buffer = new byte[(int)header.size];
            StructToBytes<MPPacketSetHUDText>(hudText, Endianness.BigEndian).ToList().CopyTo(buffer, 0);


            return (header, buffer);
        }

        public static (MPPacketHeader, byte[]) MakeDeleteMobyPacket(ushort mobyUUID)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_MOBY_DELETE;
            header.requiresAck = 255;  // 255 represents unfilled fields that the client will fill before sending
            header.ackCycle = 255; 

            MPPacketMobyCreate body = new MPPacketMobyCreate();
            body.uuid = mobyUUID;
            body.flags = 1;

            header.size = (uint)Marshal.SizeOf<MPPacketMobyCreate>();

            return (header, StructToBytes<MPPacketMobyCreate>(body, Endianness.BigEndian));
        }
        
        public static (MPPacketHeader, byte[]) MakeDeleteAllMobysPacket(ushort oClass)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_MOBY_DELETE;
            header.requiresAck = 255;  // 255 represents unfilled fields that the client will fill before sending
            header.ackCycle = 255; 

            MPPacketMobyCreate body = new MPPacketMobyCreate();
            body.uuid = oClass;
            body.flags = 2;

            header.size = (uint)Marshal.SizeOf<MPPacketMobyCreate>();

            return (header, StructToBytes<MPPacketMobyCreate>(body, Endianness.BigEndian));
        }
        
        public static (MPPacketHeader, byte[]) MakeDeleteAllMobysUIDPacket(ushort uid)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_MOBY_DELETE;
            header.requiresAck = 255;  // 255 represents unfilled fields that the client will fill before sending
            header.ackCycle = 255; 

            MPPacketMobyCreate body = new MPPacketMobyCreate();
            body.uuid = uid;
            body.flags = 8;

            header.size = (uint)Marshal.SizeOf<MPPacketMobyCreate>();

            return (header, StructToBytes<MPPacketMobyCreate>(body, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeMobyUpdatePacket(ushort id, Moby moby)
        {
            MPPacketMobyUpdate moby_update = new MPPacketMobyUpdate();

            moby_update.uuid = id;

            moby_update.mpFlags |= moby.IsActive() ? MPMobyFlags.MP_MOBY_FLAG_ACTIVE : 0;
            moby_update.mpFlags |= moby.collision ? 0 : MPMobyFlags.MP_MOBY_NO_COLLISION;
            moby_update.mpFlags |= moby.mpUpdateFunc ? 0 : MPMobyFlags.MP_MOBY_FLAG_ORIG_UDPATE_FUNC;
            
            moby_update.parent = (ushort)0; // Parent isn't really used
            moby_update.oClass = (ushort)moby.oClass;
            moby_update.level = moby.Level() != null ? (ushort)moby.Level().GetGameID() : (ushort)0;
            moby_update.x = moby.x;
            moby_update.y = moby.y;
            moby_update.z = moby.z;
            moby_update.rotX = (float)(Math.PI / 180) * moby.rotX;
            moby_update.rotY = (float)(Math.PI / 180) * moby.rotY;
            moby_update.rotZ = (float)(Math.PI / 180) * moby.rotZ;
            moby_update.animationID = moby.animationID;
            moby_update.scale = moby.scale;
            moby_update.alpha = Math.Min((byte)(moby.alpha * 128), (byte)128);
            
            moby_update.modeBits = moby.modeBits;

            MPPacketHeader moby_header = new MPPacketHeader { ptype = MPPacketType.MP_PACKET_MOBY_UPDATE, size = (uint)Marshal.SizeOf<MPPacketMobyUpdate>() };

            return (moby_header, Packet.StructToBytes<MPPacketMobyUpdate>(moby_update, Endianness.BigEndian));
        }

        public struct UpdateMobyValue {
            public UInt16 offset;
            public uint value;
            
            public UpdateMobyValue(UInt16 offset, uint value) { this.offset = offset; this.value = value; }
        }

        public static (MPPacketHeader, byte[]) MakeMobyUpdateExtended(ushort uuid, UpdateMobyValue[] values) {
            MPPacketMobyExtended moby_extended = new MPPacketMobyExtended();
            
            moby_extended.uuid = uuid;
            moby_extended.numValues = (ushort)values.Length;
            
            MPPacketHeader moby_header = new MPPacketHeader { ptype = MPPacketType.MP_PACKET_MOBY_EXTENDED, size = (uint)Marshal.SizeOf<MPPacketMobyExtended>() + (uint)(Marshal.SizeOf<MPPacketMobyExtendedPayload>() * values.Length) };
            
            byte[] buffer = new byte[moby_header.size];
            StructToBytes<MPPacketMobyExtended>(moby_extended, Endianness.BigEndian).ToList().CopyTo(buffer, 0);
            
            int offset = Marshal.SizeOf<MPPacketMobyExtended>();
            foreach (UpdateMobyValue value in values) {
                MPPacketMobyExtendedPayload payload = new MPPacketMobyExtendedPayload();
                payload.offset = value.offset;
                payload.value = value.value;
                
                StructToBytes<MPPacketMobyExtendedPayload>(payload, Endianness.BigEndian).ToList().CopyTo(buffer, offset);
                offset += Marshal.SizeOf<MPPacketMobyExtendedPayload>();
            }
            
            return (moby_header, buffer);
        }

        public static (MPPacketHeader, byte[]) MakeGoToLevelPacket(int level)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetState destinationLevelState = new MPPacketSetState();
            destinationLevelState.stateType = MPStateType.MP_STATE_TYPE_PLANET;
            destinationLevelState.value = (uint)level;

            var size = Marshal.SizeOf(destinationLevelState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(destinationLevelState, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeQueryServerResponsePacket(List<Server> servers, byte ackId, byte ackCycle)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_ACK;
            header.requiresAck = ackId;
            header.ackCycle = ackCycle;

            List<byte> bytes = new List<byte>();

            foreach (Server server in servers)
            {
                IPAddress ipAddress;
                if (IPAddress.TryParse(server.IP, out ipAddress))
                {
                    // If the parsing is successful, get the 32-bit integer representation of the IP address
                    uint addr = SwapEndianness((uint)ipAddress.Address);

                    MPPacketQueryResponseServer packet = new MPPacketQueryResponseServer();
                    packet.ip = addr;
                    packet.port = (ushort)server.Port;
                    packet.maxPlayers = (ushort)server.MaxPlayers;
                    packet.playerCount = (ushort)server.PlayerCount;
                    packet.nameLength = (ushort)Encoding.UTF8.GetBytes(server.Name).Length;

                    bytes.AddRange(Packet.StructToBytes(packet, Endianness.BigEndian));
                    bytes.AddRange(Encoding.UTF8.GetBytes(server.Name));
                }
            }

            header.size = (uint)bytes.Count;

            return (header, bytes.ToArray());
        }

        public static (MPPacketHeader, byte[]) MakeRegisterServerPacket(string ip, ushort port, ushort maxPlayers,
            ushort playerCount, string name) {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_REGISTER_SERVER;

            List<byte> bytes = new List<byte>();

            IPAddress address;
            if (!IPAddress.TryParse(ip, out address)) {
                throw new Exception($"Invalid IP address {ip}");
            }

            MPPacketRegisterServer packet = new MPPacketRegisterServer();
            packet.ip = (uint)address.MapToIPv4().Address;
            packet.port = port;
            packet.maxPlayers = maxPlayers;
            packet.playerCount = playerCount;
            packet.nameLength = (ushort)name.Length;
            
            bytes.AddRange(Packet.StructToBytes(packet, Endianness.BigEndian));
            bytes.AddRange(Encoding.UTF8.GetBytes(name));

            header.size = (uint)bytes.Count;

            return (header, bytes.ToArray());
        }

        public static (MPPacketHeader, byte[]) MakeSetItemPacket(ushort item, bool equip)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            uint flags = 0;
            flags |= 1;  // FLAG_ITEM_GIVE
            flags |= equip ? (uint)2 : (uint)0;  // FLAG_ITEM_EQUIP

            MPPacketSetState setItemState = new MPPacketSetState();
            setItemState.stateType = MPStateType.MP_STATE_TYPE_ITEM;
            setItemState.value = (flags << 16) | (uint)item;

            var size = Marshal.SizeOf(setItemState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(setItemState, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeUnlockLevelPacket(int level)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetState setItemState = new MPPacketSetState();
            setItemState.stateType = MPStateType.MP_STATE_TYPE_UNLOCK_LEVEL;
            setItemState.value = (uint)level;

            var size = Marshal.SizeOf(setItemState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(setItemState, Endianness.BigEndian));
        }
        
        public static (MPPacketHeader, byte[]) MakeSetPlayerStatePacket(ushort state)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetState setPlayerState = new MPPacketSetState();
            setPlayerState.stateType = MPStateType.MP_STATE_TYPE_PLAYER;
            setPlayerState.value = state;

            var size = Marshal.SizeOf(setPlayerState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(setPlayerState, Endianness.BigEndian));
        }
        
        public static (MPPacketHeader, byte[]) MakeSetPlayerInputStatePacket(uint state)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetState setPlayerState = new MPPacketSetState();
            setPlayerState.stateType = MPStateType.MP_STATE_TYPE_PLAYER_INPUT;
            setPlayerState.value = state;

            var size = Marshal.SizeOf(setPlayerState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(setPlayerState, Endianness.BigEndian));
        }
        
        public static (MPPacketHeader, byte[]) MakeSetAddressValuePacket(uint address, uint value)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetState setPlayerState = new MPPacketSetState();
            setPlayerState.stateType = MPStateType.MP_STATE_TYPE_ARBITRARY;
            setPlayerState.offset = address;
            setPlayerState.value = value;

            var size = Marshal.SizeOf(setPlayerState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(setPlayerState, Endianness.BigEndian));
        }
        
        public static (MPPacketHeader, byte[]) MakeSetAddressFloatPacket(uint address, float value)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetStateFloat setPlayerState = new MPPacketSetStateFloat();
            setPlayerState.stateType = MPStateType.MP_STATE_TYPE_ARBITRARY;
            setPlayerState.offset = address;
            setPlayerState.value = value;

            var size = Marshal.SizeOf(setPlayerState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetStateFloat>(setPlayerState, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeGiveBoltsPacket(uint bolts)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketBolts giveBolts = new MPPacketBolts();
            giveBolts.stateType = MPStateType.MP_STATE_TYPE_GIVE_BOLTS;
            giveBolts.value = bolts;

            var size = Marshal.SizeOf(giveBolts);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketBolts>(giveBolts, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeSetPositionPacket(ushort property, float position) {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetStateFloat setPositionState = new MPPacketSetStateFloat();
            setPositionState.stateType = MPStateType.MP_STATE_TYPE_POSITION;

            setPositionState.offset = property;
            setPositionState.value = position;

            header.size = (uint)Marshal.SizeOf(setPositionState);

            return (header, StructToBytes(setPositionState, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeSetRespawnPacket(float x, float y, float z, float rotationZ) {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetStateFloat setRespawnX = new MPPacketSetStateFloat();
            setRespawnX.stateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN;
            setRespawnX.offset = 0;
            setRespawnX.value = x;
            
            MPPacketSetStateFloat setRespawnY = new MPPacketSetStateFloat();
            setRespawnY.stateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN;
            setRespawnY.offset = 1;
            setRespawnY.value = y;

            MPPacketSetStateFloat setRespawnZ = new MPPacketSetStateFloat();
            setRespawnZ.stateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN;
            setRespawnZ.offset = 2;
            setRespawnZ.value = z;
            
            MPPacketSetStateFloat setRespawnRotZ = new MPPacketSetStateFloat();
            setRespawnRotZ.stateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN;
            setRespawnRotZ.offset = 5;
            setRespawnRotZ.value = rotationZ;
            
            List<byte> bytes = new List<byte>();
            
            bytes.AddRange(StructToBytes(setRespawnX, Endianness.BigEndian));
            bytes.AddRange(StructToBytes(setRespawnY, Endianness.BigEndian));
            bytes.AddRange(StructToBytes(setRespawnZ, Endianness.BigEndian));
            bytes.AddRange(StructToBytes(setRespawnRotZ, Endianness.BigEndian));

            header.size = (uint)bytes.Count;
            
            return (header, bytes.ToArray());
        }

        public static (MPPacketHeader, byte[]) MakeToastMessagePacket(string message, uint duration = 20) {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_TOAST_MESSAGE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketToastMessage messagePacket = new MPPacketToastMessage();
            messagePacket.messageType = 0;
            messagePacket.duration = duration;

            header.size = (uint)Marshal.SizeOf(messagePacket) + 0x50;

            byte[] buffer = new byte[(int)header.size];
            StructToBytes<MPPacketToastMessage>(messagePacket, Endianness.BigEndian).ToList().CopyTo(buffer, 0);
            Encoding.ASCII.GetBytes(message).CopyTo(buffer, 8);

            return (header, buffer);
        }
        
        public static (MPPacketHeader, byte[]) MakeBlockGoldBoltPacket(int level, int number)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetState blockGoldBolt = new MPPacketSetState();
            blockGoldBolt.stateType = MPStateType.MP_STATE_TYPE_BLOCK_GOLD_BOLT;
            blockGoldBolt.value = (ushort)number;
            blockGoldBolt.offset = (ushort)level;

            var size = Marshal.SizeOf(blockGoldBolt);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(blockGoldBolt, Endianness.BigEndian));
        }


        public static byte[] headerToBytes(MPPacketHeader header)
        {
            return Packet.StructToBytes<MPPacketHeader>(header, Endianness.BigEndian);
        }

        public static MPPacketHeader makeHeader(byte[] bytes)
        {
            return Packet.BytesToStruct<MPPacketHeader>(bytes, Endianness.BigEndian);
        }

        public enum Endianness
        {
            BigEndian,
            LittleEndian
        }

        private static void MaybeAdjustEndianness(Type type, byte[] data, Endianness endianness, int startOffset = 0)
        {
            if ((BitConverter.IsLittleEndian) == (endianness == Endianness.LittleEndian))
            {
                // nothing to change => return
                return;
            }

            foreach (var field in type.GetFields())
            {
                var fieldType = field.FieldType;
                if (field.IsStatic)
                    // don't process static fields
                    continue;

                if (fieldType == typeof(string))
                    // don't swap bytes for strings
                    continue;

                var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

                // handle enums
                if (fieldType.IsEnum)
                    fieldType = Enum.GetUnderlyingType(fieldType);

                // check for sub-fields to recurse if necessary
                var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

                var effectiveOffset = startOffset + offset;

                if (subFields.Length == 0/* && offset <= Marshal.SizeOf(fieldType)*/)
                {
                    Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
                }
                else
                {
                    // recurse
                    MaybeAdjustEndianness(fieldType, data, endianness, effectiveOffset);
                }
            }
        }

        internal static T BytesToStruct<T>(byte[] rawData, Endianness endianness) where T : struct
        {
            T result = default(T);

            MaybeAdjustEndianness(typeof(T), rawData, endianness);

            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);

            try
            {
                IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                result = (T)Marshal.PtrToStructure(rawDataPtr, typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return result;
        }

        internal static byte[] StructToBytes<T>(T data, Endianness endianness) where T : struct
        {
            byte[] rawData = new byte[Marshal.SizeOf(data)];
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(data, rawDataPtr, false);
            }
            finally
            {
                handle.Free();
            }

            MaybeAdjustEndianness(typeof(T), rawData, endianness);

            return rawData;
        }

        static uint SwapEndianness(uint x)
        {
            return ((x & 0x000000ff) << 24) +  // First byte
                   ((x & 0x0000ff00) << 8) +   // Second byte
                   ((x & 0x00ff0000) >> 8) +   // Third byte
                   ((x & 0xff000000) >> 24);   // Fourth byte
        }
    }
}

