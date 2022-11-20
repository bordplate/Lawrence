using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Lawrence
{
    public enum MPPacketType : ushort
    {
        MP_PACKET_CONNECT = 1,
        MP_PACKET_SYN = 2,
        MP_PACKET_ACK = 3,
        MP_PACKET_MOBY_UPDATE = 4,
        MP_PACKET_IDKU = 5,
        MP_PACKET_MOBY_CREATE = 6
    }

    public enum MPPacketFlags : ushort
    {
        MP_PACKET_FLAG_RPC = 0x1
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
        [FieldOffset(0x4)] public UInt32 enabled;
        [FieldOffset(0x8)] public ushort oClass;
        [FieldOffset(0xa)] public ushort level;
        [FieldOffset(0xc)] public Int32 animationID;
        [FieldOffset(0x10)] public float x;
        [FieldOffset(0x14)] public float y;
        [FieldOffset(0x18)] public float z;
        [FieldOffset(0x1c)] public float rotation;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MPPacketMobyCreate
    {
        [FieldOffset(0x0)] public UInt32 uuid;
    }


    public class Packet
	{
		public Packet()
		{
		}

        public static byte[] makeAckPacket()
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_ACK;

            return headerToBytes(header);
        }

        public static byte[] makeIDKUPacket()
        {
            MPPacketHeader header = new MPPacketHeader();
            header.ptype = MPPacketType.MP_PACKET_IDKU;

            return headerToBytes(header);
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

                if (subFields.Length == 0)
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

    }
}

