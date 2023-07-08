﻿using System;
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
		public DateTime LastPing;
	}

	public class ServerDirectory
	{
		List<Server> servers = new List<Server>();
		DateTime lastClearStale = DateTime.MinValue;

		public ServerDirectory()
		{
			
		}

		public List<Server> Servers()
		{
			ClearStale();

			return servers;
		}

		public void RegisterServer(string ip, int port, string name, int maxPlayers, int playerCount)
		{
			ClearStale();
			int index = servers.FindIndex(s => s.IP == ip && s.Port == port);

			if(index != -1)
			{
				servers[index] = new Server { IP = ip, Port = port, Name = name, MaxPlayers = maxPlayers, PlayerCount = playerCount, LastPing = DateTime.Now };
			}
			else
			{
				Server server = new Server { IP = ip, Port = port, Name = name, MaxPlayers = maxPlayers, PlayerCount = playerCount, LastPing = DateTime.Now };
				servers.Add(server);

				Logger.Log($"New server '{name}' @ {ip}:{port}");
			}
		}

		public void ClearStale()
		{
			if((DateTime.Now - lastClearStale).TotalSeconds < 30)
			{
				return;
			}

			for (int i = servers.Count - 1; i >= 0; i--)
			{
				if ((DateTime.Now - servers[i].LastPing).TotalMinutes > 2)
				{
					Logger.Log($"Removing stale server '{servers[i].Name}' @ {servers[i].IP}:{servers[i].Port}");
					servers.RemoveAt(i);
				}
			}

			lastClearStale = DateTime.Now;
		}
	}
}

