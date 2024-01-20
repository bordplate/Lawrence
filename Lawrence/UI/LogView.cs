using System;
using Lawrence.Core;
using Terminal.Gui;

namespace Lawrence.UI; 

public class LogView: FrameView {
    private readonly TextView _textView = new() {
        X = 0,
        Y = 0,
        Height = Dim.Fill(),
        Width = Dim.Fill(),
        ReadOnly = true,
        ColorScheme = Colors.Menu
    };
    
    public LogView() {
        Title = "Server log";
        
        _textView.ReadOnly = true;
        
        Add(_textView); 
        
        Setup();
    }

    private void AddText(string text) {
        _textView.Text += text;
        
        // Scroll to bottom
        if (text.Contains("\n")) {
            _textView.CursorPosition = new Point(0, _textView.Text.Split("\n").Length - 1);
            _textView.PositionCursor();
            
            _textView.SetNeedsDisplay();
            
            SetNeedsDisplay();
            
            Application.Refresh();
        }
    }

    private void Setup() {
        _textView.Text = Logger.ConsoleOutputCapture.GetCapturedOutput();
        _textView.Text += Logger.ConsoleErrorCapture.GetCapturedOutput();
        
        Logger.ConsoleOutputCapture.OnLineWritten += (line) => {
            AddText(line + "\n");
        };
        
        Logger.ConsoleOutputCapture.OnCharWritten += (character) => {
            AddText(character.ToString());
        };
        
        Logger.ConsoleErrorCapture.OnLineWritten += (line) => {
            AddText(line + "\n");
        };
        
        Logger.ConsoleErrorCapture.OnCharWritten += (character) => {
            AddText(character.ToString());
        };
    }
}