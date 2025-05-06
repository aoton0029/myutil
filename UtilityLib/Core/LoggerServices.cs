using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public class LoggingService
    {
        private readonly string _logFilePath;
        private readonly bool _consoleOutput;
        private readonly object _lockObj = new();

        public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

        public LoggingService(string logFilePath = null, bool consoleOutput = true)
        {
            _logFilePath = logFilePath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SearachAppSample", "logs", $"app_{DateTime.Now:yyyyMMdd}.log");

            _consoleOutput = consoleOutput;

            // ログディレクトリの作成
            Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath));
        }

        public void Log(LogLevel level, string message, Exception ex = null)
        {
            if (level < MinimumLevel) return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] [{level}] {message}";

            if (ex != null)
            {
                logMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }

            lock (_lockObj)
            {
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);

                if (_consoleOutput)
                {
                    System.Diagnostics.Debug.WriteLine(logMessage);
                }
            }
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message, Exception ex = null) => Log(LogLevel.Error, message, ex);
        public void Fatal(string message, Exception ex = null) => Log(LogLevel.Fatal, message, ex);
    }
}
