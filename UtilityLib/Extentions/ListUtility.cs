using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Extentions
{
    public static class ListUtility
    {
        public static void AutoFillDefault<T>(this List<T> self, int count)
        {
            for (int x = 0; x < count; ++x)
            {
                self.Add(default(T));
            }
        }
    }
}
