using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using NLua;

namespace Lawrence
{
	public class Game
	{
		static Game SharedGame;

        /// <summary>
        /// Entities in the game that are actively updated and kept track of by the game. 
        /// </summary>
        private List<Entity> entities = new List<Entity>();

        public List<Moby> mobys = new List<Moby>();
        List<Behavior> behaviors = new List<Behavior>();

        Lua state;

        string[] behaviorScripts;
        string[] runtimeScripts;

        public Game()
		{
            behaviorScripts = Directory.GetFiles("modes/");
            runtimeScripts = Directory.GetFiles("runtime/");
		}

        /// <summary>
        /// Inits the game by loading the game modes in the configured mods folder. 
        /// </summary>
        void Initialize()
        {
            foreach(var scriptFilename in runtimeScripts) {
                if (!scriptFilename.EndsWith(".lua")) continue;

                Logger.Trace($"Loading runtime script: {scriptFilename}");

                try {
                    string script = File.ReadAllText(scriptFilename);

                    State().DoString(script);
                } catch (FileNotFoundException exception) {
                    Logger.Error($"Couldn't find file {scriptFilename} in runtime folder even though it was iterated on startup", exception);
                } catch (FileLoadException exception) {
                    Logger.Error($"Failed to read runtime file {scriptFilename}", exception);
                } catch (NLua.Exceptions.LuaException exception) {
                    Logger.Error($"Failed to execute runtime script {scriptFilename}", exception);
                }
            }

            return;

            foreach (var script in behaviorScripts)
            {
                if (!script.EndsWith(".lua")) continue;

                Console.WriteLine($"Starting script at {script}");

                try
                {
                    behaviors.Add(new Behavior(script));
                } catch (ApplicationException exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
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

        public string Execute(string eval)
        {
            string output = "";

            try
            {
                object[] result = State().DoString(eval);

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

        public void Tick()
        {
            foreach (var behavior in behaviors)
            {
                behavior.Tick();
            }

            foreach(var entity in entities) {
                if (!entity.Active) {
                    continue;
                }

                entity.OnTick();
            }

            foreach (var moby in mobys)
            {
                if (!moby.active)
                {
                    continue;
                }

                if (moby.parent != null && moby.parent.IsActive())
                {
                    foreach (var behavior in behaviors)
                    {
                        behavior.PlayerTick(moby.parent);
                    }
                }

                moby.Tick();

                List<Client> ignoring = null;
                if (moby.parent != null)
                {
                    ignoring = new List<Client> { moby.parent };
                }

                if (moby.onlyVisibleToTeam)
                {
                    Lawrence.DistributePacket(Packet.MakeMobyUpdatePacket(moby), moby.level, ignoring, moby.team);
                }
                else
                {
                    if (moby.onlyVisibleToPlayer != -1)
                    {
                        Client client = Lawrence.GetClient(moby.onlyVisibleToPlayer);
                        if (client != null)
                        {
                            if (client.GetMoby() != null && moby.level == client.GetMoby().level)
                            {
                                client.SendPacket(Packet.MakeMobyUpdatePacket(moby));
                            }
                        }
                        else
                        {
                            DeleteMoby(moby.UUID);
                        }
                    }
                    else
                    {
                        Lawrence.DistributePacket(Packet.MakeMobyUpdatePacket(moby), moby.level, ignoring);
                    }
                }
            }
        }

        /// <summary>
        /// Make a new game Entity and register it in the game.
        /// </summary>
        /// <param name="luaEntity">Lua behavior object for the entity.</param>
        /// <returns></returns>
        public Entity NewEntity(LuaTable luaEntity = null) {
            Entity entity = new Entity(luaEntity);

            entities.Add(entity);

            return entity;
        }

        public List<Moby> Mobys()
		{
			return mobys;
		}


        public Moby GetMoby(uint uuid)
        {
            if (uuid > mobys.Count)
            {
                return null;
            }

            return mobys[(int)uuid - 1];
        }

        public Moby NewMoby(Client parent = null)
        {
            Moby moby = new Moby(parent);
            // Moby UUIDs start at 1, not 0.
            moby.UUID = (ushort)(mobys.Count + 1);

            foreach (Moby m in mobys)
            {
                if (m.Deleted())
                {
                    moby.UUID = m.UUID;
                }
            }

            if (moby.UUID-1 >= mobys.Count)
            {
                mobys.Add(moby);
            } else
            {
                mobys[moby.UUID-1] = moby;
            }

            if (parent != null)
            {
                Console.WriteLine($"New moby (uid: {moby.UUID}). Parent: {parent?.ID}");
            }
            else
            {
                Console.WriteLine($"New moby (uid: {moby.UUID})");
            }

            return moby;
        }

        public Moby SpawnMoby(int oClass)
        {
            Moby moby = NewMoby();
            moby.oClass = oClass;

            return moby;
        }

        public Moby SpawnMobyForPlayer(int oClass, ushort playerUUID)
        {
            Moby moby = SpawnMoby(oClass);
            moby.onlyVisibleToPlayer = playerUUID;

            return moby;
        }

        public void DeleteMobys(Func<Moby, bool> value)
        {
            Moby[] deleteMobys = mobys.Where(value).ToArray();

            foreach (var moby in deleteMobys)
            {
                Console.WriteLine($"Requesting clients delete moby {moby.UUID}");
                moby.Delete();
            }
        }

        public void DeleteMoby(ushort uuid)
        {
            foreach (Moby moby in mobys)
            {
                if (moby.UUID == uuid)
                {
                    moby.Delete();
                    return;
                }
            }
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

        public Client GetPlayer(ushort ID)
        {
            return Lawrence.GetClient((int)ID);
        }

        public List<Client> GetPlayers()
        {
            return Lawrence.GetClients().Where((Client client) =>
            {
                return client.IsActive();
            }).ToList();
        }

        public void DrawText(ushort id, string text, ushort x, ushort y, uint color = 0xC0FFA888)
        {
            Lawrence.DistributePacket(Packet.MakeSetHUDTextPacket(id, text, x, y, color));
        }

        public void DrawTextForPlayer(int playerID, ushort id, string text, ushort x, ushort y, uint color = 0xC0FFA888)
        {
            Lawrence.GetClient(playerID).SendPacket(Packet.MakeSetHUDTextPacket(id, text, x, y, color));
        }

        public void DeleteText(ushort id)
        {
            Lawrence.DistributePacket(Packet.MakeDeleteHUDTextPacket(id));
        }

        public void DeleteTextForPlayer(int playerID, ushort id)
        {
            Lawrence.GetClient(playerID).SendPacket(Packet.MakeDeleteHUDTextPacket(id));
        }

        public void GiveItemToPlayer(int playerID, ushort item)
        {
            Lawrence.GetClient(playerID).SendPacket(Packet.MakeSetItemPacket(item, true));
        }

        public void SendPlayerToPlanet(int playerID, int planet)
        {
            Lawrence.GetClient(playerID).SendPacket(Packet.MakeGoToPlanetPacket(planet));
        }

        public void OnPlayerConnect(Client client)
        {
            LuaFunction function = state.GetFunction("on_player_connect");
            if (function != null)
            {
                function.Call(new object[] { client });
            }
        }

        public void OnCollision(Moby collider, Moby collidee, uint flags)
        {
            LuaFunction function = state.GetFunction("on_collision");
            if (function != null) { 
                function.Call(new object[] { collider, collidee, flags });
            }
        }

        public void OnCollisionEnd(Moby collider, Moby collidee)
        {
            LuaFunction function = state.GetFunction("on_collision_end");
            if (function != null)
            {
                function.Call(new object[] { collider, collidee });
            }
        }

        public void PlayerPressedButtons(Client client, ControllerInput pressedButtons)
        {
            LuaFunction function = state.GetFunction("on_player_input");
            if (function != null)
            {
                function.Call(new object[] { client, ((int)pressedButtons) });
            }
        }

        public void OnPlayerGameStateChange(Client client, GameState gameState)
        {
            LuaFunction function = state.GetFunction("on_player_game_state_change");
            if (function != null)
            {
                function.Call(new object[] { client, (uint)gameState });
            }
        }
    }
}

