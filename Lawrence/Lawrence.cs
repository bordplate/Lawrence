﻿using System;
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
        public static int CLIENT_INACTIVE_TIMEOUT_SECONDS = 30;

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

            if (Settings.Default().Get<bool>("Server.directoryMode", false) || Array.IndexOf(args, "--directory") >= 0)
            {
                directoryMode = true;
                directory = new ServerDirectory();

                directory.RegisterServer("10.9.0.2", 2407, "Vetle's server", 20, 0);
                directory.RegisterServer("127.0.0.1", 2407, "localhost", 20, 0);
                directory.RegisterServer("10.9.0.5", 2407, "Someone's server", 3000, 1368);
            }

            int serverPort = Settings.Default().Get<int>("Server.port", 2407);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, serverPort);
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
                            NewClient(clientEndpoint, data);
                        }
                    }
                    catch (SocketException e)
                    {
                        Logger.Error($"Receive error", e);
                    }
                }
            }).Start();

            new Thread(() =>
            { 
                while (true)
                {
                    Stopwatch sw = new Stopwatch();

                    sw.Start();

                    // Clients that are waiting to connect aren't part of a `Player` tick loop so the Client tick 
                    //   isn't being called. Therefore we call the Tick function here if it's waiting to connect, so
                    //   packets can be processed like they should on the right thread. 
                    foreach (Client client in clients) {
                        if (client.WaitingToConnect) {
                            client.Tick();
                        }
                    }

                    Tick();

                    sw.Stop();

                    if (sw.ElapsedMilliseconds > 16)
                    {
                        Logger.Trace($"Tick running late: Elapsed={sw.ElapsedMilliseconds}");
                    }
                    else
                    {
                        Thread.Sleep((int)(16 - sw.ElapsedMilliseconds));
                    }
                }
            }).Start();

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
