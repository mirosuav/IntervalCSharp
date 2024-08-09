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
      : INumberBase<Interval<T>>,
        ISpanFormattable,
        ISpanParsable<Interval<T>>,
        IFormattable
    where T : struct
            , INumberBase<T>
            , IComparisonOperators<T, T, bool>
            , IFormattable
{
    /// <summary>
    /// Infimum
    /// </summary>
    public readonly T Min;

    /// <summary>
    /// Supremum
    /// </summary>
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

    public static int Radix => throw new NotImplementedException();

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

    public static Interval<T> Abs(Interval<T> value)
    {
        throw new NotImplementedException();
    }

    public static bool IsCanonical(Interval<T> value)
    {
        throw new NotImplementedException();
    }

    public static bool IsComplexNumber(Interval<T> value)
    {
        throw new NotImplementedException();
    }
    public static bool IsInteger(Interval<T> value)
        => value.IsPoint && T.IsInteger(value.Min);
    public static bool IsEvenInteger(Interval<T> value)
        => value.IsPoint && T.IsEvenInteger(value.Min);
    public static bool IsOddInteger(Interval<T> value)
        => value.IsPoint && T.IsOddInteger(value.Min);
    public static bool IsImaginaryNumber(Interval<T> value)
        => T.IsImaginaryNumber(value.Min) || T.IsImaginaryNumber(value.Max);
    public static bool IsNaN(Interval<T> value)
        => T.IsNaN(value.Min) || T.IsNaN(value.Max);
    public static bool IsPositive(Interval<T> value)
        => T.IsPositive(value.Min);
    public static bool IsNegative(Interval<T> value)
        => T.IsNegative(value.Max);
    public static bool IsFinite(Interval<T> value)
        => T.IsFinite(value.Min) && T.IsFinite(value.Max);
    public static bool IsInfinity(Interval<T> value)
        => T.IsInfinity(value.Min) || T.IsInfinity(value.Max);
    public static bool IsNegativeInfinity(Interval<T> value)
        => T.IsNegativeInfinity(value.Min);
    public static bool IsPositiveInfinity(Interval<T> value)
        => T.IsPositiveInfinity(value.Max);
    public static bool IsRealNumber(Interval<T> value)
        => T.IsRealNumber(value.Min) || T.IsRealNumber(value.Min);
    public static bool IsNormal(Interval<T> value)
        => T.IsNormal(value.Min) && T.IsNormal(value.Min);
    public static bool IsSubnormal(Interval<T> value)
        => T.IsSubnormal(value.Min) || T.IsSubnormal(value.Min);

    static bool INumberBase<Interval<T>>.IsZero(Interval<T> value)
    {
        throw new NotImplementedException();
    }

    public static Interval<T> MaxMagnitude(Interval<T> x, Interval<T> y)
    {
        throw new NotImplementedException();
    }

    public static Interval<T> MaxMagnitudeNumber(Interval<T> x, Interval<T> y)
    {
        throw new NotImplementedException();
    }

    public static Interval<T> MinMagnitude(Interval<T> x, Interval<T> y)
    {
        throw new NotImplementedException();
    }

    public static Interval<T> MinMagnitudeNumber(Interval<T> x, Interval<T> y)
    {
        throw new NotImplementedException();
    }

    public static Interval<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static Interval<T> Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
    {
        throw new NotImplementedException();
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static Interval<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
    {
        throw new NotImplementedException();
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
