using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using Terminal.Gui;

namespace Lawrence
{
    class Lawrence
    {
        public static double LAST_TICK_TIME_MS = 0;
        public static int CLIENT_INACTIVE_TIMEOUT_SECONDS = 30;
        
        const double TargetTickDurationMs = 1000.0 / 60.0; // 16.67 ms per tick for 60 ticks per second
        
        static double _totalTickDurationMs = 0;    // Total tick duration in milliseconds
        static long _tickCount = 0;             // Total number of ticks
        static DateTime _lastAverageUpdateTime = DateTime.UtcNow;
        private static double _ticksPerSecond = 0.0;

        static List<Client> clients = new List<Client>();

        static UdpClient server = null;

        static bool directoryMode = false;
        static ServerDirectory directory = null;

        public static bool DirectoryMode()
        {
            return directoryMode;
        }

        public static ServerDirectory Directory()
        {
            return directory;
        }

        public static void Tick()
        {
            Game.Shared().Tick();
        }
        
        static void NewClient(IPEndPoint endpoint, byte[] data = null)
        {
            Logger.Log($"Connection from {endpoint}");
            
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
            }
            else {
                clients[index] = client;
            }

            // Receive their first packet
            if (data != null)
            {
                client.ReceiveData(data);
            }
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

        public static Client GetClient(int ID)
        {
            foreach(Client client in clients)
            {
                if (client.ID == ID)
                {
                    return client;
                }
            }

            return null;
        }

        public static List<Client> GetClients()
        {
            return clients;
        }

        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        static void Main(string[] args)
        {
            // Make space above last entry in log so there's a nice split between last run of the program
            Logger.Raw("\n");
            string serverName = Settings.Default().Get("Server.name", "");

            if (Settings.Default().Get<bool>("Server.directoryMode", false, true) || Array.IndexOf(args, "--directory") >= 0)
            {
                directoryMode = true;
                directory = new ServerDirectory();
            }

            int serverPort = directoryMode ? 2407 : Settings.Default().Get<int>("Server.port", 2407);
            string listenAddress = Settings.Default().Get("Server.address", "0.0.0.0");
            
            bool advertise = Settings.Default().Get("Server.advertise", false);

            if (!directoryMode && serverName == null || serverName.Trim().Length <= 0) {
                throw new Exception("You must set a name for the server. Check settings.toml for configuration");
            }

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(listenAddress), serverPort);
            server = new UdpClient(ipep);

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

            Logger.Log($"Started Lawrence on {ipep.ToString()}");

            if (directoryMode)
            {
                Logger.Log($"Directory mode enabled!");
            }

            new Thread(() =>
            {
                byte[] data;
                long loopCount = 0;
                long dataReceived = 0;

                while (true)
                {
                    loopCount += 1;

                    if (server.Available <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    try
                    {
                        IPEndPoint clientEndpoint = null;
                        data = server.Receive(ref clientEndpoint);
                        dataReceived += data.Length;

                        if (data.Length <= 0)
                        {
                            Logger.Error("Hey, why is it 0?");
                            continue;
                        }

                        bool existingPlayer = false;
                         
                        for (int i = clients.Count - 1; i >= 0; i--) {
                            Client p = clients[i];
                            
                            // FIXME: Check if client is disconnected and remove them from clients list.

                            if (!p.IsDisconnected() && p.GetEndpoint().Address.Equals(clientEndpoint.Address) && p.GetEndpoint().Port == clientEndpoint.Port)
                            {
                                p.ReceiveData(data);
                                existingPlayer = true;
                            }
                        }

                        if (!existingPlayer)
                        {
                            NewClient(clientEndpoint, data);
                        }
                    }
                    catch (SocketException e)
                    {
                        Logger.Error($"Receive error", e);
                    }
                }
            }).Start();

            DateTime lastTickEndTime = DateTime.UtcNow;
            Stopwatch watch = new Stopwatch();
            watch.Start();

            DateTime lastDirectoryPing = DateTime.MinValue;
            
            Thread runLoop = new Thread(() => {
                while (true) {
                    // Clients that are waiting to connect aren't part of a `Player` tick loop so the Client tick 
                    //   isn't being called. Therefore we call the Tick function here if it's waiting to connect, so
                    //   packets can be processed like they should on the right thread. 
                    foreach (Client client in clients) {
                        if (client.WaitingToConnect) {
                            client.Tick();
                        }
                    }

                    if (!directoryMode) {
                        // We only process normally if we're not in directory mode.
                        Tick();
                    }

                    watch.Stop();
                    
                    double tickDuration = watch.ElapsedMilliseconds;
                    LAST_TICK_TIME_MS = tickDuration;

                    _totalTickDurationMs += tickDuration;
                    _tickCount++;

                    if (tickDuration > TargetTickDurationMs+1) {
                        Logger.Trace($"Tick running late: Elapsed={tickDuration}ms");
                    } else {
                        int sleepTime = (int)TargetTickDurationMs - (int)tickDuration;
                        if (sleepTime > 0) {
                            Thread.Sleep(sleepTime);
                        }
                    }

                    // Update average ticks per second every second
                    if ((DateTime.UtcNow - _lastAverageUpdateTime).TotalSeconds >= 1.0) {
                        double averageTickDurationMs = (double)_totalTickDurationMs / _tickCount;
                        _ticksPerSecond = _tickCount;

                        // Reset the counters
                        _totalTickDurationMs = 0;
                        _tickCount = 0;
                        _lastAverageUpdateTime = DateTime.UtcNow;
                    }

                    watch.Restart();
                }
            });

            runLoop.Start();

            if (!directoryMode && advertise) {
                string directoryIP = Settings.Default().Get("Server.directory_server", "172.104.144.15", true);
                Logger.Log($"Registering server with directory @ {directoryIP}");
                
                new Thread(() => {
                    UdpClient client = new UdpClient(directoryIP, 2407);
                    
                    while (true) {
                        if ((DateTime.Now - lastDirectoryPing).TotalMinutes >= 1.0) {
                            lastDirectoryPing = DateTime.Now;

                            string ipString = Settings.Default()
                                .Get<string>("Server.advertise_ip", ipep.Address.ToString(), true);
                            (MPPacketHeader, byte[]) packet = Packet.MakeRegisterServerPacket(ipString,
                                (ushort)serverPort, 0, 0, serverName);

                            List<byte> packetData = new List<byte>();
                            packetData.AddRange(Packet.StructToBytes(packet.Item1, Packet.Endianness.BigEndian));
                            packetData.AddRange(packet.Item2);

                            client.SendAsync(packetData.ToArray());
                        }
                        
                        Thread.Sleep(1000);
                    }
                }).Start();
            }

            while (true)
            {
                Console.Write("\r> ");
                string command = Console.ReadLine();

                if (command != null)
                {
                    Console.WriteLine(Game.Shared().Execute(command));
                }

                Thread.Yield();
            }
        }
    }
}
