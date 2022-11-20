﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Lawrence
{
    class Lawrence
    {
        static Dictionary<uint, Client> players = new Dictionary<uint, Client>();
        static Dictionary<uint, uint> playerIDs = new Dictionary<uint, uint>();

        static UdpClient server = null;

        public static void Tick(Object info)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            foreach (var client in players.Values)
            {
               foreach (var moby in Environment.Shared().GetMobys())
                {
                    if (moby.parent == client || !moby.active || moby.level != client.GetMoby().level)
                    {
                        continue;
                    }

                    MPPacketMobyUpdate moby_update = new MPPacketMobyUpdate();

                    moby_update.uuid = moby.UUID;
                    moby_update.parent = moby.parent != null ? moby.parent.GetMoby().UUID : (ushort)0;
                    moby_update.oClass = (ushort)moby.oClass;
                    moby_update.level = moby.level;
                    moby_update.x = moby.x;
                    moby_update.y = moby.y;
                    moby_update.z = moby.z;
                    moby_update.rotation = moby.rot;
                    moby_update.animationID = moby.animationID;

                    MPPacketHeader moby_header = new MPPacketHeader { ptype = MPPacketType.MP_PACKET_MOBY_UPDATE, size = (uint)Marshal.SizeOf<MPPacketMobyUpdate>() };

                    client.SendPacket(moby_header, Packet.StructToBytes<MPPacketMobyUpdate>(moby_update, Packet.Endianness.BigEndian));
                }

                client.Tick();
            }

            sw.Stop();

            if (sw.ElapsedMilliseconds > 16)
            {
                Console.WriteLine("Tick running late: Elapsed={0}", sw.ElapsedMilliseconds);
            }
        }

        static Client NewPlayer(uint playerID, IPEndPoint endpoint)
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

            players[index] = new Client(endpoint);
            playerIDs[index] = playerID;

            players[index].ID = index;

            return players[index];
        }

        public static void SendTo(byte[] bytes, EndPoint endpoint)
        {
            try
            {
                server.Client.SendTo(bytes, endpoint);
            } catch (Exception e)
            {
                Console.WriteLine($"Error sending packet: {e.Message}");
            } 
        }

        static void Main(string[] args)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 2407);
            server = new UdpClient(ipep);
            server.Client.Blocking = false;
            
            
            Timer tickTimer = new Timer(Lawrence.Tick, null, 1000, 8);

            Console.WriteLine("                                       -=*####***++++++=-                  ");
            Console.WriteLine("                                     +##%###****++====--                   ");
            Console.WriteLine("                                   .+#%%%%%##*****+==-:                    ");
            Console.WriteLine("                                 =#%%%%%%%%%##****+=-                      ");
            Console.WriteLine("                                +%%%@@@%%%%%%##***=.                       ");
            Console.WriteLine("                             .-%@@%%@@@@@%%%%%#*+:                         ");
            Console.WriteLine("                            *%@@@@@@@@@@@@%%%#+:                           ");
            Console.WriteLine("                           -%@@@@@@@@@@@@@%%%- ..                          ");
            Console.WriteLine("                           .#@@@@@@@@@@@@%%%@*-=--:                      - ");
            Console.WriteLine("                            .#%@@@@@@@@@%*:-*+===---:.                 =*- ");
            Console.WriteLine("                             .*%@@@@@@@#:  +*++++=-+:::.             =*=-: ");
            Console.WriteLine("                               *%@@@@%-    =***+#+-=: .=-.         -*+===-.");
            Console.WriteLine("                                *%@%=       :***%*+*+:-===-:     -***++++=.");
            Console.WriteLine("                                 :-         =****##%%*+=====-  :*###*####=.");
            Console.WriteLine("                                            =******+++++++==  +##########=.");
            Console.WriteLine("                                      .++:..  -+++==++**++%@#######%%%%#=: ");
            Console.WriteLine("                                     -@@@%#:         .:. .*#######%%%%%+:. ");
            Console.WriteLine("                                   -=#@@%%%%#:          =#%%%%%%%%%%%%*-.  ");
            Console.WriteLine("                                 -###*#@@@%%%*        -#%%%%%%%%%##%#+:.   ");
            Console.WriteLine("                               :*###*+++*%#--       -##%%%%%######%#=:.    ");
            Console.WriteLine("             ..:::---=---:::::*###*+++****:       .#%%%%%%%%%%%%%#+-:      ");
            Console.WriteLine("           =%@@@@@@@@@@@@@%%%%%###+++***-         .*#%%%%%%%%%@#+-:.       ");
            Console.WriteLine("         .#@@#=------=*#%@@@@%=:=*#***=             :+#%%%%%%#+-:.         ");
            Console.WriteLine("        -@@%=           .+%@@@+=+#%#+.                .=**+=-:.            ");
            Console.WriteLine("       +@%*.          .:###*#%@@@%*.                                       ");
            Console.WriteLine("     .#@%:          .:+##*+++***+.                                         ");
            Console.WriteLine("    :%%%:          =**+++++***+.                                           ");
            Console.WriteLine("   -@%%-         .*+=--=++**+.                                             ");
            Console.WriteLine("  :@%%#.       .##**++++***:                                               ");
            Console.WriteLine("  -@%%#.     .%%##*++++**=                                                 ");
            Console.WriteLine("  +@%%*:   .#%%#**+++**+.                                                  ");
            Console.WriteLine("  *@%%#+. .###*++++**+:                                                    ");
            Console.WriteLine("  =@@%%**##*+++++***:                                                      ");
            Console.WriteLine("   %@%%%#*+++++**+:                                                        ");
            Console.WriteLine("   -@@%##%#++**+:                                                          ");
            Console.WriteLine(" :+#@@%-:+#%**:                                                            ");
            Console.WriteLine("-#*+*@@#*#%%-                                                              ");
            Console.WriteLine(":#*=-=*#%#=                                                                ");
            Console.WriteLine(" .+#*+=++                                                                \n");

            Console.WriteLine($"Started Lawrence on {ipep.ToString()}");

            byte[] data;
            while (true)
            {
                if (server.Available <= 0)
                {
                    continue;
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
                        if (p.GetEndpoint().Address.Equals(clientEndpoint.Address) && p.GetEndpoint().Port == clientEndpoint.Port)
                        {
                            p.ReceiveData(data);
                            existingPlayer = true;
                        }
                    }

                    if (existingPlayer)
                    {
                        continue;
                    }

                    uint playerID = (uint)playerIDs.Count() + 1;

                    Client player;
                   
                    if (!playerIDs.ContainsKey(playerID))
                    {
                        player = NewPlayer(playerID, clientEndpoint);
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
