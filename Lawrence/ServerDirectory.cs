using System;
using System.Collections.Generic;

namespace Lawrence
{
	public struct Server
	{
		public string IP;
		public int Port;
		public string Name;
		public int MaxPlayers;
		public int PlayerCount;
	}

	public class ServerDirectory
	{
		List<Server> servers = new List<Server>();

		public ServerDirectory()
		{
			
		}

		public List<Server> Servers()
		{
			return servers;
		}

		public void RegisterServer(string ip, int port, string name, int maxPlayers, int playerCount)
		{
			Server server = new Server { IP = ip, Port = port, Name = name, MaxPlayers = maxPlayers, PlayerCount = playerCount };

			servers.Add(server);
		}
	}
}

