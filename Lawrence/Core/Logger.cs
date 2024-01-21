using System;
using System.IO;
using System.Text;

namespace Lawrence.Core;

public class Logger {
    public enum Priority {
        Log = 0,
        Error = 1,
        Trace = 2
    }

    private static Logger _shared;
    private readonly object _syncLock = new();

    private static bool _hooked;

    private string _logFile = Settings.Default().Get("Logger.path", "lawrence.log");

    public static ConsoleOutputCapturer ConsoleOutputCapture;
    public static ConsoleOutputCapturer ConsoleErrorCapture;

    private Logger() {
    }

    /// <summary>
    /// Logs a message to the console and to the log file.
    /// </summary>
    /// <param name="priority"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    private void Log(Priority priority, string value, Exception exception = null) {
        string timestamp = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        string logEntry = $"{timestamp} [{priority}] {value}";

        lock (_syncLock) {
            // Write to file
            if (priority != Priority.Trace) {
                using (var streamWriter = new StreamWriter(_logFile, true)) {
                    streamWriter.WriteLine(logEntry);

                    if (exception != null) {
                        streamWriter.WriteLine($"Exception: {exception.GetType().FullName}");
                        streamWriter.WriteLine($"Message: {exception.Message}");
                        streamWriter.WriteLine($"StackTrace: {exception.StackTrace}");
                    }
                }
            }

            TextWriter consoleOut = _hooked ? ConsoleOutputCapture : Console.Out;
            TextWriter consoleError = _hooked ? ConsoleErrorCapture : Console.Error;
            
            // Write to console
            // TextWriter consoleWriter = priority == Priority.Error ? Console.Error : Console.Out;
            TextWriter consoleWriter = priority == Priority.Error ? consoleError : consoleOut;
            consoleWriter.WriteLine($"{logEntry}");

            if (exception != null) {
                consoleWriter.WriteLine($"Exception: {exception.GetType().FullName}");
                consoleWriter.WriteLine($"Message: {exception.Message}");
                consoleWriter.WriteLine($"StackTrace: {exception.StackTrace}");
            }
        }
    }

    public static Logger Shared() {
        return _shared ??= new Logger();
    }

    /// <summary>
    /// Sets the log file to write to.
    /// </summary>
    /// <param name="filename"></param>
    public static void SetLogFile(string filename) {
        Shared()._logFile = filename;
    }

    /// <summary>
    /// Logs a message to the console and to the log file.
    /// </summary>
    /// <param name="value"></param>
    public static void Log(string value) {
        Shared().Log(Priority.Log, value);
    }

    /// <summary>
    /// Logs a message as an error to the console and to the log file.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public static void Error(string value, Exception exception = null) {
        Shared().Log(Priority.Error, value, exception);
    }

    /// <summary>
    /// Logs a message as a trace to the console, but is not logged to file.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public static void Trace(string value, Exception exception = null) {
        Shared().Log(Priority.Trace, value, exception);
    }

    /// <summary>
    /// Logs a message to the console and to the log file without any formatting.
    /// </summary>
    /// <param name="value"></param>
    public static void Raw(string value, bool logToFile = true) {
        if (logToFile) {
            using (var streamWriter = new StreamWriter(Shared()._logFile, true)) {
                streamWriter.WriteLine(value);
            }
        }

        if (_hooked) {
            ConsoleOutputCapture?.WriteLine(value);
        } else {
            Console.WriteLine(value);
        }
    }
    
    /// <summary>
    /// Hooks the regular console such that we pick up all output from Console.WriteLine and Console.Error.WriteLine
    /// </summary>
    public static void HookConsole() {
        using (ConsoleOutputCapture = new ConsoleOutputCapturer()) {
            //Console.SetOut(ConsoleOutputCapture);
        }
        
        using (ConsoleErrorCapture = new ConsoleOutputCapturer()) {
            //Console.SetError(ConsoleErrorCapture);
        }

        _hooked = true;
    }
    
    public static void UnhookConsole() {
        ConsoleOutputCapture?.Dispose();
        ConsoleErrorCapture?.Dispose();

        _hooked = false;
    }
}

public class ConsoleOutputCapturer : TextWriter {
    private StringBuilder _stringBuilder = new StringBuilder();

    // Callbacks to be notified when a line is written
    public delegate void LineWritten(string line);

    public delegate void CharWritten(char character);
    
    public event LineWritten OnLineWritten;
    public event CharWritten OnCharWritten;
    
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value) {
        // Capture the character output
        _stringBuilder.Append(value);
        
        // Notify any listeners
        OnCharWritten?.Invoke(value);
    }

    public override void WriteLine(string value) {
        // Capture the line output
        _stringBuilder.AppendLine(value);
        
        // Notify any listeners
        OnLineWritten?.Invoke(value);
    }

    public string GetCapturedOutput() {
        return _stringBuilder.ToString();
    }

    public void Clear() {
        _stringBuilder.Clear();
    }
}
