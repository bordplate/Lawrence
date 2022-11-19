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
    class Program
    {
        static Dictionary<uint, Player> players = new Dictionary<uint, Player>();
        static Dictionary<uint, uint> playerIDs = new Dictionary<uint, uint>();

        public static List<Moby> mobys = new List<Moby>();

        static UdpClient server = null;

        public static void Tick(Object info)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            foreach (var client in players.Values)
            {
               foreach (var moby in mobys)
                {
                    if (moby.parent == client || !moby.active)
                    {
                        continue;
                    }

                    MPPacketMobyUpdate moby_update = new MPPacketMobyUpdate();

                    moby_update.uuid = moby.UUID;
                    moby_update.parent = moby.parent != null ? moby.parent.GetMoby().UUID : (ushort)0;
                    moby_update.oClass = (uint)moby.oClass;
                    moby_update.x = moby.x;
                    moby_update.y = moby.y;
                    moby_update.z = moby.z;
                    moby_update.rotation = moby.rot;
                    moby_update.animationID = moby.animationID;

                    MPPacketHeader moby_header = new MPPacketHeader { ptype = MPPacketType.MP_PACKET_MOBY_UPDATE, size = (uint)Marshal.SizeOf<MPPacketMobyUpdate>() };

                    client.sendPacket(moby_header, Packet.StructToBytes<MPPacketMobyUpdate>(moby_update, Packet.Endianness.BigEndian));
                }

                client.Tick();
            }

            sw.Stop();

            if (sw.ElapsedMilliseconds > 16)
            {
                Console.WriteLine("Tick running late: Elapsed={0}", sw.ElapsedMilliseconds);
            }
        }

        public static Moby GetMoby(uint uuid)
        {
            return mobys[(int)uuid-1];
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

        public static Moby NewMoby(Player parent = null)
        {
            Moby moby = new Moby(parent);
            moby.UUID = (ushort)(mobys.Count+1);
            mobys.Add(moby);

            if (parent != null)
            {
                Console.WriteLine($"New moby (uid: {moby.UUID}). Parent: {parent?.ID}");
            } else
            {
                Console.WriteLine($"New moby (uid: {moby.UUID})");
            }

            return moby;
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

            //mobys.Add(test);

            // FIXME: We're not thread safe at all so this regularly causes crashes in things like arrays being
            //        edited while this tick thread is iterating through them. 
            Timer tickTimer = new Timer(Program.Tick, null, 1000, 8);

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
            Console.WriteLine(" .+#*+=++                                                                  \n");

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
                        if (p.endpoint.Address.Address == clientEndpoint.Address.Address && p.endpoint.Port == clientEndpoint.Port)
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
