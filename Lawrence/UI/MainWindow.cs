using System;
using System.Data;
using Lawrence.Core;
using Terminal.Gui;

namespace Lawrence.UI;

public class MainWindow : Window {
	private PlayerList _playerList;
	private LogView _logView;
	private FrameView _serverInfoView;
	
	public MainWindow()
	{
		Title = Settings.Default().Get("Server.name", "Lawrence");

		_playerList = new() {
			X = 0,
			Y = 0,
			Height = Dim.Percent(60),
			Width = Dim.Percent(30)
		};

		Add(_playerList);

		_serverInfoView = new() {
			Title = "Server info",
			X = 0,
			Y = Pos.Bottom(_playerList),
			Height = Dim.Percent(40),
			Width = Dim.Percent(30)
		};
		
		Add(_serverInfoView);

		Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(2), UpdateServerInfo);
		
		_logView = new() {
			X = Pos.Right(_playerList),
			Y = 0,
			Height = Dim.Fill(),
			Width = Dim.Fill()
		};
		
		Add(_logView);
	}

	public bool UpdateServerInfo(MainLoop mainLoop) {
		if (Lawrence.DirectoryMode()) {
			return true;
		}
		
		_serverInfoView.Text = 
$"""
Players: {Game.Game.Shared().PlayerCount()}
Ticks per second: {Lawrence.Server().TicksPerSecond()}

""";
		return true;
	}
}
