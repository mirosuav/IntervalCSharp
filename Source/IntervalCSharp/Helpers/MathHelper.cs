using System.Numerics;

namespace IntervalCSharp.Helpers;
public static class MathHelper
{
    internal static readonly InvalidOperationException NoParamsEx = new("Empty parameters array provided.");
    public static TNum Min<TNum>(params TNum[] numbers) where TNum : IComparisonOperators<TNum, TNum, bool>
    {
        if (numbers is null or { Length: 0 })
            throw NoParamsEx;

        uint minIDx = 0;
        for (uint i = 0; i < numbers.Length; i++)
        {
            if (numbers[i] < numbers[minIDx])
                minIDx = i;
        }
        return numbers[minIDx];
    }
    public static TNum Max<TNum>(params TNum[] numbers) where TNum : IComparisonOperators<TNum, TNum, bool>
    {
        if (numbers is null or { Length: 0 })
            throw NoParamsEx;

        uint maxIDx = 0;
        for (uint i = 0; i < numbers.Length; i++)
        {
            if (numbers[i] > numbers[maxIDx])
                maxIDx = i;
        }
        return numbers[maxIDx];
    }
    public static bool IsEven(this int number) => number % 2 == 0;
    public static bool IsOdd(this int number) => number % 2 == 0;
}
