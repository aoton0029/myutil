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

        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even number of characters");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }
    }
}
