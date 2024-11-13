using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    internal class DataStructure
    {
        // スタックとキュー: DFSやBFSで使われ、先入先出（FIFO）や後入先出（LIFO）の操作が必要な場面で役立つ

        // 優先度付きキュー: ダイクストラ法やヒープソートに使われ、データの順序付けが可能。
        public class PriorityQueue<T> where T : IComparable<T>
        {
            private SortedSet<(T Item, int Order)> queue;
            private int orderCounter = 0;

            public PriorityQueue()
            {
                // 順序と優先度で比較されるようカスタムコンパレータを指定
                queue = new SortedSet<(T, int)>(Comparer<(T, int)>.Create((x, y) =>
                {
                    //int cmp = x.Item.CompareTo(y.Item);
                    //return cmp == 0 ? x.Order.CompareTo(y.Order) : cmp;
                }));
            }

            // 要素を追加
            public void Enqueue(T item)
            {
                queue.Add((item, orderCounter++));
            }

            // 優先度の高い要素を取り出し削除
            public T Dequeue()
            {
                if (queue.Count == 0) throw new InvalidOperationException("Queue is empty.");
                var item = queue.Min;
                queue.Remove(item);
                return item.Item;
            }

            // キューの要素数を返す
            public int Count => queue.Count;
        }

        // セグメントツリー: 配列の範囲クエリ処理に使う。例えば区間の最小値や最大値、区間和を高速に求める。
        public class SegmentTree
        {
            private int[] tree;
            private int n;

            public SegmentTree(int[] array)
            {
                n = array.Length;
                tree = new int[2 * n];
                Array.Copy(array, 0, tree, n, n);
                for (int i = n - 1; i > 0; --i)
                    tree[i] = tree[i << 1] + tree[i << 1 | 1];
            }

            public void Update(int pos, int value)
            {
                pos += n;
                tree[pos] = value;
                for (pos >>= 1; pos > 0; pos >>= 1)
                    tree[pos] = tree[pos << 1] + tree[pos << 1 | 1];
            }

            public int Query(int left, int right)
            {
                int result = 0;
                for (left += n, right += n; left < right; left >>= 1, right >>= 1)
                {
                    if ((left & 1) > 0) result += tree[left++];
                    if ((right & 1) > 0) result += tree[--right];
                }
                return result;
            }
        }

        // BIT（Binary Indexed Tree、Fenwick Tree）: 累積和の更新とクエリを高速に行うためのデータ構造。
        public class BinaryIndexedTree
        {
            private int[] tree;
            private int n;

            // コンストラクタ：指定したサイズのBinary Indexed Treeを初期化
            public BinaryIndexedTree(int size)
            {
                n = size;
                tree = new int[n + 1]; // 1-based indexなので、サイズ+1
            }

            // 要素の追加・更新メソッド：index位置にvalueを追加
            public void Update(int index, int value)
            {
                for (int i = index; i <= n; i += (i & -i))
                {
                    tree[i] += value;
                }
            }

            // 1からindexまでの累積和を取得するメソッド
            public int Query(int index)
            {
                int sum = 0;
                for (int i = index; i > 0; i -= (i & -i))
                {
                    sum += tree[i];
                }
                return sum;
            }

            // 指定した範囲[left, right]の区間和を取得するメソッド
            public int RangeQuery(int left, int right)
            {
                return Query(right) - Query(left - 1);
            }
        }
    }
}
