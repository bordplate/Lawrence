using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using Lawrence.Game;

namespace Lawrence.Core;

public enum MPPacketType : ushort
{
    MP_PACKET_CONNECT = 1,      // Not used
    MP_PACKET_SYN = 2,
    MP_PACKET_SYN_LE = 512,  // Little endian clients will send this as their first packet when we default parse as big endian
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
    MP_PACKET_ERROR_MESSAGE = 21,
    MP_PACKET_REGISTER_HYBRID_MOBY = 22,
    MP_PACKET_MONITORED_VALUE_CHANGED = 23,
    MP_PACKET_CHANGE_MOBY_VALUE = 24,
    MP_PACKET_LEVEL_FLAG_CHANGED = 25,
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
    MP_STATE_TYPE_UNLOCK_LEVEL = 14,
    MP_STATE_TYPE_LEVEL_FLAG = 15,
}

public enum MPPacketFlags : ushort
{
    MP_PACKET_FLAG_RPC = 0x1
}

[Flags]
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
public struct MPPacketConnect : MPPacket {
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

public interface MPPacket {
    public long GetSize() {
        return Marshal.SizeOf(this);
    }

    public byte[] GetBytes(Packet.Endianness endianness) {
        return Packet.MPStructToBytes(this, endianness);
    }

    public sealed MPPacket Get() {
        return this;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketConnectResponse: MPPacket {
    public MPPacketConnectResponseStatus status;
}

[StructLayout(LayoutKind.Explicit)]
public struct MPPacketMobyUpdate : MPPacket
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
public struct MPPacketMobyExtended : MPPacket {
    public UInt16 uuid;
    public UInt16 numValues;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyExtendedPayload : MPPacket {
    public UInt16 offset;
    public UInt32 value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyCreate : MPPacket
{
    public UInt32 uuid;
    public UInt32 flags;
}

[StructLayout(LayoutKind.Explicit)]
public struct MPPacketMobyCollision : MPPacket
{
    [FieldOffset(0x0)] public uint flags;
    [FieldOffset(0x4)] public ushort uuid;
    [FieldOffset(0x6)] public ushort collidedWith;
    [FieldOffset(0x8)] public float x;
    [FieldOffset(0xc)] public float y;
    [FieldOffset(0x10)] public float z;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketSetState : MPPacket
{
    public MPStateType stateType;
    public uint offset;
    public uint value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketSetStateFloat : MPPacket
{
    public MPStateType stateType;
    public uint offset;
    public float value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketBolts : MPPacket
{
    public MPStateType stateType;
    public uint value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketSetHUDText : MPPacket
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
public struct MPPacketQueryResponseServer : MPPacket
{
    [FieldOffset(0x0)] public uint ip;
    [FieldOffset(0x4)] public ushort port;
    [FieldOffset(0x6)] public ushort maxPlayers;
    [FieldOffset(0x8)] public ushort playerCount;
    [FieldOffset(0xa)] public ushort nameLength;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketErrorMessage : MPPacket {
    public UInt16 messageLength;
}

[StructLayout(LayoutKind.Sequential)]
public struct MPPacketRegisterServer : MPPacket {
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
public struct MPPacketControllerInput : MPPacket
{
    [FieldOffset(0x0)] public ushort input;
    [FieldOffset(0x2)] public MPControllerInputFlags flags;
}

[StructLayout(LayoutKind.Sequential, Pack =  1)]
public struct MPPacketTimeResponse : MPPacket {
    public ulong clientSendTime;
    public ulong serverSendTime;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketToastMessage : MPPacket {
    public UInt32 messageType;
    public UInt32 duration;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketRegisterHybridMoby : MPPacket {
    public ushort mobyUid;
    public ushort nMonitoredAttributes;
    public ushort nMonitoredPVars;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMonitorValue : MPPacket {
    public ushort offset;
    public ushort size;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMonitoredValueChanged : MPPacket {
    public ushort uid;
    public ushort offset;
    public byte flags;
    public byte size;
    public uint oldValue;
    public uint newValue;
}

public enum MPPacketChangeMobyValueFlags : ushort {
    MP_MOBY_FLAG_FIND_BY_UUID = 1 << 0,
    MP_MOBY_FLAG_FIND_BY_UID = 1 << 1,
    MP_MOBY_FLAG_CHANGE_ATTR = 1 << 8,
    MP_MOBY_FLAG_CHANGE_PVAR = 1 << 9
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketChangeMobyValue : MPPacket {
    public ushort id;
    public ushort flags;
    public ushort numValues;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketChangeMobyValuePayload : MPPacket {
    public ushort offset;
    public ushort size;
    public uint value;
}

public struct MPPacketStringData : MPPacket {
    public string data;
    
    public long GetSize() {
        return Encoding.UTF8.GetBytes(data).Length;
    }
    
    public byte[] GetBytes(Packet.Endianness endianness) {
        return Encoding.UTF8.GetBytes(data);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketLevelFlagChanged : MPPacket {
    public ushort type;
    public byte level;
    public byte size;
    public ushort index;
    public uint value;
}

public struct PacketBodyPart<T> where T : MPPacket {
    public T BodyPart;
    public int Size;
    
    public PacketBodyPart(T bodyPart, int size) {
        BodyPart = bodyPart;
        Size = size;
    }
    
}

public partial class Packet {
    public MPPacketHeader Header;
    private List<PacketBodyPart<MPPacket>> _bodyParts = new ();
    
    private int _size = 0;

    public Packet(MPPacketType packetType, byte requiresAck = 255, byte ackCycle = 255) {
        Header = new MPPacketHeader {
            ptype = packetType,
            requiresAck = requiresAck,
            ackCycle = ackCycle
        };
    }
    
    public void AddBodyPart<T>(T bodyPart, int size = -1) where T : MPPacket {
        var packetBodyPart = new PacketBodyPart<MPPacket>(bodyPart.Get(), size);

        var realSize = (int)bodyPart.GetSize();
        
        if (size == -1) {
            _size += realSize;
            packetBodyPart.Size = realSize;
        }
        else {
            _size += size;
        }
        
        _bodyParts.Add(packetBodyPart);
    }

    public (MPPacketHeader, byte[]) GetBytes(Endianness endianness = Endianness.BigEndian) {
        byte[] buffer = new byte[_size];
        
        int offset = 0;
        
        foreach (PacketBodyPart<MPPacket> bodyPart in _bodyParts) {
            byte[] bodyPartBytes = bodyPart.BodyPart.GetBytes(endianness);
            bodyPartBytes.CopyTo(buffer, offset);
            
            offset += bodyPart.Size;
        }

        Header.size = (uint)_size;

        return (Header, buffer);
    } 
}

public partial class Packet
{
    public static Packet MakeAckPacket() {
        var packet = new Packet(MPPacketType.MP_PACKET_ACK, 0, 0);

        return packet;
    }

    public static Packet MakeDisconnectPacket() {
        var packet = new Packet(MPPacketType.MP_PACKET_DISCONNECTED, 0, 0);

        return packet;
    }

    public static Packet MakeDamagePacket(uint damage) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState bonkState = new MPPacketSetState {
            stateType = MPStateType.MP_STATE_TYPE_PLAYER,
            value = 0x16
        };

        MPPacketSetState damageState = new MPPacketSetState {
            stateType = MPStateType.MP_STATE_TYPE_DAMAGE,
            value = damage
        };
        
        packet.AddBodyPart(bonkState);
        packet.AddBodyPart(damageState);

        return packet;
    }

    public static Packet MakeSetHUDTextPacket(ushort id, string text, ushort x, ushort y, uint color, uint states) {
        if (text.Length >= 50) {
            Logger.Error("Text is too long to fit in a single packet.");
            return null;
        }

        var packet = new Packet(MPPacketType.MP_PACKET_SET_HUD_TEXT);

        // hudText.flags field contains multiple separate pieces of information. Each next one is shifted left until they don't interfere with the previous one
        uint textElementFlag = 1; // <2 bits> Drop shadow
        uint flagsSetFlag = 1 << 2; // <1 bit>
        uint gameStateFlags = states << 3; // <8 bits>

        MPPacketSetHUDText hudText = new MPPacketSetHUDText {
            x = x,
            y = y,
            color = color,
            flags = (ushort)(textElementFlag | gameStateFlags | flagsSetFlag),
            id = id
        };
        
        MPPacketStringData data = new MPPacketStringData {
            data = text
        };
        
        packet.AddBodyPart(hudText);
        packet.AddBodyPart(data, 50);
        
        return packet;
    }
    // TODO: change flag settings to reserve 2 bits for the current drop_shadow/delete options (since they are 1 and 2).
    // TODO: change flag settings to then reserve 3 bits for game state.
    // TODO: add game state argument for make to say on which gamestate it should be shown/should be moved to. maybe default it to something
    public static Packet MakeDeleteHUDTextPacket(ushort id) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_HUD_TEXT);

        MPPacketSetHUDText hudText = new MPPacketSetHUDText {
            id = id,
            flags = 2 // Delete
        };
        MPPacketStringData data = new MPPacketStringData {
            data = ""
        };

        packet.AddBodyPart(hudText);
        packet.AddBodyPart(data, 50);

        return packet;
    }

    public static Packet MakeDeleteMobyPacket(ushort mobyUUID) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_DELETE);

        MPPacketMobyCreate body = new MPPacketMobyCreate {
            uuid = mobyUUID,
            flags = 1
        };

        packet.AddBodyPart(body);
        
        return packet;
    }
    
    public static Packet MakeDeleteAllMobysPacket(ushort oClass) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_DELETE);

        MPPacketMobyCreate body = new MPPacketMobyCreate {
            uuid = oClass,
            flags = 2
        };

        packet.AddBodyPart(body);
        
        return packet;
    }
    
    public static Packet MakeDeleteAllMobysUIDPacket(ushort uid) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_DELETE);
        
        MPPacketMobyCreate body = new MPPacketMobyCreate {
            uuid = uid,
            flags = 8
        };

        packet.AddBodyPart(body);

        return packet;
    }

    public static Packet MakeMobyUpdatePacket(ushort id, Moby moby) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_UPDATE, 0, 0);

        MPPacketMobyUpdate mobyUpdate = new MPPacketMobyUpdate {
            uuid = id
        };

        mobyUpdate.mpFlags |= moby.IsActive() ? MPMobyFlags.MP_MOBY_FLAG_ACTIVE : 0;
        mobyUpdate.mpFlags |= moby.CollisionEnabled ? 0 : MPMobyFlags.MP_MOBY_NO_COLLISION;
        mobyUpdate.mpFlags |= moby.MpUpdateFunc ? 0 : MPMobyFlags.MP_MOBY_FLAG_ORIG_UDPATE_FUNC;
        
        mobyUpdate.parent = (ushort)0; // Parent isn't really used
        mobyUpdate.oClass = (ushort)moby.oClass;
        mobyUpdate.level = moby.Level() != null ? (ushort)moby.Level().GameID() : (ushort)0;
        mobyUpdate.x = moby.x;
        mobyUpdate.y = moby.y;
        mobyUpdate.z = moby.z;
        mobyUpdate.rotX = (float)(Math.PI / 180) * moby.rotX;
        mobyUpdate.rotY = (float)(Math.PI / 180) * moby.rotY;
        mobyUpdate.rotZ = (float)(Math.PI / 180) * moby.rotZ;
        mobyUpdate.animationID = moby.AnimationId;
        mobyUpdate.scale = moby.scale;
        mobyUpdate.alpha = Math.Min((byte)(moby.alpha * 128), (byte)128);
        
        mobyUpdate.modeBits = moby.modeBits;
        
        packet.AddBodyPart(mobyUpdate);

        return packet;
    }

    public struct UpdateMobyValue {
        public readonly UInt16 Offset;
        public readonly uint Value;
        
        public UpdateMobyValue(UInt16 offset, uint value) { Offset = offset; Value = value; }
    }

    public static Packet MakeMobyUpdateExtended(ushort uuid, UpdateMobyValue[] values) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_EXTENDED, 0, 0);
        
        MPPacketMobyExtended mobyExtended = new MPPacketMobyExtended {
            uuid = uuid,
            numValues = (ushort)values.Length
        };
        
        packet.AddBodyPart(mobyExtended);
        
        foreach (UpdateMobyValue value in values) {
            MPPacketMobyExtendedPayload payload = new MPPacketMobyExtendedPayload();
            payload.offset = value.Offset;
            payload.value = value.Value;
            
            packet.AddBodyPart(payload);
        }
        
        return packet;
    }

    public static Packet MakeGoToLevelPacket(int level) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState destinationLevelState = new MPPacketSetState {
            stateType = MPStateType.MP_STATE_TYPE_PLANET,
            value = (uint)level
        };

        packet.AddBodyPart(destinationLevelState);

        return packet;
    }

    public static Packet MakeQueryServerResponsePacket(List<ServerItem> servers, byte ackId, byte ackCycle) {
        var packet = new Packet(MPPacketType.MP_PACKET_ACK, ackId, ackCycle);

        foreach (ServerItem server in servers)
        {
            if (IPAddress.TryParse(server.IP, out var ipAddress))
            {
                // If the parsing is successful, get the 32-bit integer representation of the IP address
                uint addr = SwapEndianness((uint)ipAddress.Address);

                MPPacketQueryResponseServer response = new MPPacketQueryResponseServer {
                    ip = addr,
                    port = (ushort)server.Port,
                    maxPlayers = (ushort)server.MaxPlayers,
                    playerCount = (ushort)server.PlayerCount,
                    nameLength = (ushort)Encoding.UTF8.GetBytes(server.Name).Length
                };
                
                MPPacketStringData data = new MPPacketStringData {
                    data = server.Name
                };

                packet.AddBodyPart(response);
                packet.AddBodyPart(data);
            }
        }

        return packet;
    }

    public static Packet MakeRegisterServerPacket(string ip, ushort port, ushort maxPlayers,
        ushort playerCount, string name) {
        var packet = new Packet(MPPacketType.MP_PACKET_REGISTER_SERVER, 0, 0);

        if (!IPAddress.TryParse(ip, out var address)) {
            throw new Exception($"Invalid IP address {ip}");
        }

        MPPacketRegisterServer response = new MPPacketRegisterServer {
            ip = (uint)address.MapToIPv4().Address,
            port = port,
            maxPlayers = maxPlayers,
            playerCount = playerCount,
            nameLength = (ushort)name.Length
        };

        MPPacketStringData data = new MPPacketStringData {
            data = name
        };

        packet.AddBodyPart(response);
        packet.AddBodyPart(data);
        
        return packet;
    }

    public static Packet MakeSetItemPacket(ushort item, bool equip) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        uint flags = 0;
        flags |= 1;  // FLAG_ITEM_GIVE
        flags |= equip ? (uint)2 : (uint)0;  // FLAG_ITEM_EQUIP

        MPPacketSetState setItemState = new MPPacketSetState {
            stateType = MPStateType.MP_STATE_TYPE_ITEM,
            value = (flags << 16) | (uint)item
        };

        packet.AddBodyPart(setItemState);

        return packet;
    }

    public static Packet MakeUnlockLevelPacket(int level) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketSetState setItemState = new MPPacketSetState {
            stateType = MPStateType.MP_STATE_TYPE_UNLOCK_LEVEL,
            value = (uint)level
        };

        packet.AddBodyPart(setItemState);
        
        return packet;
    }
    
    public static Packet MakeSetPlayerStatePacket(ushort state) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState setPlayerState = new MPPacketSetState();
        setPlayerState.stateType = MPStateType.MP_STATE_TYPE_PLAYER;
        setPlayerState.value = state;

        packet.AddBodyPart(setPlayerState);

        return packet;
    }
    
    public static Packet MakeSetPlayerInputStatePacket(uint state) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState setPlayerState = new MPPacketSetState {
            stateType = MPStateType.MP_STATE_TYPE_PLAYER_INPUT,
            value = state
        };
        
        packet.AddBodyPart(setPlayerState);
        
        return packet;
    }
    
    public static Packet MakeSetAddressValuePacket(uint address, uint value) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketSetState setPlayerState = new MPPacketSetState {
            stateType = MPStateType.MP_STATE_TYPE_ARBITRARY,
            offset = address,
            value = value
        };
        
        packet.AddBodyPart(setPlayerState);

        return packet;
    }
    
    public static Packet MakeSetAddressFloatPacket(uint address, float value) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetStateFloat setPlayerState = new MPPacketSetStateFloat {
            stateType = MPStateType.MP_STATE_TYPE_ARBITRARY,
            offset = address,
            value = value
        };
        
        packet.AddBodyPart(setPlayerState);

        return packet;
    }

    public static Packet MakeGiveBoltsPacket(uint bolts) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketBolts giveBolts = new MPPacketBolts {
            stateType = MPStateType.MP_STATE_TYPE_GIVE_BOLTS,
            value = bolts
        };

        packet.AddBodyPart(giveBolts);
        
        return packet;
    }

    public static Packet MakeSetPositionPacket(ushort property, float position) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetStateFloat setPositionState = new MPPacketSetStateFloat {
            stateType = MPStateType.MP_STATE_TYPE_POSITION,
            offset = property,
            value = position
        };

        packet.AddBodyPart(setPositionState);

        return packet;
    }

    public static Packet MakeSetRespawnPacket(float x, float y, float z, float rotationZ) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetStateFloat setRespawnX = new MPPacketSetStateFloat {
            stateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN,
            offset = 0,
            value = x
        };

        MPPacketSetStateFloat setRespawnY = new MPPacketSetStateFloat {
            stateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN,
            offset = 1,
            value = y
        };

        MPPacketSetStateFloat setRespawnZ = new MPPacketSetStateFloat {
            stateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN,
            offset = 2,
            value = z
        };

        MPPacketSetStateFloat setRespawnRotZ = new MPPacketSetStateFloat {
            stateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN,
            offset = 5,
            value = rotationZ
        };
        
        packet.AddBodyPart(setRespawnX);
        packet.AddBodyPart(setRespawnY);
        packet.AddBodyPart(setRespawnZ);
        packet.AddBodyPart(setRespawnRotZ);
        
        return packet;
    }

    public static Packet MakeSetLevelFlagPacket(byte type, byte level, ushort index, uint[] value) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        var i = index;
        foreach (uint val in value) {
            MPPacketSetState setLevelFlag = new MPPacketSetState {
                stateType = MPStateType.MP_STATE_TYPE_LEVEL_FLAG,
                offset = ((uint)level << 24) | ((uint)type << 16) | i,
                value = val
            };
            i += 1;
            
            packet.AddBodyPart(setLevelFlag);
        }

        return packet;
    }

    public static Packet MakeToastMessagePacket(string message, uint duration = 20) {
        var packet = new Packet(MPPacketType.MP_PACKET_TOAST_MESSAGE);

        MPPacketToastMessage messagePacket = new MPPacketToastMessage {
            messageType = 0,
            duration = duration
        };

        MPPacketStringData data = new MPPacketStringData {
            data = message
        };

        packet.AddBodyPart(messagePacket);
        packet.AddBodyPart(data, 0x50);

        return packet;
    }
    
    public static Packet MakeBlockGoldBoltPacket(int level, int number) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState blockGoldBolt = new MPPacketSetState {
            stateType = MPStateType.MP_STATE_TYPE_BLOCK_GOLD_BOLT,
            value = (ushort)number,
            offset = (ushort)level
        };

        packet.AddBodyPart(blockGoldBolt);

        return packet;
    }

    public static Packet MakeErrorMessagePacket(string message) {
        var packet = new Packet(MPPacketType.MP_PACKET_ERROR_MESSAGE);
        
        MPPacketErrorMessage errorMessage = new MPPacketErrorMessage {
            messageLength = (ushort)message.Length
        };
        
        MPPacketStringData data = new MPPacketStringData {
            data = message
        };
        
        packet.AddBodyPart(errorMessage);
        packet.AddBodyPart(data);
        
        return packet;
    }

    public static Packet MakeRegisterHybridMobyPacket(Moby moby) {
        if (!moby.IsHybrid) {
            throw new Exception("Tried to register a hybrid moby that isn't a hybrid moby!");
        }
        
        var packet = new Packet(MPPacketType.MP_PACKET_REGISTER_HYBRID_MOBY);
        packet.AddBodyPart(new MPPacketRegisterHybridMoby {
            mobyUid = (ushort)moby.UID,
            nMonitoredAttributes = (ushort)moby.MonitoredAttributes.Count,
            nMonitoredPVars = (ushort)moby.MonitoredPVars.Count
        });
        
        foreach (var attribute in moby.MonitoredAttributes) {
            packet.AddBodyPart(new MPPacketMonitorValue {
                offset = attribute.Offset,
                size = attribute.Size
            });
        }
        
        foreach (var pvar in moby.MonitoredPVars) {
            packet.AddBodyPart(new MPPacketMonitorValue {
                offset = pvar.Offset,
                size = pvar.Size
            });
        }
        
        return packet;
    }

    public static Packet MakeChangeMobyValuePacket(ushort uid, MonitoredValueType type, ushort offset, ushort size,
        uint value) {
        var packet = new Packet(MPPacketType.MP_PACKET_CHANGE_MOBY_VALUE);
        
        ushort flags = type == MonitoredValueType.Attribute ? 
            (ushort)MPPacketChangeMobyValueFlags.MP_MOBY_FLAG_CHANGE_ATTR : 
            (ushort)MPPacketChangeMobyValueFlags.MP_MOBY_FLAG_CHANGE_PVAR;
        
        packet.AddBodyPart(
            new MPPacketChangeMobyValue {
                id = uid,
                flags = (ushort)(flags | (ushort)MPPacketChangeMobyValueFlags.MP_MOBY_FLAG_FIND_BY_UID),
                numValues = 1
            }
        );
        
        packet.AddBodyPart(new MPPacketChangeMobyValuePayload {
            offset = offset,
            size = size,
            value = value
        });
        
        return packet;
    }

    public static MPPacketHeader MakeHeader(byte[] bytes, Endianness endianness = Endianness.BigEndian)
    {
        return BytesToStruct<MPPacketHeader>(bytes, endianness);
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
        try {
            IntPtr rawDataPtr = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(data, rawDataPtr, false);
        }
        finally {
            handle.Free();
        }

        MaybeAdjustEndianness(data.GetType(), rawData, endianness);

        return rawData;
    }
    
    internal static byte[] MPStructToBytes<T>(T data, Endianness endianness) where T : MPPacket 
    {
        byte[] rawData = new byte[Marshal.SizeOf(data)];
        GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
        try {
            IntPtr rawDataPtr = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(data, rawDataPtr, false);
        } finally {
            handle.Free();
        }

        MaybeAdjustEndianness(data.GetType(), rawData, endianness);

        return rawData;
    }


    static uint SwapEndianness(uint x) {
        return ((x & 0x000000ff) << 24) +  // First byte
               ((x & 0x0000ff00) << 8) +   // Second byte
               ((x & 0x00ff0000) >> 8) +   // Third byte
               ((x & 0xff000000) >> 24);   // Fourth byte
    }
}
