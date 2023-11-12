using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using IntervalCSharp.Helpers;

namespace IntervalCSharp;

/// <summary>
/// Interval number defined by inclusive Min and Max bounds. Implements basic interval arithmetic operations. 
/// </summary>
/// <typeparam name="T">Numeric type of the interval boundaries.</typeparam>
public record struct Interval<T>
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
        IFormattable
    where T : struct 
            , INumberBase<T>
            , IComparisonOperators<T, T, bool>
            , IFormattable
{
    public readonly T Min;
    public readonly T Max;

    public Interval() : this(T.Zero) { }
    public Interval(T point) : this(point, point) { }
    public Interval(Interval<T> other) : this(other.Min, other.Max) { }
    public Interval(T min, T max)
    {
        if (max < min)
        {
            Min = max;
            Max = min;
        }
        else
        {
            Min = min;
            Max = max;
        }
    }


    public bool IsPoint => Min == Max;
    public bool IsZero => this == Zero;
    public bool HasZero => Min <= T.Zero && Max >= T.Zero;
    public T Width => Max - Min;
    public T Radius => (Max - Min) / _TTwo;
    public T Middle => (Min + Max) / _TTwo;
    public static Interval<T> Zero => _Zero;
    public static Interval<T> One => _One;
    public static Interval<T> AdditiveIdentity => Interval<T>.Zero;
    public static Interval<T> MultiplicativeIdentity => Interval<T>.One;

    //Basic arithmetic operators
    [Pure]
    public static Interval<T> operator +(Interval<T> left, Interval<T> right)
    {
        try
        {
            FPURounding.Down();
            T min = left.Min + right.Min;

            FPURounding.Up();
            T max = left.Max + right.Max;

            return new Interval<T>(min, max);
        }
        finally
        {
            FPURounding.Reset();
        }
    }
    [Pure]
    public static Interval<T> operator -(Interval<T> left, Interval<T> right)
    {
        try
        {
            FPURounding.Down();
            T min = left.Min - right.Max;

            FPURounding.Up();
            T max = left.Max - right.Min;

            return new Interval<T>(min, max);
        }
        finally
        {
            FPURounding.Reset();
        }
    }
    [Pure]
    public static Interval<T> operator *(Interval<T> left, Interval<T> right)
    {
        try
        {
            FPURounding.Down();
            T min = MathHelper.Min(left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max);

            FPURounding.Up();
            T max = MathHelper.Max(left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max);

            return new Interval<T>(min, max);
        }
        finally
        {
            FPURounding.Reset();
        }
    }
    [Pure]
    public static Interval<T> operator /(Interval<T> left, Interval<T> right)
    {
        if (right.HasZero)
            throw IntervalExceptions.DividingByIntervalContainingZero;
        try
        {
            FPURounding.Down();
            T min = MathHelper.Min(left.Min / right.Min, left.Min / right.Max, left.Max / right.Min, left.Max / right.Max);

            FPURounding.Up();
            T max = MathHelper.Max(left.Min / right.Min, left.Min / right.Max, left.Max / right.Min, left.Max / right.Max);

            return new Interval<T>(min, max);
        }
        finally
        {
            FPURounding.Reset();
        }
    }
    [Pure]
    public static Interval<T> operator --(Interval<T> value)
        => value - One;
    [Pure]
    public static Interval<T> operator ++(Interval<T> value)
        => value + One;
    [Pure]
    public static Interval<T> operator +(Interval<T> value)
        => value;
    [Pure]
    public static Interval<T> operator -(Interval<T> value)
        => new(-value.Max, -value.Min);

    //Comparable operators
    [Pure]
    public static bool operator ==(Interval<T>? left, Interval<T>? right)
       => left is null ? right is null
       : right is null ? false
       : left.Value.Min == right.Value.Min && left.Value.Max == right.Value.Max;
    [Pure]
    public static bool operator !=(Interval<T>? left, Interval<T>? right)
        => !(left == right);


    //Explicit conversion operators
    public static implicit operator Interval<T>?(T? num)
        => num is null ? null : new(num.Value);
    public static implicit operator Interval<T>(T num)
        => new(num);
    public static implicit operator Interval<T>?(Tuple<T, T> num)
        => num is null ? null : new(num.Item1, num.Item2);

    //Parsing methods
    [Pure]
    public override string ToString() 
        => $"{OpeningBracket}{Min}{Separator}{Max}{ClosingBracket}";
    [Pure]
    public string ToString(string? format, IFormatProvider? formatProvider)
        => $"{OpeningBracket}{Min.ToString(format, formatProvider)}{Separator}{Max.ToString(format, formatProvider)}{ClosingBracket}";

    public static Interval<T> Parse(string s)
        => Parse(s, NumberFormatInfo.CurrentInfo);
    public static Interval<T> Parse(string s, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw IntervalExceptions.IntervalStringParsingError;

        var rgx = _Template.Match(s);

        if (!rgx.Success || rgx.Groups.Count != 3)
            throw IntervalExceptions.IntervalStringParsingError;

        T min = T.Parse(rgx.Groups[1].ValueSpan, provider);
        T max = T.Parse(rgx.Groups[2].ValueSpan, provider);

        return new Interval<T>(min, max);
    }
    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out Interval<T> result)
        => TryParse(s, NumberFormatInfo.CurrentInfo, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var rgx = _Template.Match(s);

        if (!rgx.Success || rgx.Groups.Count != 3)
            return false;

        if (!T.TryParse(rgx.Groups[1].ValueSpan, provider, out T min))
            return false;

        if (!T.TryParse(rgx.Groups[2].ValueSpan, provider, out T max))
            return false;

        result = new Interval<T>(min, max);

        return true;
    }


    //Constants
    internal const char Separator = ';';
    internal const char OpeningBracket = '[';
    internal const char ClosingBracket = ']';
    internal static readonly T _TTwo = T.One + T.One;
    internal static readonly Interval<T> _Zero = new(T.Zero);
    internal static readonly Interval<T> _One = new(T.One);
    internal static readonly Regex _Template 
        = new Regex($@"^\{OpeningBracket}([0-9eE.,'\-\+ ]+)\{Separator}([0-9eE.,'\-\+ ]+)\{ClosingBracket}$");
}
