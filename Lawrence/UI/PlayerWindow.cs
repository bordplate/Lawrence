using Terminal.Gui;

namespace Lawrence.UI; 

public class PlayerWindow: Window {
    public PlayerWindow(string playerName) {
        Title = playerName;

        var player = Game.Game.Shared().FindPlayerByUsername(playerName);

        var playerInfo = new Label {
            X = 2,
            Y = 2,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        playerInfo.Text = $"""
                           Level: {player?.Level()?.GetName()}

                           X: {player?.x}
                           Y: {player?.y}
                           Z: {player?.z}
                           """;
       
        // Close button at bottom center
        var closeButton = new Button("Close") {
            X = Pos.Center(),
            Y = Pos.Percent(100) - 1,
        };
        closeButton.Clicked += () => {
            Remove(this);
        };
        
        Add(playerInfo);
        Add(closeButton);
    }
}