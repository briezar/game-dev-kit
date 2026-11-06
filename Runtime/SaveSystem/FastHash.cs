namespace GameDevKit
{
    public static class FastHash
    {
        // 32-bit FNV-1a prime & offset
        private const uint FnvPrime = 16777619;
        private const uint FnvOffset = 2166136261;

        /// <summary> FNV-1a over UTF-16 code units. Returns 0 for null or empty text. Detects accidental corruption, not tampering. </summary>
        public static uint Compute(string text)
        {
            if (string.IsNullOrEmpty(text)) { return 0; }

            unchecked
            {
                var hash = FnvOffset;
                for (var i = 0; i < text.Length; i++)
                {
                    hash ^= text[i];   // XOR with char (UTF-16 code unit)
                    hash *= FnvPrime;  // multiply by prime
                }
                return hash;
            }
        }
    }
}