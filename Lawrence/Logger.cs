using System;
using System.Data.Common;
using System.IO;

namespace Lawrence {
    public class Logger {
        public enum Priority {
            Log = 0,
            Error = 1,
            Trace = 2
        }

        private static Logger shared;
        private readonly object _syncLock = new object();

        private string _logFile = Settings.Default().Get<string>("Logger.path", "lawrence.log");

        private Logger() {
        }

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
            return shared ??= new Logger();
        }

        public static void SetLogFile(string filename) {
            Shared()._logFile = filename;
        }

        public static void Log(string value) {
            Shared().Log(Priority.Log, value);
        }

        public static void Error(string value, Exception exception = null) {
            Shared().Log(Priority.Error, value, exception);
        }

        public static void Trace(string value, Exception exception = null) {
            Shared().Log(Priority.Trace, value, exception);
        }

        public static void Raw(string value) {
            using (var streamWriter = new StreamWriter(Shared()._logFile, true)) {
                streamWriter.WriteLine(value);
            }

            Console.WriteLine(value);
        }
    }
}