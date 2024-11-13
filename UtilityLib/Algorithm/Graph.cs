using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    internal class Graph
    {
        // グラフアルゴリズム

        // ダイクストラ法: 単一始点最短経路を求めるアルゴリズム。辺の重みが非負の場合に有効。
        public int[] Dijkstra(int startNode, List<(int, int)>[] graph)
        {
            int n = graph.Length;
            int[] distances = Enumerable.Repeat(int.MaxValue, n).ToArray();
            var pq = new SortedSet<(int, int)>();
            distances[startNode] = 0;
            pq.Add((0, startNode));

            while (pq.Any())
            {
                var (dist, node) = pq.Min;
                pq.Remove(pq.Min);

                foreach (var (neighbor, weight) in graph[node])
                {
                    int newDist = dist + weight;
                    if (newDist < distances[neighbor])
                    {
                        pq.Remove((distances[neighbor], neighbor));
                        distances[neighbor] = newDist;
                        pq.Add((newDist, neighbor));
                    }
                }
            }
            return distances;
        }

        // ワーシャルフロイド法: 全点対間の最短経路を求めるアルゴリズム。O(N^3)の計算量。
        private const int INF = int.MaxValue;
        public int[,] CalculateShortestPaths(int[,] graph)
        {
            int n = graph.GetLength(0);
            int[,] dist = new int[n, n];

            // 初期化: グラフの距離をdistにコピー
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    dist[i, j] = graph[i, j];
                }
            }

            // ワーシャルフロイド法のメインループ
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (dist[i, k] != INF && dist[k, j] != INF && dist[i, j] > dist[i, k] + dist[k, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                        }
                    }
                }
            }

            return dist;
        }

        // 結果を表示するメソッド
        public void PrintShortestPaths(int[,] dist)
        {
            int n = dist.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (dist[i, j] == INF)
                        Console.Write("INF ");
                    else
                        Console.Write(dist[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
        // ベルマンフォード法: ダイクストラ法と異なり、負の重みがあるグラフでも最短経路を求められる。

        // 最小全域木（MST）: クラスカル法やプリム法がよく使われる。最小コストで全てのノードを繋ぐ。
        public class Edge : IComparable<Edge>
        {
            public int From;
            public int To;
            public int Weight;

            public Edge(int from, int to, int weight)
            {
                From = from;
                To = to;
                Weight = weight;
            }

            // 重みを基準にソートするための比較
            public int CompareTo(Edge other)
            {
                return Weight.CompareTo(other.Weight);
            }
        }

        public class UnionFind
        {
            private int[] parent;
            private int[] rank;

            public UnionFind(int size)
            {
                parent = new int[size];
                rank = new int[size];
                for (int i = 0; i < size; i++)
                {
                    parent[i] = i;
                    rank[i] = 0;
                }
            }

            // Findメソッド: 頂点xの根を見つける
            public int Find(int x)
            {
                if (parent[x] != x)
                {
                    parent[x] = Find(parent[x]); // 経路圧縮
                }
                return parent[x];
            }

            // Unionメソッド: 2つの集合を統合する
            public void Union(int x, int y)
            {
                int rootX = Find(x);
                int rootY = Find(y);

                if (rootX != rootY)
                {
                    // ランクに基づいてマージ
                    if (rank[rootX] > rank[rootY])
                    {
                        parent[rootY] = rootX;
                    }
                    else if (rank[rootX] < rank[rootY])
                    {
                        parent[rootX] = rootY;
                    }
                    else
                    {
                        parent[rootY] = rootX;
                        rank[rootX]++;
                    }
                }
            }
        }

        public class KruskalMST
        {
            public static List<Edge> FindMST(int vertices, List<Edge> edges)
            {
                var mst = new List<Edge>();
                var unionFind = new UnionFind(vertices);

                // 重みの昇順で辺をソート
                edges.Sort();

                foreach (var edge in edges)
                {
                    // サイクルが発生しない場合のみ、MSTに追加
                    if (unionFind.Find(edge.From) != unionFind.Find(edge.To))
                    {
                        mst.Add(edge);
                        unionFind.Union(edge.From, edge.To);
                    }
                }

                return mst;
            }
        }
    }
}
