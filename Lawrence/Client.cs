using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Force.Crc32;

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

            recvLock = new Mutex();
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
                Console.WriteLine($"Player {this.ID} tried to update null moby {update.uuid}.");
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

        // Parse and process a packet
        public void ParsePacket(byte[] packet)
        {
            // Start out by reading the header
            MPPacketHeader packetHeader = Packet.makeHeader(packet.Take(Marshal.SizeOf<MPPacketHeader>()).ToArray());
            byte[] packetBody = packet.Skip(Marshal.SizeOf<MPPacketHeader>()).Take((int)packetHeader.size).ToArray();

            // Check if handshake is complete, otherwise start completing it. 

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
                // Client has sent a packet that is not a handshake packet.
                // We tell the client we don't know it and it should reset state and
                // start handshake. 
                SendPacket(new MPPacketHeader { ptype = MPPacketType.MP_PACKET_IDKU }, null);
                return;
            }

            // Get size of packet body 
            var packetSize = 0;
            if (packetHeader.size > 0 && packetHeader.size < 1024 * 8)
            {
                packetSize = (int)packetHeader.size;
            }

            // If this packet requires ack and is not RPC, we send ack before processing.
            // If this is RPC we only send ack here if a previous ack has been sent and cached.
            if (packetHeader.ptype != MPPacketType.MP_PACKET_ACK && packetHeader.requiresAck != 0)  // We don't ack ack messages
            {
                // If this is an RPC packet and we've already processed and cached it, we use the cached response. 
                if ((packetHeader.flags & MPPacketFlags.MP_PACKET_FLAG_RPC) != 0 && acked[packetHeader.requiresAck].ackCycle == packetHeader.ackCycle)
                {
                    Lawrence.SendTo(acked[packetHeader.requiresAck].packet, endpoint);
                }
                else if ((packetHeader.flags & MPPacketFlags.MP_PACKET_FLAG_RPC) == 0) // If it's not RPC, we just ack the packet and process the packet
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

                        this.clientMoby.active = update.enabled == 1;

                        if (!clientMoby.active)
                        {
                            Console.WriteLine("Sending player to a planet\n");
                            SendPacket(Packet.MakeGoToPlanetPacket(11));
                            break;
                        }
                                                
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
                case MPPacketType.MP_PACKET_MOBY_COLLISION:
                    {
                        MPPacketMobyCollision collision = Packet.BytesToStruct<MPPacketMobyCollision>(packetBody, Packet.Endianness.BigEndian);

                        ushort uuid = collision.uuid;
                        ushort collidedWith = collision.collidedWith;

                        if (uuid == 0)
                        {
                            uuid = clientMoby.UUID;
                        }

                        if (collidedWith == 0)
                        {
                            collidedWith = clientMoby.UUID;
                        }

                        if (uuid == collidedWith)
                        {
                            Console.WriteLine($"Player {ID} just told us they collided with themselves.");
                            return;
                        }

                        Moby moby = Environment.Shared().GetMoby(uuid);
                        if (moby != null)
                        {
                            moby.AddCollider(collidedWith, collision.flags);
                        } else
                        {
                            Console.WriteLine($"Player {ID} claims they hit null-moby {uuid}");
                        }

                        break;
                    }
                default:
                    {
                        Console.WriteLine($"(Player {ID}) sent unknown (possibly malformed) packet {packetHeader.ptype} with size: {packetSize}. Resetting receive buffer.");
                        //recvIndex = 0;
                        break;
                    }
            }
        }

        Mutex recvLock;
        bool resetBuffer = false;
        List<byte[]> recvBuffer = new List<byte[]>(100);
        List<uint> lastHashes = new List<uint>(10);
        public void ReceiveData(byte[] data)
        {
            if (disconnected)
            {
                return;
            }

            this.lastContact = DateTimeOffset.Now.ToUnixTimeSeconds();

            recvLock.WaitOne();

            if (resetBuffer)
            {
                recvBuffer = new List<byte[]>(100);
            }

            uint currentHash = Crc32Algorithm.Compute(data);

            // Throw away duplicate packets
            if (lastHashes.Contains(currentHash))
            {
                recvLock.ReleaseMutex();
                return;
            }

            // We store the crc32 sum of the last 10 packets
            // So we can discard future packets if they are the same sum
            // We send a lot of duplicate packets and we should be able to endure
            // any packet loss. So this should help us not process a bunch of redundant packets.
            if (lastHashes.Count >= 10)
            {
                lastHashes.Remove(0);
            }

            lastHashes.Add(currentHash);

            recvBuffer.Add(data);

            recvLock.ReleaseMutex();

            if (recvBuffer.Count > 50)
            {
                Console.WriteLine($"(Player {ID}) has a shit ton of packets lmao");
                recvBuffer = new List<byte[]>(100);
            }
        }

        List<byte[]> DrainPackets()
        {
            if (recvBuffer.Count <= 0)
            {
                // We don't have enough bytes to build a header
                return null;
            }

            // We take at max 50 packets out of the buffer
            int takePackets = Math.Min(50, recvBuffer.Count);

            if (takePackets <= 0)
            {
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

        public void Tick()
        {
            if (disconnected)
            {
                return;
            }

            var packets = DrainPackets();
            if (packets != null)
            {
                foreach (var packet in packets)
                {
                    ParsePacket(packet);
                }
            }

            // Resend unacked packets
            foreach (var unacked in this.unacked.ToArray())
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
