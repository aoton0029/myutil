using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    internal class BitOperations
    {
        public static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));

        public static ulong RotateLeft(ulong value, int offset) => (value << offset) | (value >> (64 - offset));
    }
}
