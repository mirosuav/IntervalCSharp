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
            return new(T.Zero, MathHelper.Max(T.Abs(value.Min), T.Abs(value.Max)));
        }
        else
            return new Interval<T>(MathHelper.Min(T.Abs(value.Min), T.Abs(value.Max)), MathHelper.Max(T.Abs(value.Min), T.Abs(value.Max)));
    }

    public static Interval<T> Pow<T>(this Interval<T> value, T exponent)
            where T : struct,
        IComparisonOperators<T, T, bool>,
        IPowerFunctions<T>
    {
        if (T.IsNegative(exponent))
            return Interval<T>.One / value.Pow(-exponent);

        T min, max;

        try
        {
            if (!T.IsEvenInteger(exponent) || value.Min >= T.Zero)
            {
                FPURounding.Down();
                min = T.Pow(value.Min, exponent);
                FPURounding.Up();
                max = T.Pow(value.Max, -exponent);
            }
            else if (T.IsEvenInteger(exponent) && value.Max <= T.Zero)
            {
                FPURounding.Down();
                min = T.Pow(value.Min, -exponent);
                FPURounding.Up();
                max = T.Pow(value.Max, exponent);
            }
            else //if (T.IsEvenInteger(exponent) && value.HasZero)
            {
                min = T.Zero;
                FPURounding.Up();
                max = T.Pow(MathHelper.Max(T.Abs(value.Min), T.Abs(value.Max)), exponent);
            }
        }
        finally
        {
            FPURounding.Reset();
        }
        return new Interval<T>(min, max);
    }
}
