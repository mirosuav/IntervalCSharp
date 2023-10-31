using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Interval;

using Interval = Interval<decimal>;
using DoubleInterval = Interval<double>;
using FloatInterval = Interval<float>;

/// <summary>
/// Interval number defined by Min and Max inclusive bounds. Implements interval arithmetic operations. 
/// </summary>
/// <typeparam name="T">Numeric type of the interval boundaries.</typeparam>
public readonly record struct Interval<T>
      : IAdditionOperators<Interval<T>, Interval<T>, Interval<T>>,
        IAdditiveIdentity<Interval<T>, Interval<T>>,
        IDecrementOperators<Interval<T>>,
        IDivisionOperators<Interval<T>, Interval<T>, Interval<T>>,
        IEquatable<Interval<T>>,
        IEqualityOperators<Interval<T>, Interval<T>, bool>,
        IIncrementOperators<Interval<T>>,
        IMultiplicativeIdentity<Interval<T>, Interval<T>>,
        IMultiplyOperators<Interval<T>, Interval<T>, Interval<T>>,
        ISubtractionOperators<Interval<T>, Interval<T>, Interval<T>>,
        IUnaryPlusOperators<Interval<T>, Interval<T>>,
        IUnaryNegationOperators<Interval<T>, Interval<T>>,
        IComparisonOperators<Interval<T>, Interval<T>, bool>,
        IComparable<Interval<T>>,
        IComparable,
        IFormattable
    where T : struct //it expects float, double or decimal
            , INumberBase<T>
            , IComparisonOperators<T, T, bool>
            , IFormattable
{
    /// <summary>
    /// Interval lower inclusive bound
    /// </summary>
    public T Min { get; }
    /// <summary>
    /// Interval upper inclusive bound
    /// </summary>
    public T Max { get; }

    //Constructors
    public Interval() : this(T.Zero) { }
    public Interval(T point) : this(point, point) { }
    public Interval(Interval<T> other) : this(other.Min, other.Max) { }
    public Interval(T min, T max)
    {
        if (max < min)
            throw new InvalidOperationException("Cannot define interval");
        Min = min;
        Max = max;
    }

    //Utility properties
    public bool IsPoint => Min == Max;
    public T Width => Max - Min;
    public T Radius => Max - Min / _TTwo;
    public T Middle => (Min + Max) / _TTwo;
    public bool HasZero => Min <= T.Zero && Max >= T.Zero;
    public static int Radix => T.Radix;
    public static Interval<T> Zero => _Zero;
    public static Interval<T> One => _One;
    public static Interval<T> AdditiveIdentity => Interval<T>.Zero;
    public static Interval<T> MultiplicativeIdentity => Interval<T>.One;

    //Basic arithmetic operators
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
    public static Interval<T> operator --(Interval<T> value)
        => new(value.Min - T.One, value.Max - T.One);
    public static Interval<T> operator ++(Interval<T> value)
        => new(value.Min + T.One, value.Max + T.One);
    public static Interval<T> operator +(Interval<T> value)
        => value;
    public static Interval<T> operator -(Interval<T> value)
        => new(-value.Max, -value.Min);

    //Comparable operators
    public static bool operator ==(Interval<T>? left, Interval<T>? right)
       => left is null ? right is null
       : right is null ? false
       : left.Value.Min == right.Value.Min && left.Value.Max == right.Value.Max;
    public static bool operator !=(Interval<T>? left, Interval<T>? right)
        => !(left == right);
    public static bool operator >(Interval<T> left, Interval<T> right)
        => right.Max > left.Max;
    public static bool operator >=(Interval<T> left, Interval<T> right)
        => left > right || left == right;
    public static bool operator <(Interval<T> left, Interval<T> right)
        => left.Min < right.Min;
    public static bool operator <=(Interval<T> left, Interval<T> right)
        => left < right || left == right;

    //Utility methods
    public static Interval<T> Abs(Interval<T> value)
    {
        var min = T.Abs(value.Min);
        var max = T.Abs(value.Max);

        return value switch
        {
            { HasZero: true } => new(T.Zero, MathExtensions.Max(min, max)),
            _ => new(MathExtensions.Min(min, max), MathExtensions.Max(min, max))
        };
    }
    public static bool IsZero(Interval<T> value)
        => value == Interval<T>.Zero;
    public int CompareTo(Interval<T> other)
        => this < other ? -1
        : this > other ? 1
        : 0;
    public int CompareTo(object? obj)
        => obj switch
        {
            null => 1,
            Interval<T> right => this.CompareTo(right),
            T point => this.CompareTo(new(point)),
            _ => throw new ArgumentException($"Type {obj.GetType().Name} is not comparable to {typeof(Interval<T>)}")
        };

    //Parsing methods
    public string ToString(string? format, IFormatProvider? formatProvider)
        => $"[{Min.ToString(format, formatProvider)};{Max.ToString(format, formatProvider)}]";
    public static Interval<T> Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
    {
        throw new NotImplementedException();
    }


    //Constants
    static readonly T _TTwo = T.One + T.One;
    static readonly Interval<T> _Zero = new(T.Zero);
    static readonly Interval<T> _One = new(T.One);
    static readonly Regex _Template = new Regex("");//TODO define parsing regex for: [1;2] [0.111;3] [0;0,001] [1e-4;2] [0;3E12]
}
