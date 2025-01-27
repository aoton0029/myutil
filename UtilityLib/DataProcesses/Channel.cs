using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses
{
    public class Channel<T>
    {
        private readonly BlockingCollection<T> _queue = new BlockingCollection<T>();

        // データを送信
        public void Send(T item)
        {
            if (!_queue.IsAddingCompleted)
            {
                _queue.Add(item);
            }
            else
            {
                throw new InvalidOperationException("Channel is closed and cannot send data.");
            }
        }

        // チャネルを閉じる
        public void Close()
        {
            _queue.CompleteAdding();
        }

        // データを受信
        public T Receive()
        {
            return _queue.Take();
        }

        // チャネルが閉じられたかを確認
        public bool IsClosed => _queue.IsAddingCompleted;

        // データが残っているかを確認
        public bool HasData => _queue.Count > 0;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Channel<DataTable>();

            // シミュレーションするデータ取得メソッドのリスト
            List<Func<DataTable>> dataRetrievalMethods = new List<Func<DataTable>>
        {
            GetDataTable1,
            GetDataTable2,
            GetDataTable3
        };

            // 並列にデータ取得メソッドを実行して結果をチャネルに送信
            Parallel.ForEach(dataRetrievalMethods, method =>
            {
                var result = method();
                channel.Send(result);
            });

            // チャネルを閉じる
            channel.Close();

            // チャネルからデータを受信し、処理
            while (!channel.IsClosed || channel.HasData)
            {
                var dataTable = channel.Receive();
                Console.WriteLine($"Received DataTable with {dataTable.Rows.Count} rows.");
            }
        }

        // データ取得メソッド1
        static DataTable GetDataTable1()
        {
            var table = new DataTable("Table1");
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(1, "Alice");
            table.Rows.Add(2, "Bob");
            Console.WriteLine("Table1 retrieved.");
            return table;
        }

        // データ取得メソッド2
        static DataTable GetDataTable2()
        {
            var table = new DataTable("Table2");
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(3, "Charlie");
            table.Rows.Add(4, "Diana");
            Console.WriteLine("Table2 retrieved.");
            return table;
        }

        // データ取得メソッド3
        static DataTable GetDataTable3()
        {
            var table = new DataTable("Table3");
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add(5, "Eve");
            table.Rows.Add(6, "Frank");
            Console.WriteLine("Table3 retrieved.");
            return table;
        }
    }
}
