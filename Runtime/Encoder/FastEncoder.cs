using System;
using UnityEngine;

namespace GameDevKit
{
    public class FastEncoder
    {
        public static readonly FastEncoder Base62 = new("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

        /// <summary>
        /// Combine <see cref="Base62"/> and <see cref="Base32"/> but remove 'l'
        /// </summary>
        public static readonly FastEncoder Base57 = new("ABCDEFGHJKMNPQRSTVWXYZabcdefghijkmnopqrstuvwxyz0123456789");

        /// <summary>
        /// Crockford's Base32 which ULID uses. This excludes the letters I, L, O, and U to avoid confusion.
        /// </summary>
        public static readonly FastEncoder Base32 = new("0123456789ABCDEFGHJKMNPQRSTVWXYZ");

        private const int BitsInLong = 64;

        public readonly string BaseCharacters;
        public readonly uint Radix;

        public FastEncoder(string baseCharacters)
        {
            if (baseCharacters == null || baseCharacters.Length < 2)
            {
                throw new ArgumentException($"Must be at least Base2!");
            }

            BaseCharacters = baseCharacters;
            Radix = (uint)baseCharacters.Length;
        }

        public string Encode(ulong decimalNumber)
        {
            if (decimalNumber == 0) { return "0"; }

            int index = BitsInLong - 1;
            ulong currentNumber = decimalNumber;
            char[] charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % Radix);
                charArray[index--] = BaseCharacters[remainder];
                currentNumber /= Radix;
            }

            string result = new(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }

            return result;
        }

        public ulong Decode(string stringNumber)
        {
            if (string.IsNullOrEmpty(stringNumber)) { return 0; }

            ulong result = 0;
            ulong multiplier = 1;
            for (int i = stringNumber.Length - 1; i >= 0; i--)
            {
                char c = stringNumber[i];

                int digit = BaseCharacters.IndexOf(c);
                if (digit == -1) { throw new ArgumentException($"Invalid character ({c}) in the arbitrary numeral system number", "number"); }
                result += (uint)digit * multiplier;
                multiplier *= Radix;
            }

            return result;
        }
    }
}