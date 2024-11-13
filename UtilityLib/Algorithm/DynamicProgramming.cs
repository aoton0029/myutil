using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    internal class DynamicProgramming
    {
        // 動的計画法

        // ナップサック問題: 部分問題を解いて大きな問題を解決する。重さと価値のトレードオフを考慮する最適化問題で多く用いられる。
        public int Knapsack(int[] weights, int[] values, int capacity)
        {
            int n = weights.Length;
            int[,] dp = new int[n + 1, capacity + 1];
            for (int i = 1; i <= n; i++)
            {
                for (int w = 0; w <= capacity; w++)
                {
                    if (weights[i - 1] <= w)
                        dp[i, w] = Math.Max(dp[i - 1, w], dp[i - 1, w - weights[i - 1]] + values[i - 1]);
                    else
                        dp[i, w] = dp[i - 1, w];
                }
            }
            return dp[n, capacity];
        }
        // 最長共通部分列（LCS）: 文字列操作問題で用いる。2つの文字列間での共通部分列の長さを求める。
        public int LongestCommonSubsequence(string str1, string str2)
        {
            int m = str1.Length, n = str2.Length;
            int[,] dp = new int[m + 1, n + 1];
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (str1[i - 1] == str2[j - 1])
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    else
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                }
            }
            return dp[m, n];
        }

        // 最小コスト経路: グリッド上の最短経路問題やコスト計算に使われる。
    }
}
