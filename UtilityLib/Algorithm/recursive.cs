using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    public class recursive
    {
        public static void dfs(int node, List<int>[] graph, bool[] visited)
        {
            visited[node] = true;
            Console.WriteLine(node);
            foreach (int i in graph[node])
            {
                if (!visited[i])
                {
                    dfs(i, graph, visited);
                }
            }
        }

        
        // しゃくとり法
        // 条件」を満たす区間 (連続する部分列) のうち、最小の長さを求めよ
        //「条件」を満たす区間(連続する部分列) のうち、最大の長さを求めよ
        //「条件」を満たす区間(連続する部分列) を数え上げよ
        public static void shakutori()
        {
            int[] a = { 1, 2, 3, 4, 5 };
            int n = a.Length;
            int k = 3;
            int sum = 0;
            int right = 0;
            for (int left = 0; left < n; left++)
            {
                while (right < n && sum + a[right] < k)
                {
                    sum += a[right];
                    right++;
                }
                // ここで区間 [left, right) が条件を満たす
                Console.WriteLine(left + " " + right);
                if (right == left)
                {
                    right++;
                }
                else
                {
                    sum -= a[left];
                }
            }
        }
    }
}
