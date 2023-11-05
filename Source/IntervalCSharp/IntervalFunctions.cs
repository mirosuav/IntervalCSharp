using System.Numerics;

namespace IntervalCSharp;
public static class IntervalFunctions
{
    public static Interval<T> Abs<T>(this Interval<T> value)
            where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>
    {
        if (value.HasZero)
        {
            return new(T.Zero, MathExtensions.Max(T.Abs(value.Min), T.Abs(value.Max)));
        }
        else
            return new Interval<T>(T.Abs(value.Min), T.Abs(value.Max));
    }
}
