using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using Lawrence.Game;
using Lawrence.Game.UI;

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
    MP_PACKET_MOBY_DAMAGE = 9,
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
    MP_PACKET_MONITOR_ADDRESS = 26,
    MP_PACKET_MONITORED_ADDRESS_CHANGED = 27,
    MP_PACKET_MOBY_CREATE_FAILURE = 28,
    MP_PACKET_UI = 29,
    MP_PACKET_UI_EVENT = 30,
}

public enum MPStateType : ushort
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
    MP_STATE_TYPE_UNLOCK_SKILLPOINT = 16,
    MP_STATE_SET_COMMUNICATION_FLAGS = 17,
    MP_STATE_START_IN_LEVEL_MOVIE = 18,
}

public enum MPDamageFlags : uint {
    Aggressive = 1,
    GameMoby = 2,
}

public enum MPPacketFlags : ushort
{
    MP_PACKET_FLAG_RPC = 0x1
}

[Flags]
public enum MPMobyFlags : byte
{
    MP_MOBY_FLAG_ACTIVE = 0x1,
    MP_MOBY_NO_COLLISION = 0x2,
    MP_MOBY_FLAG_ORIG_UDPATE_FUNC = 0x4,
    MP_MOBY_FLAG_ATTACHED_TO = 0x8,
}

public enum MPControllerInputFlags : ushort
{
    MP_CONTROLLER_FLAGS_PRESSED = 0x1,
    MP_CONTROLLER_FLAGS_HELD = 0x2,
}

public enum MPMobyCreateFailureReason : byte
{
    UNKNOWN = 0,
    NOT_READY = 1,
    ALREADY_EXISTS = 2,
    INVALID_PARENT = 3,
    MAX_MOBYS = 4,
    UPDATE_NON_EXISTENT = 5,
    SUCCESS = 6,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketHeader
{
    public MPPacketType PacketType;
    public MPPacketFlags Flags;
    public UInt32 Size;
    public long TimeSent;
    public byte RequiresAck;
    public byte AckCycle;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketConnect : MPPacket {
    public Int32 UserId;
    public UInt32 Version;
    public byte Passcode1;
    public byte Passcode2;
    public byte Passcode3;
    public byte Passcode4;
    public byte Passcode5;
    public byte Passcode6;
    public byte Passcode7;
    public byte Passcode8;
    public UInt16 NickLength;

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
        if (this is MPPacketData data) {
            return data.GetData().Length;
        }
        
        return Marshal.SizeOf(this);
    }

    public byte[] GetBytes(Packet.Endianness endianness) {
        // If we're MPPacketData, we take the _data field and return it
        if (this is MPPacketData data) {
            return data.GetData();
        }
        
        // Otherwise, we marshal the struct to bytes
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyUpdate : MPPacket
{
    public ushort Uuid;
    public ushort OClass;
    public byte AnimationID;
    public byte AnimationDuration;
    public float X;
    public float Y;
    public float Z;
    public float RotX;
    public float RotY;
    public float RotZ;
    public float Scale;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyExtended : MPPacket {
    public UInt16 Uuid;
    public UInt16 NumValues;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyExtendedPayload : MPPacket {
    public UInt16 Offset;
    public UInt32 Value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyCreate : MPPacket
{
    public ushort Uuid;
    public ushort ParentUuid;
    public byte SpawnId;
    public MPMobyFlags Flags;
    public ushort OClass;
    public ushort ModeBits;
    public byte PositionBone;
    public byte TransformBone;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyCreateResponse : MPPacket {
    public ushort Uuid;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyDelete : MPPacket {
    public uint Uuid;
    public uint Flags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyCreateFailure : MPPacket {
    public ushort Uuid;
    public MPMobyCreateFailureReason Reason;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMobyDamage : MPPacket
{
    public ushort Uuid;
    public ushort CollidedWithUuid;
    public MPDamageFlags Flags;
    public ushort DamagedOClass;
    public ushort SourceOClass;
    public float X;
    public float Y;
    public float Z;
    public float Damage;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketSetState : MPPacket
{
    public ushort Flags;
    public MPStateType StateType;
    public uint Offset;
    public uint Value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketSetStateFloat : MPPacket {
    public ushort Flags;
    public MPStateType StateType;
    public uint Offset;
    public float Value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketSetHUDText : MPPacket
{
    public ushort Id;
    public ushort X;
    public ushort Y;
    public ushort Flags;
    public uint Color;
    public ushort BoxHeight;
    public ushort BoxWidth;
    public float Size;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketQueryGameServers : MPPacket {
    public byte Version;
    public uint Page;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketQueryResponseServer : MPPacket
{
    public uint Ip;
    public ushort Port;
    public ushort MaxPlayers;
    public ushort PlayerCount;
    public ushort NameLength;
    public ushort DescriptionLength;
    public ushort OwnerLength;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketErrorMessage : MPPacket {
    public UInt16 MessageLength;
}

[StructLayout(LayoutKind.Sequential)]
public struct MPPacketRegisterServer : MPPacket {
    public uint Ip;
    public ushort Port;
    public ushort MaxPlayers;
    public ushort PlayerCount;
    public ushort NameLength;
    public ushort DescriptionLength;
    public ushort OwnerNameLength;
    
    public string GetName(byte[] packetBody) {
        byte[] nameBytes = packetBody.Skip(Marshal.SizeOf(this)).Take(NameLength).ToArray();
        
        return Encoding.UTF8.GetString(nameBytes);
    }
    
    public string GetDescription(byte[] packetBody) {
        byte[] descriptionBytes = packetBody.Skip(Marshal.SizeOf(this) + NameLength).Take(DescriptionLength).ToArray();
        
        return Encoding.UTF8.GetString(descriptionBytes);
    }
    
    public string GetOwner(byte[] packetBody) {
        byte[] ownerBytes = packetBody.Skip(Marshal.SizeOf(this) + NameLength + DescriptionLength).Take(OwnerNameLength).ToArray();
        
        return Encoding.UTF8.GetString(ownerBytes);
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct MPPacketControllerInput : MPPacket
{
    [FieldOffset(0x0)] public ushort Input;
    [FieldOffset(0x2)] public MPControllerInputFlags Flags;
}

[StructLayout(LayoutKind.Sequential, Pack =  1)]
public struct MPPacketTimeResponse : MPPacket {
    public ulong ClientSendTime;
    public ulong ServerSendTime;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketToastMessage : MPPacket {
    public UInt32 MessageType;
    public UInt32 Duration;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketRegisterHybridMoby : MPPacket {
    public ushort MobyUid;
    public ushort MonitoredAttributesCount;
    public ushort MonitoredPVarsCount;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMonitorValue : MPPacket {
    public ushort Offset;
    public ushort Size;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMonitoredValueChanged : MPPacket {
    public ushort Uid;
    public ushort Offset;
    public byte Flags;
    public byte Size;
    public uint OldValue;
    public uint NewValue;
}

public enum MPPacketChangeMobyValueFlags : ushort {
    MP_MOBY_FLAG_FIND_BY_UUID = 1 << 0,
    MP_MOBY_FLAG_FIND_BY_UID = 1 << 1,
    MP_MOBY_FLAG_CHANGE_ATTR = 1 << 8,
    MP_MOBY_FLAG_CHANGE_PVAR = 1 << 9
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketChangeMobyValue : MPPacket {
    public ushort Id;
    public ushort Flags;
    public ushort NumValues;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketChangeMobyValuePayload : MPPacket {
    public ushort Offset;
    public ushort Size;
    public uint Value;
}

public struct MPPacketStringData : MPPacket {
    public string Data;
    
    public long GetSize() {
        return Encoding.UTF8.GetBytes(Data).Length;
    }
    
    public byte[] GetBytes(Packet.Endianness endianness) {
        return Encoding.UTF8.GetBytes(Data);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketLevelFlagsChanged : MPPacket {
    public ushort Type;
    public byte Level;
    public byte Flags;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketLevelFlag : MPPacket {
    public byte Size;
    public ushort Index;
    public uint Value;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMonitorAddress : MPPacket {
    public byte Flags;
    public byte Size;
    public uint Address;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketMonitoredAddressChanged : MPPacket {
    public uint Address;
    public ushort Size;
    public uint OldValue;
    public uint NewValue;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketSpawned : MPPacket {
    public byte SpawnId;
    public ushort LevelId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketUI : MPPacket {
    public ushort Id;
    public MPUIElementType ElementType;
    public MPUIOperationFlag Operations;
    public byte Items;
}
    
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketUIItem : MPPacket {
    public ushort Id;
    public MPUIOperationFlag Operations;
    public byte Pad;
    public ushort Attribute;
    public ushort DataLength;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketUIEvent : MPPacket {
    public MPUIElementEventType EventType;
    public ushort ElementId;
    public uint Data;
    public ushort ExtraLength;
    
    public byte[] GetExtraData(byte[] packetBody) {
        return packetBody.Skip(Marshal.SizeOf(this)).Take(ExtraLength).ToArray();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MPPacketData () : MPPacket {
    private List<byte> _data = new();
    
    public byte[] GetData() {
        return _data.ToArray();
    }

    public void Write(byte[] data) {
        _data.AddRange(data);
    }
}

[Flags]
public enum MPUIOperationFlag : byte {
    Create = 1,
    Update = 2,
    Delete = 4,
    ClearAll = 8
}

public enum MPUIElementType : ushort {
    None = 0,
    Text = 1, 
    TextArea = 2,
    ListMenu = 3,
    Input = 4,
}

public enum MPUIElementAttribute : ushort {
    None,
    Position,
    Size,
    Margins,
    Visible,
    States,
    LineSpacing,
    ElementSpacing,
    DrawsBackground,
    Text,
    TitleText,
    DetailsText,
    TextSize,
    TextColor,
    TitleTextSize,
    TitleTextColor,
    DetailsTextSize,
    DetailsTextColor,
    MenuDefaultColor,
    MenuSelectedColor,
    MenuSelectedItem,
    MenuItems,
    Shadow,
    InputPrompt,
    WorldSpacePosition,
    Alignment,
    WorldSpaceFlags,
    WorldSpaceMaxDistance,
}
    
public enum MPUIElementEventType: ushort {
    MPUIElementEventTypeNone,
    MPUIElementEventTypeItemActivated,
    MPUIElementEventTypeItemSelected,
    MPUIElementEventTypeMakeFocused,
    MPUIElementEventTypeActivate,
    MPUIElementEventTypeInputCallback,
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
            PacketType = packetType,
            RequiresAck = requiresAck,
            AckCycle = ackCycle
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

        Header.Size = (uint)_size;

        return (Header, buffer);
    } 
}

public partial class Packet {
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
            StateType = MPStateType.MP_STATE_TYPE_PLAYER,
            Value = 0x16
        };

        MPPacketSetState damageState = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_DAMAGE,
            Value = damage
        };

        packet.AddBodyPart(bonkState);
        packet.AddBodyPart(damageState);

        return packet;
    }

    public static Packet? MakeSetHUDTextPacket(ushort id, string text, ushort x, ushort y, uint color, uint states) {
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
            X = x,
            Y = y,
            Color = color,
            Flags = (ushort)(textElementFlag | gameStateFlags | flagsSetFlag),
            Id = id
        };

        MPPacketStringData data = new MPPacketStringData {
            Data = text
        };

        packet.AddBodyPart(hudText);
        packet.AddBodyPart(data, 50);

        return packet;
    }

    public static Packet? MakeUIItemPacket(ViewElement? element, MPUIOperationFlag flags) { 
        var packet = new Packet(MPPacketType.MP_PACKET_UI);
        
        var attributes = new List<MPPacket>();

        if (element != null) {
            foreach (var attribute in element.GetAttributes()) {
                if (!attribute.Dirty && (flags & MPUIOperationFlag.Create) == 0) {
                    continue;
                }

                var dataPacket = attribute.GetPacket();
                if (dataPacket == null) continue;
                var attributePacket = new MPPacketUIItem {
                    Id = element.Id,
                    Operations = flags,
                    Attribute = (ushort)attribute.ElementAttribute,
                    DataLength = (ushort)(dataPacket?.GetSize() ?? 0)
                };

                attributes.Add(attributePacket);
                attributes.Add(dataPacket!);
            }
        }
        
        MPUIElementType elementType = MPUIElementType.None;
        switch (element) {
            case TextElement text:
                elementType = MPUIElementType.Text;
                break;
            case TextAreaElement textArea:
                elementType = MPUIElementType.TextArea;
                break;
            case ListMenuElement listMenu:
                elementType = MPUIElementType.ListMenu;
                break;
            case InputElement input:
                elementType = MPUIElementType.Input;
                break;
        }

        packet.AddBodyPart(new MPPacketUI {
            Id = element?.Id ?? (ushort)0,
            ElementType = elementType,
            Operations = flags,
            Items = (byte)(attributes.Count / 2)
        });

        if (attributes.Count == 0 && (flags & (MPUIOperationFlag.ClearAll | MPUIOperationFlag.Delete)) == 0) {
            return null;
        }
        
        foreach (var attribute in attributes) {
            packet.AddBodyPart(attribute);
        }

        return packet;
    }

    public static Packet MakeUIEventPacket(MPUIElementEventType eventType, ViewElement element) {
        var packet = new Packet(MPPacketType.MP_PACKET_UI_EVENT);
        
        var uiEvent = new MPPacketUIEvent {
            EventType = eventType,
            ElementId = element.Id,
            Data = 0,
            ExtraLength = 0
        };
        
        packet.AddBodyPart(uiEvent);
        
        return packet;
    }

    // TODO: change flag settings to reserve 2 bits for the current drop_shadow/delete options (since they are 1 and 2).
    // TODO: change flag settings to then reserve 3 bits for game state.
    // TODO: add game state argument for make to say on which gamestate it should be shown/should be moved to. maybe default it to something
    public static Packet MakeDeleteHUDTextPacket(ushort id) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_HUD_TEXT);

        MPPacketSetHUDText hudText = new MPPacketSetHUDText {
            Id = id,
            Flags = 2 // Delete
        };
        MPPacketStringData data = new MPPacketStringData {
            Data = ""
        };

        packet.AddBodyPart(hudText);
        packet.AddBodyPart(data, 50);

        return packet;
    }

    public static Packet MakeDeleteMobyPacket(ushort mobyUUID) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_DELETE);

        MPPacketMobyDelete body = new MPPacketMobyDelete {
            Uuid = mobyUUID,
            Flags = 1
        };

        packet.AddBodyPart(body);

        return packet;
    }

    public static Packet MakeDeleteAllMobysPacket(ushort oClass) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_DELETE);

        MPPacketMobyDelete body = new MPPacketMobyDelete {
            Uuid = oClass,
            Flags = 2
        };

        packet.AddBodyPart(body);

        return packet;
    }

    public static Packet MakeDeleteAllMobysUIDPacket(ushort uid) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_DELETE);

        MPPacketMobyDelete body = new MPPacketMobyDelete {
            Uuid = uid,
            Flags = 8
        };

        packet.AddBodyPart(body);

        return packet;
    }

    public static Packet MakeCreateMobyPacket(ushort id, Moby moby, ushort parentId = 0) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_CREATE);

        var flags = MPMobyFlags.MP_MOBY_FLAG_ACTIVE;
        flags |= moby.CollisionEnabled ? 0 : MPMobyFlags.MP_MOBY_NO_COLLISION;
        flags |= moby.MpUpdateFunc ? 0 : MPMobyFlags.MP_MOBY_FLAG_ORIG_UDPATE_FUNC;
        flags |= moby.AttachedTo != null ? MPMobyFlags.MP_MOBY_FLAG_ATTACHED_TO : 0;

        MPPacketMobyCreate mobyCreate = new MPPacketMobyCreate {
            Uuid = id,
            ParentUuid = parentId,
            Flags = flags,
            OClass = (ushort)moby.oClass,
            ModeBits = moby.modeBits,
            PositionBone = moby.PositionBone,
            TransformBone = moby.TransformBone,
        };

        packet.AddBodyPart(mobyCreate);

        return packet;
    }
    
public static Packet MakeMobyUpdatePacket(ushort id, Moby moby) {
        var packet = new Packet(MPPacketType.MP_PACKET_MOBY_UPDATE, 0, 0);

        MPPacketMobyUpdate mobyUpdate = new MPPacketMobyUpdate {
            Uuid = id
        };
        
        mobyUpdate.OClass = (ushort)moby.oClass;
        mobyUpdate.X = moby.x;
        mobyUpdate.Y = moby.y;
        mobyUpdate.Z = moby.z;
        mobyUpdate.RotX = (float)(moby.rotX * (Math.PI/180));
        mobyUpdate.RotY = (float)(moby.rotY * (Math.PI/180));
        mobyUpdate.RotZ = (float)(moby.rotZ * (Math.PI/180));
        mobyUpdate.AnimationID = (byte)moby.AnimationId;
        mobyUpdate.Scale = moby.scale;
        
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
            Uuid = uuid,
            NumValues = (ushort)values.Length
        };
        
        packet.AddBodyPart(mobyExtended);
        
        foreach (UpdateMobyValue value in values) {
            MPPacketMobyExtendedPayload payload = new MPPacketMobyExtendedPayload();
            payload.Offset = value.Offset;
            payload.Value = value.Value;
            
            packet.AddBodyPart(payload);
        }
        
        return packet;
    }

    public static Packet MakeGoToLevelPacket(int level) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState destinationLevelState = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_PLANET,
            Value = (uint)level
        };

        packet.AddBodyPart(destinationLevelState);

        return packet;
    }

    public static Packet MakeMobyCreateResponsePacket(ushort uuid, byte ackId, byte ackCycle) {
        var packet = new Packet(MPPacketType.MP_PACKET_ACK, ackId, ackCycle);
        
        MPPacketMobyCreateResponse mobyCreateResponse = new MPPacketMobyCreateResponse {
            Uuid = uuid
        };
        
        packet.AddBodyPart(mobyCreateResponse);
        
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
                    Ip = addr,
                    Port = (ushort)server.Port,
                    MaxPlayers = (ushort)server.MaxPlayers,
                    PlayerCount = (ushort)server.PlayerCount,
                    NameLength = (ushort)Encoding.UTF8.GetBytes(server.Name).Length,
                    DescriptionLength = (ushort)Encoding.UTF8.GetBytes(server.Description).Length,
                    OwnerLength = (ushort)Encoding.UTF8.GetBytes(server.Owner).Length
                };

                packet.AddBodyPart(response);
                
                packet.AddBodyPart(new MPPacketStringData {
                    Data = server.Name
                });
                
                packet.AddBodyPart(new MPPacketStringData {
                    Data = server.Description
                });
                
                packet.AddBodyPart(new MPPacketStringData {
                    Data = server.Owner
                });
            }
        }

        return packet;
    }

    public static Packet MakeRegisterServerPacket(string ip, ushort port, ushort maxPlayers,
        ushort playerCount, string name, string description = "", string owner = "") {
        var packet = new Packet(MPPacketType.MP_PACKET_REGISTER_SERVER, 0, 0);

        if (!IPAddress.TryParse(ip, out var address)) {
            throw new Exception($"Invalid IP address {ip}");
        }

        MPPacketRegisterServer response = new MPPacketRegisterServer {
            Ip = (uint)address.MapToIPv4().Address,
            Port = port,
            MaxPlayers = maxPlayers,
            PlayerCount = playerCount,
            NameLength = (ushort)name.Length,
            DescriptionLength = (ushort)description.Length,
            OwnerNameLength = (ushort)owner.Length
        };

        packet.AddBodyPart(response);
        
        packet.AddBodyPart(new MPPacketStringData {
            Data = name
        });
        
        packet.AddBodyPart(new MPPacketStringData {
            Data = description
        });
        
        packet.AddBodyPart(new MPPacketStringData {
            Data = owner
        });
        
        return packet;
    }

    public static Packet MakeSetItemPacket(ushort item, bool equip) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        uint flags = 0;
        flags |= 1;  // FLAG_ITEM_GIVE
        flags |= equip ? (uint)2 : (uint)0;  // FLAG_ITEM_EQUIP

        MPPacketSetState setItemState = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_ITEM,
            Value = (flags << 16) | (uint)item
        };

        packet.AddBodyPart(setItemState);

        return packet;
    }

    public static Packet MakeStartInLevelMoviePacket(uint movie) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketSetState startInLevelMovieState = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_START_IN_LEVEL_MOVIE,
            Value = movie,
            Offset =  0
        };
        
        packet.AddBodyPart(startInLevelMovieState);
        
        return packet;
    }

    public static Packet MakeUnlockLevelPacket(int level) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketSetState setItemState = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_UNLOCK_LEVEL,
            Value = (uint)level
        };

        packet.AddBodyPart(setItemState);
        
        return packet;
    }

    public static Packet MakeUnlockSkillpointPacket(byte skillpoint) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketSetState setItemState = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_UNLOCK_SKILLPOINT,
            Value = skillpoint
        };
        
        packet.AddBodyPart(setItemState);

        return packet;
    }
    
    public static Packet MakeSetPlayerStatePacket(ushort state) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState setPlayerState = new MPPacketSetState();
        setPlayerState.StateType = MPStateType.MP_STATE_TYPE_PLAYER;
        setPlayerState.Value = state;

        packet.AddBodyPart(setPlayerState);

        return packet;
    }
    
    public static Packet MakeSetPlayerInputStatePacket(uint state) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState setPlayerState = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_PLAYER_INPUT,
            Value = state
        };
        
        packet.AddBodyPart(setPlayerState);
        
        return packet;
    }
    
    public static Packet MakeSetAddressValuePacket(uint address, uint value, ushort size = 4) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketSetState setPlayerState = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_ARBITRARY,
            Flags = size,
            Offset = address,
            Value = value
        };
        
        packet.AddBodyPart(setPlayerState);

        return packet;
    }
    
    public static Packet MakeSetAddressFloatPacket(uint address, float value) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
    
        MPPacketSetStateFloat setPlayerState = new MPPacketSetStateFloat {
            StateType = MPStateType.MP_STATE_TYPE_ARBITRARY,
            Offset = address,
            Value = value
        };
        
        packet.AddBodyPart(setPlayerState);
    
        return packet;
    }

    public static Packet MakeGiveBoltsPacket(int bolts, bool setBolts = false) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketSetState giveBolts = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_GIVE_BOLTS,
            Value = (uint)bolts,
            Offset = setBolts ? (uint)1 : 0
        };

        packet.AddBodyPart(giveBolts);
        
        return packet;
    }

    public static Packet MakeSetPositionPacket(ushort property, float position) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetStateFloat setPositionState = new MPPacketSetStateFloat {
            StateType = MPStateType.MP_STATE_TYPE_POSITION,
            Offset = property,
            Value = position
        };

        packet.AddBodyPart(setPositionState);

        return packet;
    }

    public static Packet MakeSetRespawnPacket(float x, float y, float z, float rotationZ) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetStateFloat setRespawnX = new MPPacketSetStateFloat {
            StateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN,
            Offset = 0,
            Value = x
        };

        MPPacketSetStateFloat setRespawnY = new MPPacketSetStateFloat {
            StateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN,
            Offset = 1,
            Value = y
        };

        MPPacketSetStateFloat setRespawnZ = new MPPacketSetStateFloat {
            StateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN,
            Offset = 2,
            Value = z
        };

        MPPacketSetStateFloat setRespawnRotZ = new MPPacketSetStateFloat {
            StateType = MPStateType.MP_STATE_TYPE_SET_RESPAWN,
            Offset = 5,
            Value = rotationZ
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
                StateType = MPStateType.MP_STATE_TYPE_LEVEL_FLAG,
                Offset = ((uint)level << 24) | ((uint)type << 16) | i,
                Value = val
            };
            i += 1;
            
            packet.AddBodyPart(setLevelFlag);
        }

        return packet;
    }

    public static Packet MakeSetLevelFlagPacket(byte type, byte level, List<(ushort, uint)> flags) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        foreach ((ushort index, uint value) in flags) {
            MPPacketSetState setLevelFlag = new MPPacketSetState {
                StateType = MPStateType.MP_STATE_TYPE_LEVEL_FLAG,
                Offset = ((uint)level << 24) | ((uint)type << 16) | index,
                Value = value
            };
            
            packet.AddBodyPart(setLevelFlag);
        }

        return packet;
    }

    public static Packet MakeSetCommunicationFlagsPacket(UInt32 bitmap) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);
        
        MPPacketSetState setCommunicationFlags = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_SET_COMMUNICATION_FLAGS,
            Value = bitmap
        };
        
        packet.AddBodyPart(setCommunicationFlags);
        
        
        return packet;
    }

    public static Packet MakeToastMessagePacket(string message, uint duration = 20) {
        var packet = new Packet(MPPacketType.MP_PACKET_TOAST_MESSAGE);

        MPPacketToastMessage messagePacket = new MPPacketToastMessage {
            MessageType = 0,
            Duration = duration
        };

        MPPacketStringData data = new MPPacketStringData {
            Data = message
        };

        packet.AddBodyPart(messagePacket);
        packet.AddBodyPart(data, 0x50);

        return packet;
    }
    
    public static Packet MakeBlockGoldBoltPacket(int level, int number) {
        var packet = new Packet(MPPacketType.MP_PACKET_SET_STATE);

        MPPacketSetState blockGoldBolt = new MPPacketSetState {
            StateType = MPStateType.MP_STATE_TYPE_BLOCK_GOLD_BOLT,
            Value = (ushort)number,
            Offset = (ushort)level
        };

        packet.AddBodyPart(blockGoldBolt);

        return packet;
    }

    public static Packet MakeErrorMessagePacket(string message) {
        var packet = new Packet(MPPacketType.MP_PACKET_ERROR_MESSAGE);
        
        MPPacketErrorMessage errorMessage = new MPPacketErrorMessage {
            MessageLength = (ushort)message.Length
        };
        
        MPPacketStringData data = new MPPacketStringData {
            Data = message
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
            MobyUid = (ushort)moby.UID,
            MonitoredAttributesCount = (ushort)moby.MonitoredAttributes.Count,
            MonitoredPVarsCount = (ushort)moby.MonitoredPVars.Count
        });
        
        foreach (var attribute in moby.MonitoredAttributes) {
            packet.AddBodyPart(new MPPacketMonitorValue {
                Offset = attribute.Offset,
                Size = attribute.Size
            });
        }
        
        foreach (var pvar in moby.MonitoredPVars) {
            packet.AddBodyPart(new MPPacketMonitorValue {
                Offset = pvar.Offset,
                Size = pvar.Size
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
                Id = uid,
                Flags = (ushort)(flags | (ushort)MPPacketChangeMobyValueFlags.MP_MOBY_FLAG_FIND_BY_UID),
                NumValues = 1
            }
        );
        
        packet.AddBodyPart(new MPPacketChangeMobyValuePayload {
            Offset = offset,
            Size = size,
            Value = value
        });
        
        return packet;
    }
    
    public static Packet MakeMonitorAddressPacket(uint address, byte size, byte flags) {
        var packet = new Packet(MPPacketType.MP_PACKET_MONITOR_ADDRESS);
        
        packet.AddBodyPart(new MPPacketMonitorAddress {
            Address = address,
            Size = size,
            Flags = flags
        });
        
        return packet;
    }

    public static MPPacketHeader? MakeHeader(byte[] bytes, Endianness endianness = Endianness.BigEndian) {
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

    internal static T? BytesToStruct<T>(byte[] rawData, Endianness endianness) where T : struct
    {
        T? result = default(T);

        MaybeAdjustEndianness(typeof(T), rawData, endianness);

        GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);

        try
        {
            IntPtr rawDataPtr = handle.AddrOfPinnedObject();
            result = (T?)Marshal.PtrToStructure(rawDataPtr, typeof(T));
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
