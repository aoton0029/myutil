using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            foreach(int i in Enumerable.Range(1, 5))
            {
                Debug.WriteLine(i);
                //mm(10, 4, i);
            }
            var ds = CountWays(10, 4);
            Debug.WriteLine(string.Join(", ", ds));
        }



        private int CountWays(int balls, int boxes)
        {
            int[,] dp = new int[balls + 1, boxes + 1];

            for (int i = 0; i <= boxes; i++)
                dp[0, i] = 1;

            for (int i = 1; i <= balls; i++)
            {
                for (int j = 1; j <= boxes; j++)
                {
                    dp[i, j] = dp[i - 1, j] + dp[i, j - 1];
                }
            }

            return dp[balls, boxes];
        }

        private void mm(int balls, int boxes, int min)
        {
            List<int[]> results = new List<int[]>();
            int[] current = new int[boxes];

            // 最小個数を各箱に割り振る
            int totalMinBalls = min * boxes;
            if (totalMinBalls > balls)
            {
                Debug.WriteLine("割り振る玉の数が足りません。");
                return;
            }
            balls -= totalMinBalls;

            // 残りの玉を割り振る
            DistributeBalls(balls, boxes, 0, current, results, min);

            // 結果の表示
            foreach (var result in results)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] += min; // 最小個数を加える
                }
                Console.WriteLine(string.Join(", ", result));
            }

        }

        static void DistributeBalls(int balls, int boxes, int index, int[] current, List<int[]> results, int minBallsPerBox)
        {
            if (index == boxes - 1)
            {
                current[index] = balls;
                results.Add((int[])current.Clone());
                return;
            }

            for (int i = 0; i <= balls; i++)
            {
                current[index] = i;
                DistributeBalls(balls - i, boxes, index + 1, current, results, minBallsPerBox);
            }
        }

        // 5個の箱に玉12個を割り振るアルゴリズム。
        private void m(int max, int box, int min)
        {
            int[] a = new int[box];
            if (max < box * min)
            {
                Debug.WriteLine("Error");
                return;
            }
            for (int i = 0; i < box; i++)
            {
                a[i] = min;
            }
            max -= box * min;
            for (int i = 0; i < max; i++)
            {
                a[i % box]++;
            }
            Debug.WriteLine(string.Join(", ", a));
        }


    }
}
