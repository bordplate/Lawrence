using System;
using System.Collections.Generic;
using System.IO;

namespace Lawrence.Core;

public struct ServerItem
{
	public string IP;
	public int Port;
	public string Name;
	public int MaxPlayers;
	public int PlayerCount;
	public DateTime LastPing;
	public string Description;
	public string Owner;
}

public class ServerDirectory
{
	private readonly List<ServerItem> _servers = new();
	private DateTime _lastClearStale = DateTime.MinValue;

	public List<ServerItem> Servers() { 
		ClearStale();
		
		return _servers;
	}

	public void RegisterServer(string ip, int port, string name, int maxPlayers, int playerCount, string description, string owner)
	{
		ClearStale();
		int index = _servers.FindIndex(s => s.IP == ip && s.Port == port);

		if(index != -1) {
			_servers[index] = new ServerItem {
				IP = ip, 
				Port = port, 
				Name = name, 
				MaxPlayers = maxPlayers, 
				PlayerCount = playerCount,
				LastPing = DateTime.Now,
				Description =  description,
				Owner = owner
			};
		} else {
			ServerItem serverItem = new ServerItem {
				IP = ip, 
				Port = port, 
				Name = name, 
				MaxPlayers = maxPlayers, 
				PlayerCount = playerCount, 
				LastPing = DateTime.Now,
				Description =  description,
				Owner = owner
			};
			_servers.Add(serverItem);

			Logger.Log($"New server '{name}' @ {ip}:{port}");
		}
		
		WriteServersToFile();
	}

	public void ClearStale()
	{
		if((DateTime.Now - _lastClearStale).TotalSeconds < 30)
		{
			return;
		}

		for (int i = _servers.Count - 1; i >= 0; i--)
		{
			if ((DateTime.Now - _servers[i].LastPing).TotalMinutes > 3)
			{
				Logger.Log($"Removing stale server '{_servers[i].Name}' @ {_servers[i].IP}:{_servers[i].Port}");
				_servers.RemoveAt(i);
			}
		}

		_lastClearStale = DateTime.Now;
	}
	
	private void WriteServersToFile()
	{
		using (StreamWriter file = new StreamWriter("servers.log", false))
		{
			foreach (var server in _servers)
			{
				file.WriteLine($"({server.PlayerCount}/{server.MaxPlayers}) {server.Name}");
			}
		}
	}
}

