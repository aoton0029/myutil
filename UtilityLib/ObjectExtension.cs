using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    internal class ObjectExtension
    {
        public static bool OperatorEquality<T>(T left, T right)
            where T : class, IEquatable<T>
        {
            if (object.ReferenceEquals(left, right))
                return true;
            else if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                return false;
            else
                return left.Equals(right);
        }
    }
}
