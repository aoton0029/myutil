using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    internal class ZAlgorithm
    {
        public static int[] CalculateZ(string s)
        {
            int n = s.Length;
            int[] z = new int[n];
            int l = 0, r = 0;

            for (int i = 1; i < n; i++)
            {
                if (i <= r)
                {
                    z[i] = Math.Min(r - i + 1, z[i - l]);
                }
                while (i + z[i] < n && s[z[i]] == s[i + z[i]])
                {
                    z[i]++;
                }
                if (i + z[i] - 1 > r)
                {
                    l = i;
                    r = i + z[i] - 1;
                }
            }

            return z;
        }

        public static List<int> SearchPattern(string text, string pattern)
        {
            string combined = pattern + "$" + text;
            int[] z = CalculateZ(combined);
            List<int> result = new List<int>();

            for (int i = 0; i < z.Length; i++)
            {
                if (z[i] == pattern.Length)
                {
                    result.Add(i - pattern.Length - 1);
                }
            }

            return result;
        }

    }
}
