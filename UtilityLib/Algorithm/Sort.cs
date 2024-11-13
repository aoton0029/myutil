using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    internal class Sort
    {
        // クイックソート: 平均O(N log N)で動作するソート。競技プログラミングでよく使用。
        public void QuickSort(int[] array, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(array, left, right);
                QuickSort(array, left, pivot - 1);
                QuickSort(array, pivot + 1, right);
            }
        }

        private int Partition(int[] array, int left, int right)
        {
            int pivot = array[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                if (array[j] < pivot)
                {
                    i++;
                    (array[i], array[j]) = (array[j], array[i]);
                }
            }
            (array[i + 1], array[right]) = (array[right], array[i + 1]);
            return i + 1;
        }

        // マージソート: 安定ソートで、分割統治法に基づくソートアルゴリズム。再帰的に配列を分割してマージ。
        public int[] MergeSort(int[] array)
        {
            if (array.Length <= 1) return array;
            int mid = array.Length / 2;
            int[] left = MergeSort(array[..mid]);
            int[] right = MergeSort(array[mid..]);
            return Merge(left, right);
        }

        private int[] Merge(int[] left, int[] right)
        {
            int[] result = new int[left.Length + right.Length];
            int i = 0, j = 0, k = 0;
            while (i < left.Length && j < right.Length)
                result[k++] = left[i] < right[j] ? left[i++] : right[j++];
            while (i < left.Length) result[k++] = left[i++];
            while (j < right.Length) result[k++] = right[j++];
            return result;
        }
        // ヒープソート: ヒープ構造を使ったソート。効率が良いが、安定ソートではない。
    }
}
