using System.Linq;

namespace Lawrence.Core; 

public class Command {
    public delegate void CommandHandler(string[] args);
    public event CommandHandler? OnCommand;

    public struct Arg {
        public string? Name;
        public bool Optional = false;
        public string Type = "string";

        public Arg() {
            Name = null;
        }

        public override string ToString() {
            return Optional ? $"[{Name}]" : $"<{Name}>";
        }
    }
    public string? Name { get; set; }
    public Arg[] Args { get; set; } = { };
    
    public string? Description { get; set; }

    public void Run(string[] args) {
        var numRequiredArgs = Args.Count(arg => !arg.Optional);
        
        if (args.Length < numRequiredArgs) {
            Logger.Raw($"Missing argument. Usage: {Name} {string.Join(" ", Args)}", false);
            return;
        }
        
        // Check the type
        for (var i = 0; i < args.Length; i++) {
            if (i >= Args.Length) {
                break;
            }
            
            var arg = Args[i];
            var value = args[i];

            switch (arg.Type) {
                case "int": {
                    if (!int.TryParse(value, out _)) {
                        Logger.Raw($"Argument `{arg.Name}` must be an integer.", false);
                        return;
                    }
                    break;
                }
                case "string": {
                    break;
                }
                case "float": {
                    if (!float.TryParse(value, out _)) {
                        Logger.Raw($"Argument `{arg.Name}` must be a floating point number.", false);
                        return;
                    }
                    break;
                }
            }
        }
        
        OnCommand?.Invoke(args);
    }
}