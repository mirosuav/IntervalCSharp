using System.Numerics;
using IntervalCSharp.Helpers;

namespace IntervalCSharp;
public static class Functions
{
    public static Interval<T> Abs<T>(this Interval<T> value)
            where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>, IPowerFunctions<T>
    {
        if (value.HasZero)
        {
            return new(T.Zero, MathHelper.Max(T.Abs(value.Inf), T.Abs(value.Sup)));
        }
        else
            return new Interval<T>(MathHelper.Min(T.Abs(value.Inf), T.Abs(value.Sup)), MathHelper.Max(T.Abs(value.Inf), T.Abs(value.Sup)));
    }

    public static Interval<T> Pow<T>(this Interval<T> value, T exponent)
            where T : struct, IComparisonOperators<T, T, bool>, IPowerFunctions<T>
    {
        if (T.IsNegative(exponent))
            return Interval<T>.One / value.Pow(-exponent);

        T min, max;

        try
        {
            if (!T.IsEvenInteger(exponent) || value.Inf >= T.Zero)
            {
                FPURounding.Down();
                min = T.Pow(value.Inf, exponent);
                FPURounding.Up();
                max = T.Pow(value.Sup, exponent);
            }
            else if (T.IsEvenInteger(exponent) && value.Sup <= T.Zero)
            {
                FPURounding.Down();
                max = T.Pow(value.Sup, exponent);
                FPURounding.Up();
                min = T.Pow(value.Inf, exponent);
            }
            else //if (T.IsEvenInteger(exponent) && value.HasZero)
            {
                min = T.Zero;
                FPURounding.Up();
                max = T.Pow(MathHelper.Max(T.Abs(value.Inf), T.Abs(value.Sup)), exponent);
            }
        }
        finally
        {
            FPURounding.Reset();
        }
        return new Interval<T>(min, max);
    }
}
