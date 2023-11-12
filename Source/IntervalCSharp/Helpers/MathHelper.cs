using System.Numerics;

namespace IntervalCSharp.Helpers;
public static class MathHelper
{
    internal static readonly InvalidOperationException NoParamsEx = new("Empty parameters array provided.");
    public static T Min<T>(params T[] numbers) where T : IComparisonOperators<T, T, bool>
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
    public static T Max<T>(params T[] numbers) where T : IComparisonOperators<T, T, bool>
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
}
