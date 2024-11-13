using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    internal class NumberTheory
    {
        // エラトステネスの篩: 指定範囲内の素数を効率よく求めるアルゴリズム。
        public List<int> SieveOfEratosthenes(int limit)
        {
            bool[] isPrime = Enumerable.Repeat(true, limit + 1).ToArray();
            List<int> primes = new();
            for (int i = 2; i <= limit; i++)
            {
                if (isPrime[i])
                {
                    primes.Add(i);
                    for (int j = i * 2; j <= limit; j += i)
                        isPrime[j] = false;
                }
            }
            return primes;
        }

        // ユークリッドの互除法: 最大公約数（GCD）を求めるためのアルゴリズム。
        public int GCD(int a, int b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }


        // 拡張ユークリッド法: ax + by = GCD(a, b)の整数解を求める。特に数論問題で用いる。

        // モジュラ逆元: 除算が必要なモジュラ計算に用いる。フェルマーの小定理を利用。
        public int ModInverse(int a, int mod)
        {
            return PowerMod(a, mod - 2, mod);
        }

        private int PowerMod(int baseValue, int exponent, int mod)
        {
            int result = 1;
            baseValue %= mod;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1) result = (result * baseValue) % mod;
                baseValue = (baseValue * baseValue) % mod;
                exponent >>= 1;
            }
            return result;
        }
    }
}
