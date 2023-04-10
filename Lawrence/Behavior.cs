using System;
using System.IO;
using NLua;

namespace Lawrence
{
	public class Behavior
	{
		Lua state;
		string behaviorFile;

		LuaFunction tickFunction;
		LuaFunction onLoadFunction;
		LuaFunction playerTickFunction;

		public Behavior(string behaviorFile)
		{
			this.behaviorFile = behaviorFile;
			state = Game.Shared().State();

			state.DoString(File.ReadAllText(behaviorFile));

			onLoadFunction = state.GetFunction("on_load");

			if (onLoadFunction != null)
			{
				onLoadFunction.Call();
			}

            tickFunction = state.GetFunction("tick");
            if (tickFunction == null)
            {
				throw new ApplicationException($"ERROR: Behavior '{behaviorFile}' does not have required `tick()` function.");
            }

			playerTickFunction = state.GetFunction("player_tick");
        }

		public void Tick()
		{
			tickFunction.Call();
		}

		public void PlayerTick(Client client)
		{
			if (playerTickFunction != null)
			{
				playerTickFunction.Call(new object[] { client });
			}
		}
	}
}
