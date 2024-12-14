using System.Diagnostics;

namespace DataSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            const int arraySize = 10_000_000;

            // �N���X�^�̔z��
            var stopwatch = Stopwatch.StartNew();
            PointClass[] classArray = new PointClass[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                classArray[i] = new PointClass(i, i);
            }
            stopwatch.Stop();
            Debug.WriteLine($"�N���X�^ �z��̏���������: {stopwatch.ElapsedMilliseconds}ms");

            // �\���̌^�̔z��
            stopwatch.Restart();
            PointStruct[] structArray = new PointStruct[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                structArray[i] = new PointStruct(i, i);
            }
            stopwatch.Stop();
            Debug.WriteLine($"�\���̌^ �z��̏���������: {stopwatch.ElapsedMilliseconds}ms");

            // �������g�p��
            GC.Collect();
            long memoryUsed = Process.GetCurrentProcess().PrivateMemorySize64;
            Debug.WriteLine($"���݂̃������g�p��: {memoryUsed / 1024 / 1024} MB");
        }

        // �N���X�^
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

        // �\���̌^
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
