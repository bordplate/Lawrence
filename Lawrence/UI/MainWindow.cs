﻿using System;
using System.Data;
using Lawrence.Core;
using Terminal.Gui;

namespace Lawrence.UI;

public class MainWindow : Window {
	private PlayerList _playerList;
	private LogView _logView;
	private FrameView _serverInfoView;
	private CommandField _commandField;

	private MenuBar _menu;
	
	public MainWindow()
	{
		Title = Settings.Default().Get("Server.name", "Lawrence");

		_playerList = new() {
			X = 0,
			Y = 0,
			Height = Dim.Percent(55),
			Width = Dim.Percent(30)
		};

		Add(_playerList);

		_serverInfoView = new() {
			Title = "Server info",
			X = 0,
			Y = Pos.Bottom(_playerList),
			Height = Dim.Fill(),
			Width = Dim.Percent(30)
		};
		
		Add(_serverInfoView);

		Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(2), UpdateServerInfo);
		
		_logView = new() {
			X = Pos.Right(_playerList),
			Y = 0,
			Height = Dim.Fill(1),
			Width = Dim.Fill()
		};
		
		Add(_logView);
		
		_commandField = new() {
			X = Pos.Right(_playerList),
			Y = Pos.Bottom(_logView),
			Height = Dim.Fill(),
			Width = Dim.Fill()
		};

		Add(_commandField);
		
		_menu = new MenuBar (new MenuBarItem [] {
			new MenuBarItem ("_File", new MenuItem [] {
				new MenuItem ("_Quit", "", () => { 
					Application.RequestStop();
				})
			}),
		});
	}
	
	public MenuBar Menu() {
		return _menu;
	}

	public bool UpdateServerInfo(MainLoop mainLoop) {
		if (Lawrence.DirectoryMode()) {
			return true;
		}

		if (Lawrence.Server() is {} server) {
			_serverInfoView.Text =
				$"""
				 Players: {Game.Game.Shared().PlayerCount()}
				 Ticks per second: {server.TicksPerSecond()}

				 """;
		}

		return true;
	}
}
