using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Extentions
{
    internal static class ArrayExt
    {
        public static T[] Merge<T>(this T[] a, T[] b)
        {
            T[] c = new T[a.Length + b.Length];
            a.CopyTo(c, 0);
            b.CopyTo(c, a.Length);
            return c;
        }

        public static bool SequenceEqual<T>(this T[] a, T[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].Equals(b[i]))
                    return false;
            }

            return true;
        }

        public static string ToUTF8(this byte[] content, int? index = null, int? count = null)
        {
            if (index is null || count is null)
                return Encoding.UTF8.GetString(content);
            else
                return Encoding.UTF8.GetString(
                    content,
                    index ?? 0,
                    count ?? content.Length
                );
        }

        public static string ToUTF32(this byte[] content, int? index = null, int? count = null)
        {
            if (index is null || count is null)
                return Encoding.UTF32.GetString(content);
            else
                return Encoding.UTF32.GetString(
                    content,
                    index ?? 0,
                    count ?? content.Length
                );
        }

        public static string ToUnicode(this byte[] content, int? index = null, int? count = null)
        {
            if (index is null || count is null)
                return Encoding.Unicode.GetString(content);
            else
                return Encoding.Unicode.GetString(
                    content,
                    index ?? 0,
                    count ?? content.Length
                );
        }

        public static string ToASCII(this byte[] content, int? index = null, int? count = null)
        {
            if (index is null || count is null)
                return Encoding.ASCII.GetString(content);
            else
                return Encoding.ASCII.GetString(
                    content,
                    index ?? 0,
                    count ?? content.Length
                );
        }

        public static T[] Concat<T>(this T[] array, T[] other)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var result = new T[array.Length + other.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            Array.Copy(other, 0, result, array.Length, other.Length);
            return result;
        }

        public static T[] Concat<T>(this T[][] arrays)
        {
            if (arrays is null)
            {
                throw new ArgumentNullException(nameof(arrays));
            }

            int length = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                var array = arrays[i] ?? Array.Empty<T>();
                length += array.Length;
            }

            var result = new T[length];
            int index = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                var array = arrays[i] ?? Array.Empty<T>();
                Array.Copy(array, 0, result, index, array.Length);
                index += array.Length;
            }
            return result;
        }

        public static T[] InitializeArray<T>(this int length, Converter<int, T> indexMap)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            if (indexMap is null)
            {
                throw new ArgumentNullException(nameof(indexMap));
            }

            var result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = indexMap(i);
            }
            return result;
        }
    }
}
