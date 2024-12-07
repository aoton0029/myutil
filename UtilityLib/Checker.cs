using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public static class Checker
    {
        public static T KeepPos<T>(T value) where T : struct, IComparable<T>
        {
            var zero = default(T);
            return value.CompareTo(zero) < 0 ? zero : value;
        }
    }
}
