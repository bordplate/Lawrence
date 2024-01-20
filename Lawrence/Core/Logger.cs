using System;
using System.IO;

namespace Lawrence.Core;

public class Logger {
    public enum Priority {
        Log = 0,
        Error = 1,
        Trace = 2
    }

    private static Logger _shared;
    private readonly object _syncLock = new object();

    private string _logFile = Settings.Default().Get<string>("Logger.path", "lawrence.log");

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

            // Write to console
            TextWriter consoleWriter = priority == Priority.Error ? Console.Error : Console.Out;
            consoleWriter.WriteLine($"\r{logEntry}");

            if (exception != null) {
                consoleWriter.WriteLine($"Exception: {exception.GetType().FullName}");
                consoleWriter.WriteLine($"Message: {exception.Message}");
                consoleWriter.WriteLine($"StackTrace: {exception.StackTrace}");
            }

            consoleWriter.Write("> ");
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
    public static void Raw(string value) {
        using (var streamWriter = new StreamWriter(Shared()._logFile, true)) {
            streamWriter.WriteLine(value);
        }

        Console.WriteLine(value);
    }
}
