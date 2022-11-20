using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Lawrence
{
    // Metadata structure for packet acknowledgement data
    public struct AckedMetadata
    {
        public byte ackCycle;
        public byte ackIndex;
        public byte[] packet;
    }
    
    public class Client
    {
        IPEndPoint endpoint;
        bool handshakeCompleted = false;

        public uint ID = 0;

        long lastContact = 0;

        bool disconnected = true;

        // All connected clients should have an associated client moby. 
        Moby clientMoby;

        AckedMetadata[] acked = new AckedMetadata[256];

        byte ackCycle = 1;
        byte ackIndex = 1;
        List<AckedMetadata> unacked = new List<AckedMetadata>();

        public Client(IPEndPoint endpoint, uint ID)
        {
            this.ID = ID;
            this.endpoint = endpoint;
            this.lastContact = DateTimeOffset.Now.ToUnixTimeSeconds();
            this.disconnected = false;

            clientMoby = Environment.Shared().NewMoby(this);
        }

        public IPEndPoint GetEndpoint()
        {
            return endpoint;
        }


        // Amount of seconds since we last saw activity on this client.
        public long GetInactiveSeconds()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds() - this.lastContact;
        }

        public bool IsDisconnected()
        {
            return disconnected;
        }

        public Moby GetMoby()
        {
            return clientMoby;
        }

        public void UpdateMoby(MPPacketMobyUpdate update)
        {
            Moby moby = Environment.Shared().GetMoby(update.uuid);

            if (moby == null)
            {
                Console.WriteLine($"Player {this.ID} tried to update null moby {moby.UUID}.");
                return;
            }

            if (moby.parent != this)
            {
                Console.WriteLine($"Player {this.ID} tried to update moby {moby.UUID} that does not belong to them.");
                return;
            }

            if (update.enabled != 0 && !moby.active && moby.state == 0)
            {
                moby.active = true;
                moby.state = 1;

                Console.WriteLine($"Got first update for {moby.UUID} (oClass: {moby.oClass}) from player {this.ID}");
            } else if (update.oClass != moby.oClass)
            {
                Console.WriteLine($"Changed oClass for {moby.UUID}, from {moby.oClass} to {update.oClass}");
            }

            moby.oClass = (int)update.oClass;
            moby.active = update.enabled != 0;
            moby.x = update.x;
            moby.y = update.y;
            moby.z = update.z;
            moby.level = update.level;
            moby.animationID = update.animationID;
            moby.rot = update.rotation;

            return;
        }

        (byte, byte) NextAck()
        {
            if (ackIndex >= 254)
            {
                ackIndex = 0;
                ackCycle++;
            }

            if (ackCycle >= 254)
            {
                ackCycle = 0;
            }

            return (ackIndex++, ackCycle);
        }

        public void SendPacket(MPPacketHeader packetHeader, byte[] packetBody)
        {
            var bodyLen = 0;
            if (packetBody != null)
            {
                bodyLen = packetBody.Length;
            }
            byte[] packet = new byte[Marshal.SizeOf<MPPacketHeader>() + bodyLen];

            // Fill ack fields if necessary
            if (packetHeader.requiresAck == 255 && packetHeader.ackCycle == 255)
            {
                (packetHeader.requiresAck, packetHeader.ackCycle) = NextAck();
            }

            Packet.StructToBytes<MPPacketHeader>(packetHeader, Packet.Endianness.BigEndian).CopyTo(packet, 0);
            if (packetBody != null)
            {
                packetBody.CopyTo(packet, Marshal.SizeOf<MPPacketHeader>());
            }

            // Cache ack response packets
            if (packetHeader.ptype == MPPacketType.MP_PACKET_ACK && packetHeader.requiresAck != 0)
            {
                var ack = acked[packetHeader.requiresAck];
                if (ack.ackCycle != packetHeader.ackCycle)
                {
                    ack.ackCycle = packetHeader.ackCycle;
                    ack.packet = packet;
                    Console.WriteLine($"Caching ack response");
                }

                acked[packetHeader.requiresAck] = ack;
            }

            // Cache unacked request packets
            if (packetHeader.ptype != MPPacketType.MP_PACKET_ACK && packetHeader.requiresAck != 0)
            {
                unacked.Add(new AckedMetadata { packet = packet, ackIndex = packetHeader.requiresAck, ackCycle = packetHeader.ackCycle } );
            }

            Lawrence.SendTo(packet, endpoint);
        }

        public void SendPacket((MPPacketHeader packetHeader, byte[] packetBody) packet)
        {
            (MPPacketHeader header, byte[] body) = packet;

            SendPacket(header, body);
        }

        public void ParsePacket(byte[] packet)
        {
            MPPacketHeader packetHeader = Packet.makeHeader(packet.Take(Marshal.SizeOf<MPPacketHeader>()).ToArray());
            byte[] packetBody = packet.Skip(Marshal.SizeOf<MPPacketHeader>()).Take((int)packetHeader.size).ToArray();

            // TODO: Implement some sort of protection against anyone spoofing others.
            //       Ideally the handshake should start a session that the client can
            //          easily use to identify itself. Ideally without much computational
            //          overhead. 
            if ((!handshakeCompleted) && packetHeader.ptype == MPPacketType.MP_PACKET_SYN)
            {
                handshakeCompleted = true;

                Console.WriteLine("Player handshake complete.");

                SendPacket(Packet.MakeAckPacket());
                return;
            }
            else if (!handshakeCompleted)
            {
                SendPacket(new MPPacketHeader { ptype = MPPacketType.MP_PACKET_IDKU }, null);
                return;
            }

            var packetSize = 0;
            if (packetHeader.size > 0 && packetHeader.size < 1024 * 8)
            {
                packetSize = (int)packetHeader.size;
            }

            // If this packet requires ack and is not RPC, we send ack before processing.
            // If this is RPC we only send ack here if a previous ack has been sent and cached.

            if (packetHeader.ptype != MPPacketType.MP_PACKET_ACK)  // We don't ack ack messages
            {
                if (packetHeader.requiresAck != 0 && (packetHeader.flags & MPPacketFlags.MP_PACKET_FLAG_RPC) > 0 && acked[packetHeader.requiresAck].ackCycle == packetHeader.ackCycle)
                {
                    Lawrence.SendTo(acked[packetHeader.requiresAck].packet, endpoint);
                }
                else if (packetHeader.requiresAck != 0 && (packetHeader.flags & MPPacketFlags.MP_PACKET_FLAG_RPC) == 0)
                {
                    MPPacketHeader ack = new MPPacketHeader
                    {
                        ptype = MPPacketType.MP_PACKET_ACK,
                        flags = 0,
                        size = 0,
                        requiresAck = packetHeader.requiresAck,
                        ackCycle = packetHeader.ackCycle
                    };

                    SendPacket(ack, null);
                }
            }


            switch (packetHeader.ptype)
            {
                case MPPacketType.MP_PACKET_SYN:
                    {
                        SendPacket(new MPPacketHeader { ptype = MPPacketType.MP_PACKET_ACK, ackCycle = 0, requiresAck = 0 }, null);
                        break;
                    }
                case MPPacketType.MP_PACKET_ACK:
                    foreach (var unacked in this.unacked.ToArray())
                    {
                        if (unacked.ackCycle == packetHeader.ackCycle && unacked.ackIndex == packetHeader.requiresAck)
                        {
                            this.unacked.Remove(unacked);
                        }
                    }
                    break;
                case MPPacketType.MP_PACKET_MOBY_UPDATE:
                    {
                        MPPacketMobyUpdate update = Packet.BytesToStruct<MPPacketMobyUpdate>(packetBody, Packet.Endianness.BigEndian);

                        if (update.uuid != 0)
                        {
                            UpdateMoby(update);

                            break;
                        }

                        this.clientMoby.active = true;
                                                
                        this.clientMoby.x = update.x;
                        this.clientMoby.y = update.y;
                        this.clientMoby.z = update.z;
                        this.clientMoby.level = update.level;
                        this.clientMoby.rot = update.rotation;
                        this.clientMoby.animationID = update.animationID;

                        break;
                    }
                case MPPacketType.MP_PACKET_MOBY_CREATE:
                    {
                        Moby moby = Environment.Shared().NewMoby(this);

                        MPPacketMobyCreate createPacket = new MPPacketMobyCreate
                        {
                            uuid = moby.UUID
                        };

                        MPPacketHeader header = new MPPacketHeader {
                            ptype = MPPacketType.MP_PACKET_ACK,
                            flags = MPPacketFlags.MP_PACKET_FLAG_RPC,
                            size = (uint)Marshal.SizeOf<MPPacketMobyCreate>(),
                            requiresAck = packetHeader.requiresAck,
                            ackCycle = packetHeader.ackCycle
                        };

                        Console.WriteLine($"Player({this.ID}) created moby (uuid: {moby.UUID})");

                        SendPacket(header, Packet.StructToBytes<MPPacketMobyCreate>(createPacket, Packet.Endianness.BigEndian));

                        break;
                    }
                default:
                    {
                        Console.WriteLine($"Player sent unknown packet {packetHeader.ptype} with size: {packetSize}.");
                        break;
                    }
            }
        }

        int recvIndex = 0;
        byte[] recvBuffer = new byte[(1024 * 8) + 12];
        bool recvLock = false;
        public void ReceiveData(byte[] data)
        {
            if (disconnected)
            {
                return;
            }

            // FIXME: This is potentially super slow. Idk if there's a better way to do multi-threading like this
            int lockCounter = 0;
            while (recvLock)
            {
                lockCounter++;
                if (lockCounter > 10000000)
                {
                    // Something has gone wrong, we just force unlock and hope it's ok.
                    Console.WriteLine($"Disconnected: {ID}");
                    recvLock = false;
                }
            }

            this.lastContact = DateTimeOffset.Now.ToUnixTimeSeconds();

            recvLock = true;

            int received = data.Length;

            if (recvIndex+received > recvBuffer.Length)
            {
                Console.WriteLine($"Player's ({this.ID} buffer offset + received data higher than buffer size. Resetting.");
                recvIndex = 0;
            }

            if (recvIndex < 0)
            {
                Console.WriteLine($"recvIndex was {recvIndex}. Setting to 0");
                recvIndex = 0;
            }

            data.CopyTo(recvBuffer, recvIndex);

            recvIndex += received;

            recvLock = false;
        }

        // Returns and drains the receive buffer if a full packet is available.
        byte[] DrainPacket()
        {
            if (recvLock || recvIndex < Marshal.SizeOf<MPPacketHeader>())
            {
                // We don't have enough bytes to build a header
                return null;
            }

            recvLock = true;

            MPPacketHeader header = Packet.makeHeader(recvBuffer.Take(Marshal.SizeOf<MPPacketHeader>()).ToArray());

            if (header.size > recvIndex-Marshal.SizeOf<MPPacketHeader>())
            {
                // We don't have the full packet payload
                recvLock = false;
                return null;
            }

            int packetSize = (int)(Marshal.SizeOf<MPPacketHeader>() + header.size);

            // NOTE: These Take(), CopyTo(), Skip(), and ToArray() calls are expensive. Easy optimization
            byte[] packet = recvBuffer.Take(packetSize).ToArray();
            recvBuffer.Skip(packetSize).ToArray().CopyTo(recvBuffer, 0);
            recvIndex -= packetSize;

            recvLock = false;

            return packet;
        }

        public void Tick()
        {
            if (disconnected)
            {
                return;
            }

            var packet = DrainPacket();
            int processedPackets = 0;
            while (packet != null && processedPackets < 10)
            {
                ParsePacket(packet);
                processedPackets += 1;

                packet = DrainPacket();
            }

            // Resend unacked packets
            foreach (var unacked in this.unacked)
            {
                Lawrence.SendTo(unacked.packet, endpoint);
            }
        }

        public void Disconnect()
        {
            if (disconnected)
            {
                return;
            }

            // Send a disconnect packet. Don't really care if they receive it.
            SendPacket(Packet.MakeDisconnectPacket());
            disconnected = true;
        }
    }
}
