using System;
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
        static int CLIENT_INACTIVE_TIMEOUT_SECONDS = 30;

        static List<Client> clients = new List<Client>();

        static List<uint> disconnectedClients = new List<uint>();

        static UdpClient server = null;

        static int playerCount = 0;

        public static void Tick(Object info)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            foreach (var client in clients.ToArray())
            {
                // Check if this client is still alive
                if (client.IsDisconnected())
                {
                    continue;
                }

                if (client.GetInactiveSeconds() > CLIENT_INACTIVE_TIMEOUT_SECONDS)
                {
                    Console.WriteLine($"Client {client.ID} inactive for more than {CLIENT_INACTIVE_TIMEOUT_SECONDS} seconds.");

                    // Notify client and delete client's mobys and their children
                    client.Disconnect();
                    playerCount -= 1;
                    Environment.Shared().DeleteMobys(moby => moby.parent == client);

                    continue;
                }

                // Update the client on what's happening
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
                    moby_update.level = moby.parent == null ? moby.level : moby.parent.GetMoby().level;
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

        public static void DistributePacket((MPPacketHeader, byte[]) packet)
        {
            foreach (var client in clients)
            {
                if (client.IsDisconnected()) continue;
                client.SendPacket(packet);
            }
        }

        static void NewPlayer(IPEndPoint endpoint)
        {
            playerCount += 1;

            int index = clients.Count;
            for (int i = 0; i < clients.Count; i++) 
            {
                if (clients[i].IsDisconnected())
                {
                    index = i;
                    break;
                }
            }

            Client client = new Client(endpoint, (uint)index);

            if (index >= clients.Count)
            {
                clients.Add(client);
            } else
            {
                clients[index] = client;
            }

            Console.WriteLine($"New player {client.ID} from {endpoint.ToString()}");
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
            server.Client.ReceiveTimeout = 1;


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
            long loopCount = 0;
            while (true)
            {
                loopCount += 1;
                Console.Write($"\r({playerCount} players) ");

                if (server.Available <= 0)
                {
                    continue;
                }

                try
                {
                    IPEndPoint clientEndpoint = null;
                    data = server.Receive(ref clientEndpoint);

                    if (data.Length <= 0)
                    {
                        Console.WriteLine("Hey, why is it 0?");
                        continue;
                    }

                    bool existingPlayer = false;
                    foreach (var p in clients)
                    {
                        if (!p.IsDisconnected() && p.GetEndpoint().Address.Equals(clientEndpoint.Address) && p.GetEndpoint().Port == clientEndpoint.Port)
                        {
                            p.ReceiveData(data);
                            existingPlayer = true;
                        }
                    }

                    if (!existingPlayer)
                    {
                        NewPlayer(clientEndpoint);
                    }
                } catch (SocketException e)
                {
                    //Console.WriteLine($"Socket error: {e.Message}");
                }
            }

            Console.WriteLine("Bye!");
        }
    }
}
