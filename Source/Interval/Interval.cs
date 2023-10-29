using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Interval;

using Interval = Interval<decimal>;
using IntervalDouble = Interval<double>;

public readonly record struct Interval<T>
    : INumberBase<T>, IComparisonOperators<T, T, bool>
    //      IEqualityOperators<Interval, Interval, bool>,
    //      IComparisonOperators<Interval, Interval, bool>,
    //      IEquatable<Interval>,
    //      IComparable<Interval>,
    //      IComparable,
    //      IFormattable
    where T : struct, INumberBase<T>, IComparisonOperators<T, T, bool>
{
    static readonly T _TTwo = T.One + T.One;
    public static readonly Interval<T> Zero = new(T.Zero);
    public static readonly Interval<T> One = new(T.One);

    /// <summary>
    /// Interval lower inclusive bound
    /// </summary>
    public T Min { get; }
    /// <summary>
    /// Interval upper inclusive bound
    /// </summary>
    public T Max { get; }

    public Interval() : this(T.Zero) { }
    public Interval(T point) : this(point, point) { }
    public Interval(Interval<T> other) : this(other.Min, other.Max) { }
    public Interval(T min, T max) => (Min, Max) = (min, max);

    public T Width => Max - Min;
    public T Radius => Max - Min / _TTwo;
    public T Middle => (Min + Max) / _TTwo;
    public bool HasZero => Min <= T.Zero && Max >= T.Zero;


    public static Interval<T> operator +(Interval<T> left, Interval<T> right)
    {
        try
        {
            FPUControl.SetRoundingDOWN();
            T min = left.Min + right.Min;

            FPUControl.SetRoundingUP();
            T max = left.Max + right.Max;

            return new Interval<T>(min, max);
        }
        finally
        {
            FPUControl.RevertRoundingMode();
        }
    }
    public static Interval<T> operator -(Interval<T> left, Interval<T> right)
    {
        try
        {
            FPUControl.SetRoundingDOWN();
            T min = left.Min - right.Max;

            FPUControl.SetRoundingUP();
            T max = left.Max - right.Min;

            return new Interval<T>(min, max);
        }
        finally
        {
            FPUControl.RevertRoundingMode();
        }
    }
    public static Interval<T> operator *(Interval<T> left, Interval<T> right)
    {
        try
        {
            FPUControl.SetRoundingDOWN();
            T min = MathExtensions.Min(left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max);

            FPUControl.SetRoundingUP();
            T max = MathExtensions.Max(left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max);

            return new Interval<T>(min, max);
        }
        finally
        {
            FPUControl.RevertRoundingMode();
        }
    }
    public static Interval<T> operator /(Interval<T> left, Interval<T> right)
    {
        if (right.HasZero)
            throw new DivideByZeroException("Division by interval containing Zero.");
        try
        {
            FPUControl.SetRoundingDOWN();
            T min = MathExtensions.Min(left.Min / right.Min, left.Min / right.Max, left.Max / right.Min, left.Max / right.Max);

            FPUControl.SetRoundingUP();
            T max = MathExtensions.Max(left.Min / right.Min, left.Min / right.Max, left.Max / right.Min, left.Max / right.Max);

            return new Interval<T>(min, max);
        }
        finally
        {
            FPUControl.RevertRoundingMode();
        }
    }

}
