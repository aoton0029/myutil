using System.Diagnostics;

namespace DataSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            const int arraySize = 10_000_000;

            // クラス型の配列
            var stopwatch = Stopwatch.StartNew();
            PointClass[] classArray = new PointClass[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                classArray[i] = new PointClass(i, i);
            }
            stopwatch.Stop();
            Debug.WriteLine($"クラス型 配列の初期化時間: {stopwatch.ElapsedMilliseconds}ms");

            // 構造体型の配列
            stopwatch.Restart();
            PointStruct[] structArray = new PointStruct[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                structArray[i] = new PointStruct(i, i);
            }
            stopwatch.Stop();
            Debug.WriteLine($"構造体型 配列の初期化時間: {stopwatch.ElapsedMilliseconds}ms");

            // メモリ使用量
            GC.Collect();
            long memoryUsed = Process.GetCurrentProcess().PrivateMemorySize64;
            Debug.WriteLine($"現在のメモリ使用量: {memoryUsed / 1024 / 1024} MB");
        }

        // クラス型
        public class PointClass
        {
            public int X { get; set; }
            public int Y { get; set; }

            public PointClass(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        // 構造体型
        public struct PointStruct
        {
            public int X { get; set; }
            public int Y { get; set; }

            public PointStruct(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}
