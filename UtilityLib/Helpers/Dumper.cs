using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Helpers
{
    internal static class Dumper
    {
        public static string Dump<T>(this Queue<T> queue, string separater = " ", bool print = true)
        {
            var result = new StringBuilder();
            queue.ForEach(x =>
            {
                result.Append(x?.ToString());
                result.Append(separater);
            }, true);
            if (print) result.ToString().Print();
            return result.ToString();
        }

        public static string[] Dump2Lines<T>(this Queue<T> queue, bool print = true)
        {
            var result = new List<string>();
            queue.ForEach(x => result.Add(x?.ToString() ?? string.Empty), true);
            if (print) result.ToArray().Print();
            return [.. result];
        }


        public static string Print<T>(this T? src, bool print = true)
        {
            if (print)
                Console.WriteLine(src?.ToString());

            return src?.ToString() ?? "";
        }

        public static string Print<T>
        (
            this IEnumerable<T> array,
            string? connection = ", ",
            bool cutEnding = true,
            bool newLineConnection4StringArray = true,
            bool print = true
        )
        {
            if (array is Queue<T> queue) return Dump(queue, connection ?? " ", print);

            var sb = new StringBuilder();

            var useNewLine2ReplaceConnectionString
                = newLineConnection4StringArray && array is IEnumerable<string>;

            foreach (var item in array)
            {
                sb.Append(item?.ToString());

                if (useNewLine2ReplaceConnectionString)
                    sb.Append(Environment.NewLine);
                else sb.Append(connection);
            }

            var result = sb.ToString();

            if (useNewLine2ReplaceConnectionString)
            {
                if (print)
                    Console.WriteLine(result);

                return result;
            }

            if (cutEnding)
                result = result[..(sb.Length - connection?.Length ?? 0)];

            if (print)
                Console.WriteLine(result);

            return result;
        }
    }
}
