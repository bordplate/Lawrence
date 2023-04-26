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
        MP_PACKET_IDKU = 5,
        MP_PACKET_MOBY_CREATE = 6,
        MP_PACKET_DISCONNECTED = 7,
        MP_PACKET_MOBY_DELETE = 8,
        MP_PACKET_MOBY_COLLISION = 9,
        MP_PACKET_SET_STATE = 10,
        MP_PACKET_SET_HUD_TEXT = 11,
        MP_PACKET_QUERY_GAME_SERVERS = 12,
        MP_PACKET_CONTROLLER_INPUT = 13
    }

    public enum MPStateType : uint
    {
        MP_STATE_TYPE_DAMAGE = 1,
        MP_STATE_TYPE_PLAYER =  2,
        MP_STATE_TYPE_POSITION = 3,
        MP_STATE_TYPE_PLANET = 4,
        MP_STATE_TYPE_GAME = 5,
        MP_STATE_TYPE_ITEM = 6
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

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketHeader
    {
        [FieldOffset(0x0)] public MPPacketType ptype;
        [FieldOffset(0x2)] public MPPacketFlags flags;
        [FieldOffset(0x4)] public UInt32 size;
        [FieldOffset(0x8)] public byte requiresAck;
        [FieldOffset(0x9)] public byte ackCycle;
        [FieldOffset(0xb)] public byte pad;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketMobyUpdate
    {
        [FieldOffset(0x0)] public ushort uuid;
        [FieldOffset(0x2)] public ushort parent;
        [FieldOffset(0x4)] public byte team;
        [FieldOffset(0x5)] public byte reserved;
        [FieldOffset(0x6)] public MPMobyFlags flags;
        [FieldOffset(0x8)] public ushort oClass;
        [FieldOffset(0xa)] public ushort level;
        [FieldOffset(0xc)] public Int32 animationID;
        [FieldOffset(0x10)] public Int32 animationDuration;
        [FieldOffset(0x14)] public float x;
        [FieldOffset(0x18)] public float y;
        [FieldOffset(0x1c)] public float z;
        [FieldOffset(0x20)] public float rotation;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketMobyCreate
    {
        [FieldOffset(0x0)] public UInt32 uuid;
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

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketSetState
    {
        [FieldOffset(0x0)] public MPStateType stateType;
        [FieldOffset(0x4)] public uint offset;
        [FieldOffset(0x8)] public uint value;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketSetStateFloat
    {
        [FieldOffset(0x0)] public MPStateType stateType;
        [FieldOffset(0x4)] public uint offset;
        [FieldOffset(0x8)] public float value;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketSetHUDText
    {
        [FieldOffset(0x0)] public ushort id;
        [FieldOffset(0x2)] public ushort x;
        [FieldOffset(0x4)] public ushort y;
        [FieldOffset(0x6)] public ushort flags;
        [FieldOffset(0x8)] public uint color;
        [FieldOffset(0xc)] public ushort box_height;
        [FieldOffset(0x10)] public ushort box_width;
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

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketControllerInput
    {
        [FieldOffset(0x0)] public ushort input;
        [FieldOffset(0x2)] public MPControllerInputFlags flags;
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

            //MPPacketSetState damageState = new MPPacketSetState();
            //damageState.stateType = MPStateType.MP_STATE_TYPE_DAMAGE;
            //damageState.value = 1;  // Damage player by 1 health

            var size = Marshal.SizeOf(bonkState);// + Marshal.SizeOf(damageState);
            header.size = (uint)size;
            List<byte> bytes = new List<byte>();
            
            bytes.AddRange(StructToBytes<MPPacketSetState>(bonkState, Endianness.BigEndian));
            //bytes.AddRange(StructToBytes<MPPacketSetState>(damageState, Endianness.BigEndian));

            return (header, bytes.ToArray());
        }

        public static (MPPacketHeader, byte[]) MakeSetHUDTextPacket(ushort id, string text, ushort x, ushort y, uint color)
        {
            if (text.Length >= 50)
            {
                return (new MPPacketHeader(), null);
            }

            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_HUD_TEXT;
            header.requiresAck = 255;
            header.ackCycle = 255;

            MPPacketSetHUDText hudText = new MPPacketSetHUDText();
            hudText.x = x;
            hudText.y = y;
            hudText.color = color;
            hudText.flags = 1;  // Drop shadow
            hudText.id = id;

            header.size = (uint)Marshal.SizeOf(hudText) + 50;

            //List<byte> buffer = new List<byte>((int)header.size);
            byte[] buffer = new byte[(int)header.size];
            StructToBytes<MPPacketSetHUDText>(hudText, Endianness.BigEndian).ToList().CopyTo(buffer, 0);
            //text.ToList().CopyTo(buffer, Marshal.SizeOf(hudText));
            Encoding.ASCII.GetBytes(text).CopyTo(buffer, Marshal.SizeOf(hudText));
            

            return (header, buffer);
        }

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

            header.size = (uint)Marshal.SizeOf<MPPacketMobyCreate>();

            return (header, StructToBytes<MPPacketMobyCreate>(body, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeMobyUpdatePacket(ushort id, Moby moby)
        {
            MPPacketMobyUpdate moby_update = new MPPacketMobyUpdate();

            moby_update.uuid = id;

            moby_update.flags |= moby.active ? MPMobyFlags.MP_MOBY_FLAG_ACTIVE : 0;
            moby_update.flags |= moby.collision ? 0 : MPMobyFlags.MP_MOBY_NO_COLLISION;
            moby_update.flags |= moby.mpUpdateFunc ? 0 : MPMobyFlags.MP_MOBY_FLAG_ORIG_UDPATE_FUNC;
            
            moby_update.parent = (ushort)0; // Parent isn't really used
            moby_update.oClass = (ushort)moby.oClass;
            moby_update.level = moby.Level() != null ? (ushort)moby.Level().GetGameID() : (ushort)0;
            moby_update.x = moby.x;
            moby_update.y = moby.y;
            moby_update.z = moby.z;
            moby_update.rotation = moby.rot;
            moby_update.animationID = moby.animationID;

            MPPacketHeader moby_header = new MPPacketHeader { ptype = MPPacketType.MP_PACKET_MOBY_UPDATE, size = (uint)Marshal.SizeOf<MPPacketMobyUpdate>() };

            return (moby_header, Packet.StructToBytes<MPPacketMobyUpdate>(moby_update, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeGoToPlanetPacket(int planet)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;

            MPPacketSetState destinationPlanetState = new MPPacketSetState();
            destinationPlanetState.stateType = MPStateType.MP_STATE_TYPE_PLANET;
            destinationPlanetState.value = (uint)planet;

            var size = Marshal.SizeOf(destinationPlanetState);// + Marshal.SizeOf(damageState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(destinationPlanetState, Endianness.BigEndian));
        }

        public static (MPPacketHeader, byte[]) MakeQuerySerserResponsePacket(List<Server> servers, byte ackId, byte ackCycle)
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

        public static (MPPacketHeader, byte[]) MakeSetItemPacket(ushort item, bool give)
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_SET_STATE;

            MPPacketSetState setItemState = new MPPacketSetState();
            setItemState.stateType = MPStateType.MP_STATE_TYPE_ITEM;
            setItemState.value = ((uint)(give ? 1 : 0) << 16) | (uint)item;

            var size = Marshal.SizeOf(setItemState);
            header.size = (uint)size;

            return (header, StructToBytes<MPPacketSetState>(setItemState, Endianness.BigEndian));
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

