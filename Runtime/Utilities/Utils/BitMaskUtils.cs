using UnityEngine;

public static class BitMaskUtils
{
    public static bool HasFlag(int mask, int bit) => (mask & (1 << bit)) != 0;
    public static int AddFlag(int mask, int bit) => mask | (1 << bit);
    public static int RemoveFlag(int mask, int bit) => mask & ~(1 << bit);
}