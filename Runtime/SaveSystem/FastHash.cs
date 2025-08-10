namespace GameDevKit
{
    public static class FastHash
    {
        // 32-bit FNV-1a prime & offset
        private const uint FnvPrime = 16777619;
        private const uint FnvOffset = 2166136261;

        public static uint Compute(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            uint hash = FnvOffset;

            // FNV-1a byte-wise hash
            for (int i = 0; i < text.Length; i++)
            {
                hash ^= text[i];      // XOR with char (UTF-16 code unit)
                hash *= FnvPrime;     // multiply by prime
            }

            return hash;
        }
    }
}