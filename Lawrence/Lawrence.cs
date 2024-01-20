using System;
using System.Threading;

using Lawrence.Core;

namespace Lawrence;

class Lawrence {
    public static int CLIENT_INACTIVE_TIMEOUT_SECONDS = 30;
    
    private static DirectoryClient _directoryClient;
    
    private static Server _server;
    private static DirectoryServer _directoryServer;

    private static bool _directoryMode;

    public static bool DirectoryMode()
    {
        return _directoryMode;
    }

    public static void ForceDirectorySync() {
        _directoryClient?.ForceDirectorySync();
    }

    public static ServerDirectory Directory()
    {
        return _directoryServer.Directory();
    }

    public static Server Server() {
        return _server ??= _directoryServer.Server();
    }
    
    static void Main(string[] args)
    {
        // Make space above last entry in log so there's a nice split between last run of the program
        Logger.Raw("\n");
        string serverName = Settings.Default().Get("Server.name", "");

        if (Settings.Default().Get("Server.directoryMode", false, true) || Array.IndexOf(args, "--directory") >= 0) {
            _directoryMode = true;
        }

        int serverPort = _directoryMode ? 2407 : Settings.Default().Get("Server.port", 2407);
        int maxPlayers = Settings.Default().Get("Server.max_players", 10);
        string listenAddress = Settings.Default().Get("Server.address", "0.0.0.0");
        
        bool advertise = Settings.Default().Get("Server.advertise", false);

        if (!_directoryMode && serverName == null || serverName.Trim().Length <= 0) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Server Name not set, Enter name...");
            Console.ResetColor();
            Settings.Default().Set("Server.name", serverName = Console.ReadLine());
        }

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

        if (!_directoryMode) {
            _server = new Server(listenAddress, serverPort, serverName, maxPlayers);
            _server.Start();
            
            Logger.Log($"Started Lawrence on {_server.ListenAddress()}");
            
            if (advertise) {
                string directoryIP = Settings.Default().Get("Server.directory_server", "172.104.144.15", true);
                Logger.Log($"Registering server with directory @ {directoryIP}");
            
                _directoryClient = new(directoryIP, _server);
                _directoryClient.Start();
            }
        } else {
            _directoryServer = new DirectoryServer(listenAddress, serverPort);
            _directoryServer.Start();
            
            Logger.Log($"Started Lawrence Directory on {_directoryServer.ListenAddress()}");
        }

        while (true) {
            Thread.Sleep(16);

            if (!Environment.UserInteractive) {
                continue;
            }
            
            Console.Write("\r> ");
            string command = Console.ReadLine();

            if (command != null) {
                Console.WriteLine(Game.Game.Shared().Execute(command));
            }
        }
    }
}
