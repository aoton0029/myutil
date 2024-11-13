using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    public class Search
    {
        // 深さ優先探索（DFS）: グラフの全てのノードを探索するために使われる。再帰やスタックで実装されることが多い。
        public void DepthFirstSearch(int node, bool[] visited, List<int>[] graph)
        {
            visited[node] = true;
            foreach (var neighbor in graph[node])
            {
                if (!visited[neighbor])
                {
                    DepthFirstSearch(neighbor, visited, graph);
                }
            }
        }
        //幅優先探索（BFS）: 最短経路問題に使われ、キューを用いる。迷路探索などで頻出。
        public void BreadthFirstSearch(int startNode, List<int>[] graph)
        {
            var queue = new Queue<int>();
            bool[] visited = new bool[graph.Length];
            queue.Enqueue(startNode);
            visited[startNode] = true;

            while (queue.Count > 0)
            {
                int node = queue.Dequeue();
                foreach (var neighbor in graph[node])
                {
                    if (!visited[neighbor])
                    {
                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // 二分探索: ソートされた配列や数値範囲内での探索に使われる。O(log N)の効率で探索可能。
        public int BinarySearch(int[] sortedArray, int target)
        {
            int left = 0, right = sortedArray.Length - 1;
            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (sortedArray[mid] == target) return mid;
                else if (sortedArray[mid] < target) left = mid + 1;
                else right = mid - 1;
            }
            return -1; // 見つからない場合
        }

        // 全探索: すべての組み合わせを探索する方法で、パズル系の問題に利用。

        // ビット全探索


    }
}
