using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using NLua;

namespace Lawrence
{
    public class Game {
        static Game SharedGame;

        private NotificationCenter _notificationCenter = new NotificationCenter();

        Lua state;

        string[] modsFolders;
        string[] runtimeScripts;

        private List<Mod> mods = new List<Mod>();

        private long _ticks = 0;

        public Game() {
            modsFolders = Directory.GetDirectories("mods/");
            runtimeScripts = Directory.GetFiles("runtime/");
        }

        /// <summary>
        /// Inits the game by loading the game modes in the configured mods folder. 
        /// </summary>
        void Initialize() {
            // Start off by loading some runtime Lua that initializes all the important Lua objects and similar.
            foreach (var scriptFilename in runtimeScripts) {
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
            foreach (var folder in modsFolders) {
                string canonicalName = folder.Split("/").Last().Split("\\").Last();

                // Users can disable or enable mods from the main settings.toml file. 
                if (!Settings.Default().Get<bool>($"Mod.{canonicalName}.enabled", true)) {
                    continue;
                }

                foreach (var file in Directory.GetFiles(folder)) {
                    if (file.EndsWith(".toml", StringComparison.OrdinalIgnoreCase)) {
                        try {
                            Logger.Log($"Loading configuration for mod at {file}");
                            Mod mod = new Mod(file);

                            // Run the entry Lua file
                            string entry = mod.Settings().Get<string>("General.entry");
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

                            State().DoString(entryFile, entry);

                            mods.Add(mod);
                        }
                        catch (Exception exception) {
                            Logger.Error($"Failed to load mod at {folder}", exception);
                        }
                    }
                }
            }
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
            if (state == null) {
                state = new Lua();

                state.LoadCLRPackage();
                state["Game"] = this;

                state["print"] = (string text) => {
                    Logger.Log(text);
                };
            }

            return state;
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
                    output += $"\n{obj.ToString()}";
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
			if (Game.SharedGame == null)
			{
				Game.SharedGame = new Game();
                Game.SharedGame.Initialize();
			}

			return Game.SharedGame;
		}

        public void Tick() {
            _ticks += 1;
            
            NotificationCenter().Post<TickNotification>(new TickNotification());
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
        public Entity NewEntity(LuaTable luaEntity = null) {
            Entity entity = new Entity(luaEntity);

            return entity;
        }

        public Label NewLabel(LuaTable luaEntity, string text = "", ushort x = 0, ushort y = 0, uint color = 0xC0FFA888) {
            Label label = new Label(luaEntity, text, x, y, color);

            return label;
        }

        /// <summary>
        /// Returns a new Universe CLR object.
        /// </summary>
        /// <param name="universeTable">Lua entity object</param>
        /// <returns></returns>
        public Universe NewUniverse(LuaTable universeTable) {
            return new Universe(universeTable);
        }

        public int PlayerCount()
        {
            int players = 0;

            foreach (Client client in Lawrence.GetClients())
            {
                if (client.IsActive())
                {
                    players += 1;
                }
            }

            return players;
        }
    }
}

