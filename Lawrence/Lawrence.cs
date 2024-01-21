using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

using Lawrence.Core;
using Terminal.Gui;

using Command = Lawrence.Core.Command;

namespace Lawrence;

class Lawrence {
    public static int CLIENT_INACTIVE_TIMEOUT_SECONDS = 30;
    
    private static DirectoryClient _directoryClient;
    
    private static Server _server;
    private static DirectoryServer _directoryServer;

    private static bool _directoryMode;
    
    private static Dictionary<string, List<Command>> _commands = new();
    
    public static void RegisterCommand(string sectionName, Command command) {
        if (!_commands.ContainsKey(sectionName)) {
            _commands[sectionName] = new();
        }
        
        _commands[sectionName].Add(command);
    }
    
    public static void UnregisterCommand(Command command) {
        foreach (var section in _commands) {
            if (section.Value.Contains(command)) {
                section.Value.Remove(command);
            }
        }
    }

    public static Dictionary<string, List<Command>> Commands() {
        return _commands;
    }

    public static Command Command(string name) {
        foreach (var section in _commands) {
            foreach (var command in section.Value) {
                if (command.Name == name) {
                    return command;
                }
            }
        }

        return null;
    }

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

    public static void ConfigureCommands() {
        // Help command
        var helpCommand = new Command {
            Name = "help",
            Description = "Shows help for a command. If no command is specified, shows a list of all available commands.",
            Args = new[] { new Command.Arg { Name = "command", Optional = true } },
        };

        helpCommand.OnCommand += (args) => {
            if (args.Length <= 0) {
                Logger.Raw("Available commands:", false);
                foreach (var command in Commands()) {
                    Logger.Raw($"\n{command.Key}:", false);
                    
                    foreach (var subCommand in command.Value) {
                        Logger.Raw($"  {subCommand.Name} {string.Join(" ", subCommand.Args)}", false);
                    }
                }
            }
            else {
                var command = Command(args[0]);
                if (command == null) {
                    Logger.Raw($"Command `{args[0]}` not found.", false);
                    return;
                }

                Logger.Raw($"Usage: {command.Name} {string.Join(" ", command.Args)}\n\n{command.Description}", false);
            }
        };
        
        var exitCommand = new Command {
            Name = "exit",
            Description = "Shuts down and exits the server.",
            Args = new Command.Arg[] { },
        };
        
        exitCommand.OnCommand += _ => {
            Application.RequestStop();
        };

        RegisterCommand("General", helpCommand);
        RegisterCommand("General", exitCommand);
    }

    private static void SetupWizard() {
        Application.Init();

        var wizard = new Wizard ($"Server Setup");

        // Add 1st step
        var firstStep = new Wizard.WizardStep ("Server name");
        wizard.AddStep(firstStep);
        
        firstStep.NextButtonText = "Next";
        firstStep.HelpText = 
            """
            Set the name of your server. 
            
            If you're not sure about IP and port settings, just keep the defaults.
            
            If you want to advertise your server publicly, check the 'Advertise server' checkbox. This will make your server appear in the server list.
            
            You can use your mouse to navigate this user interface. Press tab to switch between fields. 
            """;
        
        var serverNameLabel = new Label ("Server name:") {
            AutoSize = true
        };
        var serverName = new TextField {
            X = Pos.Right(serverNameLabel) + 1,
            Width = Dim.Fill() - 1
        };
        
        var ipAddressLabel = new Label ("IP address:") {
            Y = 2,
            AutoSize = true
        };
        var ipAddress = new TextField {
            X = Pos.Right(ipAddressLabel) + 1,
            Y = 2,
            Width = Dim.Fill() - 1
        };
        ipAddress.Text = "0.0.0.0";
        
        var portLabel = new Label ("Port:") {
            Y = 3,
            AutoSize = true
        };
        var port = new TextField {
            X = Pos.Right(portLabel) + 1,
            Y = 3,
            Width = Dim.Fill() - 1
        };
        port.Text = "2407";
        
        var advertise = new CheckBox("Advertise server") {
            Y = 5,
            Width = Dim.Fill() - 1
        };
        
        firstStep.Add(serverNameLabel);
        firstStep.Add(serverName);
        firstStep.Add(ipAddressLabel);
        firstStep.Add(ipAddress);
        firstStep.Add(portLabel);
        firstStep.Add(port);
        firstStep.Add(advertise);
        
        // Add 2nd step
        var secondStep = new Wizard.WizardStep ("Enable mods");
        wizard.AddStep(secondStep);

        secondStep.HelpText = "Select the mods you want to enable for the server. Typically you only want to enable 1 mod at a time.";
        secondStep.NextButtonText = "Finish";
        
        var modsTable = new TableView {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var modsDataTable = new DataTable();
        modsDataTable.Columns.Add(" ");
        modsDataTable.Columns.Add("Name");
        modsDataTable.Columns.Add("Path");

        var mods = Game.Game.AllMods();

        foreach (var mod in mods) {
            modsDataTable.Rows.Add("[ ]", mod.Name(), mod.Path());
        }

        modsTable.Table = modsDataTable;
        
        modsTable.CellActivated += (args) => {
            if (args.Row < 0 || args.Row >= modsDataTable.Rows.Count) {
                return;
            }

            var enabled = modsDataTable.Rows[args.Row][0].ToString() == "[ ]" ? true : false;

            modsDataTable.Rows[args.Row][0] = enabled ? "[X]" : "[ ]";
            
            var mod = mods[args.Row];
            
            Settings.Default().Set($"Mod.{mod.CanonicalName()}.enabled", enabled);
            
            modsTable.SetNeedsDisplay();
        };
        
        secondStep.Add(modsTable);
        
        wizard.Finished += (args) =>
        {
            Settings.Default().Set("Server.name", serverName.Text.ToString());
            Settings.Default().Set("Server.address", ipAddress.Text.ToString());
            Settings.Default().Set("Server.port", int.Parse(port.Text.ToString()));
            Settings.Default().Set("Server.advertise", advertise.Checked);
            
            MessageBox.Query("Success", $"Setup complete", "Ok");
            
            Application.RequestStop();
        };

        Application.Top.Add (wizard);
        Application.Run ();
        Application.Shutdown ();
    }

    private static void Start(string[] args) {
                // If we're non-interactive, we should not hook console
        var interactive = Settings.Default().Get("Server.interactive", true, true);

        if (interactive) {
            Logger.HookConsole();
        }
        
        // Make space above last entry in log so there's a nice split between last run of the program
        Logger.Raw("\n");
        string serverName = Settings.Default().Get("Server.name", "");

        if (Settings.Default().Get("Server.directory_mode", false, true) || Array.IndexOf(args, "--directory") >= 0) {
            _directoryMode = true;
        }

        int serverPort = _directoryMode ? 2407 : Settings.Default().Get("Server.port", 2407);
        int maxPlayers = Settings.Default().Get("Server.max_players", 10);
        string listenAddress = Settings.Default().Get("Server.address", "0.0.0.0");
        
        bool advertise = Settings.Default().Get("Server.advertise", false);

        if (!_directoryMode && (serverName == null || serverName.Trim().Length <= 0)) {
            if (interactive) {
                SetupWizard();
            }
            else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Server Name not set, Enter name...");
                Console.ResetColor();
                Settings.Default().Set("Server.name", serverName = Console.ReadLine());
            }
        }

        Logger.Raw("                                       -=*####***++++++=-                  ", false);
        Logger.Raw("                                     +##%###****++====--                   ", false);
        Logger.Raw("                                   .+#%%%%%##*****+==-:                    ", false);
        Logger.Raw("                                 =#%%%%%%%%%##****+=-                      ", false);
        Logger.Raw("                                +%%%@@@%%%%%%##***=.                       ", false);
        Logger.Raw("                             .-%@@%%@@@@@%%%%%#*+:                         ", false);
        Logger.Raw("                            *%@@@@@@@@@@@@%%%#+:                           ", false);
        Logger.Raw("                           -%@@@@@@@@@@@@@%%%- ..                          ", false);
        Logger.Raw("                           .#@@@@@@@@@@@@%%%@*-=--:                      - ", false);
        Logger.Raw("                            .#%@@@@@@@@@%*:-*+===---:.                 =*- ", false);
        Logger.Raw("                             .*%@@@@@@@#:  +*++++=-+:::.             =*=-: ", false);
        Logger.Raw("                               *%@@@@%-    =***+#+-=: .=-.         -*+===-.", false);
        Logger.Raw("                                *%@%=       :***%*+*+:-===-:     -***++++=.", false);
        Logger.Raw("                                 :-         =****##%%*+=====-  :*###*####=.", false);
        Logger.Raw("                                            =******+++++++==  +##########=.", false);
        Logger.Raw("                                      .++:..  -+++==++**++%@#######%%%%#=: ", false);
        Logger.Raw("                                     -@@@%#:         .:. .*#######%%%%%+:. ", false);
        Logger.Raw("                                   -=#@@%%%%#:          =#%%%%%%%%%%%%*-.  ", false);
        Logger.Raw("                                 -###*#@@@%%%*        -#%%%%%%%%%##%#+:.   ", false);
        Logger.Raw("                               :*###*+++*%#--       -##%%%%%######%#=:.    ", false);
        Logger.Raw("             ..:::---=---:::::*###*+++****:       .#%%%%%%%%%%%%%#+-:      ", false);
        Logger.Raw("           =%@@@@@@@@@@@@@%%%%%###+++***-         .*#%%%%%%%%%@#+-:.       ", false);
        Logger.Raw("         .#@@#=------=*#%@@@@%=:=*#***=             :+#%%%%%%#+-:.         ", false);
        Logger.Raw("        -@@%=           .+%@@@+=+#%#+.                .=**+=-:.            ", false);
        Logger.Raw("       +@%*.          .:###*#%@@@%*.                                       ", false);
        Logger.Raw("     .#@%:          .:+##*+++***+.                                         ", false);
        Logger.Raw("    :%%%:          =**+++++***+.                                           ", false);
        Logger.Raw("   -@%%-         .*+=--=++**+.                                             ", false);
        Logger.Raw("  :@%%#.       .##**++++***:                                               ", false);
        Logger.Raw("  -@%%#.     .%%##*++++**=                                                 ", false);
        Logger.Raw("  +@%%*:   .#%%#**+++**+.                                                  ", false);
        Logger.Raw("  *@%%#+. .###*++++**+:                                                    ", false);
        Logger.Raw("  =@@%%**##*+++++***:                                                      ", false);
        Logger.Raw("   %@%%%#*+++++**+:                                                        ", false);
        Logger.Raw("   -@@%##%#++**+:                                                          ", false);
        Logger.Raw(" :+#@@%-:+#%**:                                                            ", false);
        Logger.Raw("-#*+*@@#*#%%-                                                              ", false);
        Logger.Raw(":#*=-=*#%#=                                                                ", false);
        Logger.Raw(" .+#*+=++                                                                \n", false);

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

        ConfigureCommands();

        if (interactive) {
            Application.Init();
            
            UI.MainWindow window = new();
            
            Application.Top.Add(window);
            Application.Top.Add(window.Menu(), window);
            
            Application.Run();
        } else {
            while (true) {
                Thread.Sleep(1000);
            }
        }
    }
    
    static void Main(string[] args) {
        try {
            Start(args);
        } catch (OperationCanceledException) {
            Logger.Log("Caught ctrl-c, shutting down...");
            Application.RequestStop();
        } catch (Exception exception) {
            Application.Shutdown();
            
            Logger.UnhookConsole();
            Logger.Error("Caught exception in main thread", exception);
            
            Environment.Exit(-1); 
        }
        
        Application.Shutdown();
        Environment.Exit(0);
    }
}
