using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Tasks
{
    public class TcpClientWrapper : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public TcpClientWrapper(string ipAddress, int port)
        {
            _client = new TcpClient();
            _client.Connect(ipAddress, port);
            _stream = _client.GetStream();
        }

        public async Task<string> SendQueryAsync(string query, CancellationToken token)
        {
            byte[] queryBytes = Encoding.ASCII.GetBytes(query);
            await _stream.WriteAsync(queryBytes, 0, queryBytes.Length, token);

            byte[] buffer = new byte[1024];
            int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _client?.Close();
        }
    }

    public class TaskManager
    {
        private readonly ConcurrentDictionary<int, Task> _tasks;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TaskManager()
        {
            _tasks = new ConcurrentDictionary<int, Task>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        // 新しいタスクを追加し管理する
        public int AddTask(Func<CancellationToken, Task> taskFunc)
        {
            var token = _cancellationTokenSource.Token;
            var task = Task.Run(() => taskFunc(token), token);
            int taskId = task.Id;
            if (!_tasks.TryAdd(taskId, task))
            {
                throw new Exception("タスクの追加に失敗しました。");
            }
            return taskId;
        }

        // 特定のタスクをキャンセルする
        public void CancelTask(int taskId)
        {
            if (_tasks.TryRemove(taskId, out var task))
            {
                // タスクのキャンセルをリクエストする
                _cancellationTokenSource.Cancel();
                Console.WriteLine($"タスク {taskId} がキャンセルされました。");
            }
            else
            {
                Console.WriteLine($"タスク {taskId} は存在しません。");
            }
        }

        // 全てのタスクをキャンセルする
        public void CancelAllTasks()
        {
            _cancellationTokenSource.Cancel();
            Console.WriteLine("全てのタスクがキャンセルされました。");
        }

        // 全てのタスクの完了を待機する
        public async Task WaitAllTasksAsync()
        {
            await Task.WhenAll(_tasks.Values);
        }

        // タスクの状態を表示する
        public void DisplayTaskStatus()
        {
            foreach (var kvp in _tasks)
            {
                Console.WriteLine($"タスクID: {kvp.Key}, 状態: {kvp.Value.Status}");
            }
        }
    }

}
