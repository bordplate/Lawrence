using System;
using System.Data;
using Terminal.Gui;

namespace Lawrence.UI;

public class MainWindow : Window {
	private PlayerList _playerList;
	private LogView _logView;
	
	public MainWindow()
	{
		Title = "Lawrence Multiplayer Server";

		_playerList = new() {
			X = 0,
			Y = 0,
			Height = Dim.Fill(),
			Width = Dim.Percent(30)
		};

		Add(_playerList);
		
		_logView = new() {
			X = Pos.Right(_playerList),
			Y = 0,
			Height = Dim.Fill(),
			Width = Dim.Fill()
		};
		
		Add(_logView);
	}
}
