using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Lawrence
{
    class Program
    {
        static Dictionary<uint, Player> players = new Dictionary<uint, Player>();
        static Dictionary<uint, uint> playerIDs = new Dictionary<uint, uint>();

        static List<Moby> mobys = new List<Moby>();

        static UdpClient server = null;

        public static void Tick(Object info)
        {
            foreach (var client in players.Values)
            {
                foreach(var player in players.Values)
                {
                    foreach (var moby in mobys)
                    {
                        MPPacketMobyUpdate moby_update = new MPPacketMobyUpdate();

                        moby_update.uuid = moby.UUID;
                        moby_update.oClass = 0;
                        moby_update.x = moby.x;
                        moby_update.y = moby.y;
                        moby_update.z = moby.z;
                        moby_update.rotation = moby.rot;

                        MPPacketHeader moby_header = new MPPacketHeader { ptype = MPPacketType.MP_PACKET_MOBY_UPDATE, size = (uint)Marshal.SizeOf<MPPacketMobyUpdate>() };

                        //player.sendPacket(moby_header, Packet.StructToBytes<MPPacketMobyUpdate>(moby_update, Packet.Endianness.BigEndian));
                    }

                    if (client.ID == player.ID)
                    {
                        continue;
                    }

                    MPPacketMobyUpdate update = new MPPacketMobyUpdate();

                    update.oClass = 0;
                    update.uuid = player.ID;

                    update.animationID = player.animationID;

                    update.x = player.x;
                    update.y = player.y;
                    update.z = player.z;
                    update.rotation = player.rot;

                    

                    MPPacketHeader header = new MPPacketHeader { ptype = MPPacketType.MP_PACKET_MOBY_UPDATE, size = (uint)Marshal.SizeOf<MPPacketMobyUpdate>() };
                    client.sendPacket(header, Packet.StructToBytes<MPPacketMobyUpdate>(update, Packet.Endianness.BigEndian));
                }
            }
        }

        static Player NewPlayer(uint playerID, IPEndPoint endpoint)
        {
            Console.WriteLine($"New player from {endpoint.ToString()}");

            uint index = (uint)players.Count;
            for (uint i = 0; i < players.Count; i++) 
            {
                if (players[i] == null)
                {
                    index = i;
                    break;
                }
            }

            players[index] = new Player(endpoint);
            playerIDs[index] = playerID;

            players[index].ID = index;

            return players[index];
        }

        static void Main(string[] args)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 2407);
            server = new UdpClient(ipep);
            server.Client.Blocking = false;
            
            Moby test = new Moby();
            test.UUID = 10;
            test.x = 245.78f;
            test.y = 149.57f;
            test.z = 140.34f;
            test.rot = -0.784f;

            mobys.Add(test);

            Timer tickTimer = new Timer(Program.Tick, null, 1000, 8);

            Console.WriteLine("Starting Lawrence...");
            
            byte[] data;
            while (true)
            {
                if (server.Available <= 0)
                {
                    continue;
                }

                foreach(var player in players.Values)
                {
                    try
                    {
                        //player.ReceiveData(server);
                    } catch (SocketException e)
                    {
                        Console.WriteLine($"SocketException thrown.");
                    }
                }

                if (server.Available < 8)
                {
                    continue;
                }

                try
                {
                    IPEndPoint clientEndpoint = null;

                    data = server.Receive(ref clientEndpoint);

                    bool existingPlayer = false;
                    foreach (var p in players.Values)
                    {
                        if (p.endpoint.Address.Address == clientEndpoint.Address.Address && p.endpoint.Port == clientEndpoint.Port)
                        {
                            p.ReceiveData(data);
                            existingPlayer = true;
                        }
                    }

                    if (existingPlayer)
                    {
                        //Console.WriteLine("Shouldn't end up here dude.");
                        continue;
                    }

                    uint playerID = (uint)playerIDs.Count() + 1;

                    Player player;
                   
                    if (!playerIDs.ContainsKey(playerID))
                    {
                        player = NewPlayer(playerID, clientEndpoint);
                        player.server = server;
                    }
                    else
                    {
                        player = players[playerIDs[playerID]];
                    }
                } catch (SocketException e)
                {

                }
            }
        }
    }
}
