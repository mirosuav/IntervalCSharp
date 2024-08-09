using System.Numerics;

namespace IntervalCSharp;
public static class Extensions
{
    /// <summary>
    /// Check contains given interval entirely with inclusive bounds
    /// </summary>
    public static bool Contains<T>(this Interval<T> value, Interval<T> other)
            where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>
        => value.Min <= other.Min && value.Max >= other.Max;

    /// <summary>
    /// Check it overlaps with given interval, meaning at least one bound is common for both
    /// </summary>
    public static bool Overlaps<T>(this Interval<T> value, Interval<T> other)
            where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>
        => value.Min <= other.Max && other.Min <= value.Max;
}
