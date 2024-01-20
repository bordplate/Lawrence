using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

using Lawrence.Core;
using Lawrence.Game;

namespace Lawrence.UI;

public class PlayerList: FrameView  {
    private readonly ListView _playerList = new() {
        X = 0,
        Y = 0,
        Height = Dim.Fill(),
        Width = Dim.Fill()
    };
    
    private List<string> _players = new();
    
    public PlayerList() {
        Title = "Players";
        
        Add(_playerList);

        _playerList.SetSource(_players);
        
        // Subscribe to player join notifications
        Game.Game.Shared().NotificationCenter().Subscribe<PlayerJoinedNotification>(OnPlayerJoin);
        Game.Game.Shared().NotificationCenter().Subscribe<PlayerDisconnectedNotification>(OnPlayerDisconnect);
    }

    public void OnPlayerJoin(PlayerJoinedNotification notification) {
        // Run on Terminal.Gui main loop main loop
        Application.MainLoop.Invoke(() => {
            if (notification.Entity is Player player) {
                if (!_players.Contains(player.Username())) {
                    _players.Add(player.Username());
                }
            }
        });
    }

    public void OnPlayerDisconnect(PlayerDisconnectedNotification notification) {
        Application.MainLoop.Invoke(() => {
            if (notification.Entity is Player player) {
                if (_players.Contains(player.Username())) {
                    _players.Remove(player.Username());
                }
            }
        });
    }
}