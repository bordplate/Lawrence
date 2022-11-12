using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace Lawrence
{

    public class Player
    {
        public IPEndPoint endpoint;
        public bool handshakeCompleted = false;

        public uint ID = 0;

        public int animationID = 0;

        public float x = 0.0f;
        public float y = 0.0f;
        public float z = 0.0f;
        public float rot = 0.0f;

        public UdpClient server;

        byte state = 0;

        public Player(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;


            
        }

        public void sendPacket(MPPacketHeader packetHeader, byte[] packetBody)
        {
            var bodyLen = 0;
            if (packetBody != null)
            {
                bodyLen = packetBody.Length;
            }
            byte[] packet = new byte[Marshal.SizeOf<MPPacketHeader>() + bodyLen];

            Packet.StructToBytes<MPPacketHeader>(packetHeader, Packet.Endianness.BigEndian).CopyTo(packet, 0);
            if (packetBody != null)
            {
                packetBody.CopyTo(packet, Marshal.SizeOf<MPPacketHeader>());
            }

            try
            {
                server.Client.SendTo(packet, endpoint);
            } catch (System.NullReferenceException e)
            {
                Console.WriteLine("Wow it's null");
            }
        }

        public void parsePacket(MPPacketHeader packetHeader, byte[] packetBody)
        {
            // TODO: Implement some sort of protection against anyone spoofing others.
            //       Ideally the handshake should start a session that the client can
            //          easily use to identify itself. Ideally without much computational
            //          overhead. 
            if ((!handshakeCompleted) && packetHeader.ptype == MPPacketType.MP_PACKET_SYN)
            {
                handshakeCompleted = true;

                Console.WriteLine("Player handshake complete.");

                sendPacket(new MPPacketHeader { ptype = MPPacketType.MP_PACKET_ACK }, null);
                return;
            }
            else if (!handshakeCompleted)
            {
                sendPacket(new MPPacketHeader { ptype = MPPacketType.MP_PACKET_IDKU }, null);
                return;
            }

            var packetSize = 0;
            if (packetHeader.size > 0 && packetHeader.size < 1024 * 8)
            {
                packetSize = (int)packetHeader.size;
            }

            switch (packetHeader.ptype)
            {
                case MPPacketType.MP_PACKET_SYN:
                    {
                        sendPacket(new MPPacketHeader { ptype = MPPacketType.MP_PACKET_ACK }, null);
                        break;
                    }
                case MPPacketType.MP_PACKET_MOBY_UPDATE:
                    {
                        MPPacketMobyUpdate update = Packet.BytesToStruct<MPPacketMobyUpdate>(packetBody, Packet.Endianness.BigEndian);

                        if (update.x != this.x || update.y != this.y || update.z != this.z || update.animationID != this.animationID || update.rotation != this.rot)
                        {
                            //Console.WriteLine($"UID {this.ID}, X: {update.x}, Y: {update.y}, Z: {update.z}, rot: {update.rotation}, animID: {update.animationID}");
                        }
                            

                        this.x = update.x;
                        this.y = update.y;
                        this.z = update.z;
                        this.rot = update.rotation;
                        this.animationID = update.animationID;

                        //Console.WriteLine($"Player is at (x: {x}, y: {y}, z: {z}, rot: {rot})");

                        break;
                    }
                default:
                    {
                        Console.WriteLine($"Player sent unknown packet {packetHeader.ptype} with size: {packetSize}.");
                        break;
                    }
            }

            //Console.WriteLine($"Processed packet {packetHeader.ptype} with size: {packetSize}.");
        }

        int num_req_bytes = 8;
        int buffer_offset = 0;
        MPPacketHeader? current_header = null;
        byte[] recv_buffer = new byte[(1024 * 8) + 8];
        public void ReceiveData(byte[] data)
        {
            int received = data.Length;

            data.CopyTo(recv_buffer, buffer_offset);
            
            num_req_bytes -= received;
            buffer_offset += received;

            if (current_header == null && num_req_bytes <= 0)
            {
                // Parse header and reset buffer to start receiving packet body.
                current_header = Packet.makeHeader(recv_buffer);
                num_req_bytes = (int)current_header.Value.size;

                // If size is too large, we just discard this.
                // Probably going to cause problems. 
                if (num_req_bytes > (1024 * 8))
                {
                    //Console.WriteLine($"Discarding packet {current_header.Value.ptype} with size: {current_header.Value.size}.");
                    num_req_bytes = Marshal.SizeOf<MPPacketHeader>();
                    current_header = null;
                    buffer_offset = 0;
                }
            }

            if (current_header is MPPacketHeader header && num_req_bytes <= 0)
            {
                // We have full packet at this point, parse it and process.
                parsePacket(header, recv_buffer.Skip(Marshal.SizeOf<MPPacketHeader>()).Take((int)current_header.Value.size).ToArray());

                // Reset values to start receiving new packet
                num_req_bytes = Marshal.SizeOf<MPPacketHeader>();
                buffer_offset = 0;
                current_header = null;
            }
        }
    }
}
