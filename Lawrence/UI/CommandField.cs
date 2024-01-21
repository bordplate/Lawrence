using System.Collections.Generic;
using System.Linq;
using Lawrence.Core;
using Terminal.Gui;

namespace Lawrence.UI; 

public class CommandField: TextField {
    private List<string> _history = new();
    private int _historyIndex = 0;
    
    public CommandField() {
        
    }

    public override bool OnKeyDown(KeyEvent keyEvent) {
        if (keyEvent.Key == Key.CursorUp || keyEvent.Key == Key.CursorDown) {
            return false;
        }
        
        return base.OnKeyDown(keyEvent);
    }

    public override bool OnKeyUp(KeyEvent keyEvent) {
        if (keyEvent.Key == Key.Enter) {
            Logger.Raw($"> {Text}", false);

            var prompt = Text.ToString().Trim();

            _history.Insert(0, prompt);
            _historyIndex = 0;

            Text = "";
            Clear();

            var commandName = prompt.Trim().Split(" ").First();

            var command = Lawrence.Command(commandName);

            if (command == null) {
                Logger.Raw($"Unknown command `{commandName}`.", false);

                return base.OnKeyUp(keyEvent);
            }

            // Split args and make everything between 's and "s a single arg
            var args = new List<string> { };

            var currentArg = "";
            var inQuotes = false;
            var inSingleQuotes = false;
            foreach (var character in prompt) {
                if (character == ' ' && !inQuotes && !inSingleQuotes) {
                    args.Add(currentArg);
                    currentArg = "";
                    continue;
                }

                if (character == '"' && !inSingleQuotes) {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (character == '\'' && !inQuotes) {
                    inSingleQuotes = !inSingleQuotes;
                    continue;
                }

                currentArg += character;
            }
            
            if (currentArg.Length > 0) {
                args.Add(currentArg);
            }

            command.Run(args.Skip(1).ToArray());
        }

        return base.OnKeyUp(keyEvent);
    }

    public override bool ProcessKey(KeyEvent keyEvent) {
        if (keyEvent.Key == Key.CursorUp) {
            if (_history.Count == 0) {
                return true;
            }
            
            if (_historyIndex >= _history.Count) {
                return true;
            }
            
            _historyIndex++;
            
            Text = _history[_historyIndex-1];
            
            SetNeedsDisplay();
            CursorPosition = Text.Length;

            return true;
        }
        if (keyEvent.Key == Key.CursorDown) {
            if (_history.Count == 0) {
                return true;
            }
            
            if (_historyIndex <= 0) {
                return true;
            }
            
            _historyIndex--;
            
            Text = _historyIndex > 0 ? _history[_historyIndex-1] : "";
            
            SetNeedsDisplay();
            CursorPosition = Text.Length;

            return true;
        }
        
        return base.ProcessKey(keyEvent);
    }
}