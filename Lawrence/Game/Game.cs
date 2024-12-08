using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NLua;

using Lawrence.Core;
using Lawrence.Game.UI;

namespace Lawrence.Game;


public enum ControllerInput {
    L2 = 1,
    R2 = 2,
    L1 = 4,
    R1 = 8,
    Triangle = 16,
    Circle = 32,
    Cross = 64,
    Square = 128,
    Select = 256,
    L3 = 512,
    R3 = 1024,
    Start = 2048,
    Up = 4096,
    Right = 8192,
    Down = 16384,
    Left = 32768
}

public enum GameState {
    PlayerControl = 0x0,
    Movie = 0x1,
    CutScene = 0x2,
    Menu = 0x3,
    Prompt = 0x4,
    Vendor = 0x5,
    Loading = 0x6,
    Cinematic = 0x7,
    UnkFF = 0xff,
}

public class Game {
    private static Game? _sharedGame;

    private NotificationCenter _notificationCenter = new();

    private Lua? _state;

    private readonly string[] _modsFolders;
    private readonly string[] _runtimeScripts;

    private readonly List<Mod> _mods = new();

    private readonly Server _server;

    private long _ticks = 0;
    
    private readonly List<Universe> _universes = new();
    
    /// <summary>
    /// Time in milliseconds
    /// </summary>
    private ulong _time;
    private ulong _deltaTime;

    public Game(Server server) {
        _server = server;
        
        _modsFolders = Directory.GetDirectories("mods/");
        _runtimeScripts = Directory.GetFiles("runtime/");
    }

    /// <summary>
    /// Inits the game by loading the game modes in the configured mods folder. 
    /// </summary>
    void Initialize() {
        // Start off by loading some runtime Lua that initializes all the important Lua objects and similar.
        foreach (var scriptFilename in _runtimeScripts) {
            if (!scriptFilename.EndsWith(".lua")) continue;

            Logger.Trace($"Loading runtime script: {scriptFilename}");

            try {
                string script = File.ReadAllText(scriptFilename);

                State().DoString(script);
            }
            catch (FileNotFoundException exception) {
                Logger.Error(
                    $"Couldn't find file {scriptFilename} in runtime folder even though it was iterated on startup",
                    exception);
            }
            catch (FileLoadException exception) {
                Logger.Error($"Failed to read runtime file {scriptFilename}", exception);
            }
            catch (NLua.Exceptions.LuaException exception) {
                Logger.Error($"Failed to execute runtime script {scriptFilename}", exception);
            }
            catch (Exception exception) {
                Logger.Error($"Unknown exception when executing runtime script {scriptFilename}", exception);
            }
        }

        // Load the user-installed mods
        foreach (var folder in _modsFolders) {
            string canonicalName = folder.Split("/").Last().Split("\\").Last();

            // Users can disable or enable mods from the main settings.toml file. 
            if (!Settings.Default().Get($"Mod.{canonicalName}.enabled", false)) {
                continue;
            }

            foreach (var file in Directory.GetFiles(folder)) {
                if (file.EndsWith(".toml", StringComparison.OrdinalIgnoreCase)) {
                    try {
                        Logger.Log($"Loading configuration for mod at {file}");
                        Mod mod = new Mod(file, canonicalName);

                        // Run the entry Lua file
                        var entry = mod.Settings().Get<string>("General.entry");
                        if (entry == null) {
                            Logger.Error(
                                $"Mod at {file} does not contain `General.entry` Lua file to specify which Lua file should be executed first to start the mod. ");
                            continue;
                        }

                        string entryFile = File.ReadAllText($"{mod.Path()}/{entry}");
                        
                        if (entryFile == null) {
                            Logger.Error($"Could not load entry file at {entryFile}.");
                            continue;
                        }
                        
                        // Add the mod path to Lua package path
                        _state?.DoString($"package.path = package.path .. \";{folder.Replace("\\", "\\\\")}/?.lua\"", "set package path chunk");

                        State().DoString(entryFile, entry);

                        _mods.Add(mod);
                    }
                    catch (Exception exception) {
                        Logger.Error($"Failed to load mod at {folder}", exception);
                    }
                }
            }
        }
        
        ConfigureCommands();
    }

    /// <summary>
    /// Configures CLI commands for the game.
    /// </summary>
    private void ConfigureCommands() {
        // Add commands
        var setPositionCommand = new Command {
            Name = "set_position",
            Args = new [] {
                new Command.Arg { Name = "player_name" },
                new Command.Arg { Name = "x", Type = "float" },
                new Command.Arg { Name = "y", Type = "float" },
                new Command.Arg { Name = "z", Type = "float" },
            },
            Description = ""
        };

        setPositionCommand.OnCommand += args => {
            var playerName = args[0];
            var x = float.Parse(args[1]);
            var y = float.Parse(args[2]);
            var z = float.Parse(args[3]);

            var player = FindPlayerByUsername(playerName);
            
            if (player == null) {
                Logger.Raw($"Could not find player with username {playerName}", false);
                return;
            }

            player.x = x;
            player.y = y;
            player.z = z;
            
            Logger.Raw($"Set position of {playerName} to {x}, {y}, {z}", false);
        };
        
        var infoCommand = new Command {
            Name = "info",
            Description = "Prints information about the specified player.",
            Args = new [] {
                new Command.Arg { Name = "player_name" }
            }
        };
        
        infoCommand.OnCommand += args => {
            var playerName = args[0];

            var player = FindPlayerByUsername(playerName);
            
            if (player == null) {
                Logger.Raw($"Could not find player with username {playerName}", false);
                return;
            }

            var info = 
                $"""
                {playerName}: 
                    level: {player.Level()?.GetName()}
                 
                    x: {player.x}
                    y: {player.y}
                    z: {player.z}
                """;

            Logger.Raw(info, false);
        };
        
        var setLevelCommand = new Command {
            Name = "set_level",
            Description = "Sets the level of the specified player.",
            Args = new [] {
                new Command.Arg { Name = "player_name" },
                new Command.Arg { Name = "level_name" }
            }
        };

        setLevelCommand.OnCommand += (args) => {
            var playerName = args[0];
            
            var player = FindPlayerByUsername(playerName);
            
            if (player == null) {
                Logger.Raw($"Could not find player with username {playerName}", false);
                return;
            }
            
            var levelName = args[1];
            var level = player.Universe()?.GetLevelByName(levelName);
            
            if (level == null) {
                Logger.Raw($"Could not find level with name {levelName}", false);
                return;
            }
            
            player.LoadLevel(levelName);
            
            Logger.Raw($"Set level of {playerName} to {levelName}", false);
        };
        
        var showMessageCommand = new Command {
            Name = "show_message",
            Description = "Shows a message to the specified player.",
            Args = new [] {
                new Command.Arg { Name = "player_name" },
                new Command.Arg { Name = "message" }
            }
        };
        
        showMessageCommand.OnCommand += (args) => {
            var playerName = args[0];
            
            var player = FindPlayerByUsername(playerName);
            
            if (player == null) {
                Logger.Raw($"Could not find player with username {playerName}", false);
                return;
            }
            
            var message = args[1];
            
            player.ShowErrorMessage(message);
            
            Logger.Raw($"Showed message to {playerName}", false);
        };

        Lawrence.RegisterCommand("Game", infoCommand);
        Lawrence.RegisterCommand("Game", setPositionCommand);
        Lawrence.RegisterCommand("Game", setLevelCommand);
        Lawrence.RegisterCommand("Game", showMessageCommand);
    }

    /// <summary>
    /// Returns a list of all mods, enabled and disabled.
    /// </summary>
    public static List<Mod> AllMods() {
        // Load mods from folder
        List<Mod> mods = new();
        
        string[] modsFolders = Directory.GetDirectories(
            Settings.Default().Get("Server.mods_path", "mods/", true)!
        );
        
        foreach (var folder in modsFolders) {
            string canonicalName = folder.Split("/").Last().Split("\\").Last();

            foreach (var file in Directory.GetFiles(folder)) {
                if (file.EndsWith(".toml", StringComparison.OrdinalIgnoreCase)) {
                    try {
                        Logger.Log($"Loading configuration for mod at {file}");
                        Mod mod = new Mod(file, canonicalName);

                        mods.Add(mod);
                    }
                    catch (Exception exception) {
                        Logger.Error($"Failed to load mod at {folder}", exception);
                    }
                }
            }
        }

        return mods;
    }

    /// <summary>
    /// Central notification center for all things happening within the game. 
    /// </summary>
    /// <returns></returns>
    public NotificationCenter NotificationCenter() {
        return _notificationCenter;
    }

    /// <summary>
    /// How many server ticks have happened thus far.
    /// </summary>
    /// <returns></returns>
    public long Ticks() {
        return _ticks;
    }

    public Lua State()
    {
        if (_state == null) {
            _state = new Lua();
            _state.UseTraceback = true;

            _state.LoadCLRPackage();
            _state["Game"] = this;
            
            _state["NativeView"] = (LuaTable playerTable) => new View(playerTable);
            _state["NativeListMenuElement"] = () => new ListMenuElement();
            _state["NativeTextAreaElement"] = () => new TextAreaElement();
            _state["NativeTextElement"] = () => new TextElement();
            _state["NativeInputElement"] = () => new InputElement();
            
            _state["GetTypeName"] = (object? obj) => {
                return obj?.GetType().Name;
            };

            _state["print"] = (string text) => {
                Logger.Log(text);
            };
        }

        return _state;
    }

    /// <summary>
    /// Execute code in the current Lua context
    /// </summary>
    /// <param name="eval">Lua code to execute</param>
    /// <returns>Execution result</returns>
    public string Execute(string eval)
    {
        string output = "";

        try
        {
            object[] result = State().DoString(eval, "console");

            foreach (object obj in result)
            {
                output += $"\n{obj}";
            }
        } catch (Exception e)
        {
            output = e.Message;
        }

        return output;
    }

	// Get shared environment singleton
	public static Game Shared()
	{
		if (_sharedGame == null)
		{
            if (Lawrence.Server() is not { } server) {
                throw new Exception("Server is not initialized.");
            }
            
			_sharedGame = new Game(server);
            _sharedGame.Initialize();
		}

		return _sharedGame;
	}

    /// <summary>
    /// Gets the time since Jan 1 1970 in milliseconds.
    /// </summary>
    /// <returns>Current time in milliseconds.</returns>
    public ulong Time() {
        return _time;
    }

    public ulong DeltaTime() {
        return _deltaTime;
    }

    public void Tick() {
        DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        TimeSpan timeSinceEpoch = DateTimeOffset.UtcNow - unixEpoch;
        ulong time = (ulong)timeSinceEpoch.TotalMilliseconds;

        _deltaTime = time - _time;
        _time = time;
        
        _ticks += 1;
        
        GC.AddMemoryPressure(1024 * 1024 * 1024);

        NotificationCenter().Post(new PreTickNotification());
        NotificationCenter().Post(new TickNotification());
        NotificationCenter().Post(new PostTickNotification());
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Called by a Client when handshake is completed and a player has connected to the game server. 
    /// </summary>
    /// <param name="client"></param>
    public void OnPlayerConnect(Client client) {
        Player player = new Player(client);

        _notificationCenter.Post(new PlayerJoinedNotification(0, null, player));
    }

    /// <summary>
    /// Make a new game Entity, but does not automatically add it to the game.
    /// </summary>
    /// <param name="luaEntity">Lua behavior object for the entity.</param>
    /// <returns></returns>
    public Entity NewEntity(LuaTable? luaEntity = null) {
        Entity entity = new Entity(luaEntity);

        return entity;
    }

    public Label NewLabel(LuaTable luaEntity, string text = "", ushort x = 0, ushort y = 0, uint color = 0xC0FFA888) {
        Label label = new Label(luaEntity, text, x, y, color);

        return label;
    }
    
    public Player? FindPlayerByUsername(string username) {
        foreach (var universe in _universes) {
            foreach (var player in universe.Find<Player>()) {
                if (player.Username() == username) {
                    return player;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a new Universe CLR object.
    /// </summary>
    /// <param name="universeTable">Lua entity object</param>
    /// <returns></returns>
    public Universe NewUniverse(LuaTable universeTable) {
        var universe = new Universe(universeTable);
        
        _universes.Add(universe);
        
        return universe;
    }

    public int PlayerCount()
    {
        int players = 0;

        foreach (Client client in  _server.Clients())
        {
            if (client.IsActive())
            {
                players += 1;
            }
        }

        return players;
    }
}
