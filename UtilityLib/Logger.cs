using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class AsyncFileLogger : IDisposable
    {
        private readonly string _logFilePath;
        private FileStream _fileStream;
        private readonly object _lockObj = new object();

        public AsyncFileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
            _fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, true);
        }

        public async Task LogAsync(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}";
            byte[] bytes = Encoding.UTF8.GetBytes(logEntry);

            // lockで排他制御を行い、複数スレッドからの同時書き込みを防ぐ
            lock (_lockObj)
            {
                // 非同期的に書き込みを行う
                _fileStream.WriteAsync(bytes, 0, bytes.Length);
            }

            // 非同期的にバッファをディスクにフラッシュ
            await _fileStream.FlushAsync();
        }

        public void Dispose()
        {
            // リソースを解放
            _fileStream?.Dispose();
        }
    }
}
