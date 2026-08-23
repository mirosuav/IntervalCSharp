using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;
using IntervalCSharp.Helpers;

namespace IntervalCSharp;

/// <summary>
/// Interval number defined by inclusive Min and Max bounds. Implements basic interval arithmetic operations.
/// </summary>
/// <remarks>
/// Every arithmetic operator computes its lower bound with the FPU rounding towards negative infinity
/// and its upper bound with the FPU rounding towards positive infinity, so the mathematically exact
/// result is always enclosed. See <see cref="FpuRounding"/> for the platform support matrix and for
/// why such a scope must never contain an <c>await</c>.
/// </remarks>
/// <typeparam name="T">
/// IEEE-754 binary floating-point type of the interval boundaries (<see cref="float"/>,
/// <see cref="double"/>, <see cref="Half"/>). Directed rounding is meaningless for exact or
/// software-rounded types such as <see cref="decimal"/>, hence the constraint.
/// </typeparam>
public readonly record struct Interval<T>
      : INumberBase<Interval<T>>
    where T : struct
            , IFloatingPointIeee754<T>
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

    /// <remarks>
    /// Halved before subtracting, not after: <c>(Max - Min) / 2</c> overflows to infinity for a
    /// wide interval such as [-1e308; 1e308] whose radius, 1e308, is perfectly representable.
    /// Halving is exact for binary floating point, so this costs no accuracy.
    /// </remarks>
    public T Radius => Max / _TTwo - Min / _TTwo;

    /// <remarks>
    /// Halved before adding, for the same reason as <see cref="Radius"/>: <c>(Min + Max) / 2</c>
    /// overflows for [1e308; 1.5e308], whose middle, 1.25e308, is representable.
    /// </remarks>
    public T Middle => Min / _TTwo + Max / _TTwo;

    /// <summary>
    /// Magnitude: the largest absolute value attained on the interval, mag(X) = max{|x| : x in X}.
    /// </summary>
    public T Magnitude => T.Max(T.Abs(Min), T.Abs(Max));

    /// <summary>
    /// Mignitude: the smallest absolute value attained on the interval, mig(X) = min{|x| : x in X}.
    /// Zero whenever the interval straddles zero.
    /// </summary>
    public T Mignitude => HasZero ? T.Zero : T.Min(T.Abs(Min), T.Abs(Max));

    public static Interval<T> Zero => _Zero;
    public static Interval<T> One => _One;
    public static Interval<T> AdditiveIdentity => Interval<T>.Zero;
    public static Interval<T> MultiplicativeIdentity => Interval<T>.One;

    public static int Radix => T.Radix;

    //Basic arithmetic operators
    [Pure]
    public static Interval<T> operator +(Interval<T> left, Interval<T> right)
    {
        try
        {
            FpuRounding.Down();
            T min = left.Min + right.Min;

            FpuRounding.Up();
            T max = left.Max + right.Max;

            return new Interval<T>(min, max);
        }
        finally
        {
            FpuRounding.Reset();
        }
    }
    [Pure]
    public static Interval<T> operator -(Interval<T> left, Interval<T> right)
    {
        try
        {
            FpuRounding.Down();
            T min = left.Min - right.Max;

            FpuRounding.Up();
            T max = left.Max - right.Min;

            return new Interval<T>(min, max);
        }
        finally
        {
            FpuRounding.Reset();
        }
    }
    [Pure]
    public static Interval<T> operator *(Interval<T> left, Interval<T> right)
    {
        try
        {
            FpuRounding.Down();
            T min = MathHelper.Min(left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max);

            FpuRounding.Up();
            T max = MathHelper.Max(left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max);

            return new Interval<T>(min, max);
        }
        finally
        {
            FpuRounding.Reset();
        }
    }
    [Pure]
    public static Interval<T> operator /(Interval<T> left, Interval<T> right)
    {
        if (right.HasZero)
            throw IntervalExceptions.DividingByIntervalContainingZero;
        try
        {
            FpuRounding.Down();
            T min = MathHelper.Min(left.Min / right.Min, left.Min / right.Max, left.Max / right.Min, left.Max / right.Max);

            FpuRounding.Up();
            T max = MathHelper.Max(left.Min / right.Min, left.Min / right.Max, left.Max / right.Min, left.Max / right.Max);

            return new Interval<T>(min, max);
        }
        finally
        {
            FpuRounding.Reset();
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

    //Formatting methods
    [Pure]
    public override string ToString()
        => ToString(null, null);
    [Pure]
    public string ToString(string? format, IFormatProvider? formatProvider)
        => $"{OpeningBracket}{Min.ToString(format, formatProvider)}{Separator}{Max.ToString(format, formatProvider)}{ClosingBracket}";

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;
        if (TryWrite(destination, ref charsWritten, OpeningBracket)
            && TryWrite(destination, ref charsWritten, Min, format, provider)
            && TryWrite(destination, ref charsWritten, Separator)
            && TryWrite(destination, ref charsWritten, Max, format, provider)
            && TryWrite(destination, ref charsWritten, ClosingBracket))
            return true;

        charsWritten = 0;
        return false;
    }

    private static bool TryWrite(Span<char> destination, ref int written, char value)
    {
        if (written >= destination.Length)
            return false;

        destination[written++] = value;
        return true;
    }

    private static bool TryWrite(Span<char> destination, ref int written, T value, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (!value.TryFormat(destination[written..], out int bounds, format, provider))
            return false;

        written += bounds;
        return true;
    }

    //Parsing methods - every overload funnels into the span+NumberStyles TryParse below.
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
    {
        result = Zero;
        s = s.Trim();

        if (s.Length < 3 || s[0] != OpeningBracket || s[^1] != ClosingBracket)
            return false;

        ReadOnlySpan<char> bounds = s[1..^1];
        int separator = bounds.IndexOf(Separator);

        //Exactly one separator, so "[1;0;1]" and "[1.0]" are both rejected.
        if (separator < 0 || bounds.LastIndexOf(Separator) != separator)
            return false;

        if (!T.TryParse(bounds[..separator], style, provider, out T min)
            || !T.TryParse(bounds[(separator + 1)..], style, provider, out T max))
            return false;

        result = new Interval<T>(min, max);
        return true;
    }
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
        => TryParse(s, DefaultStyle, provider, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
        => TryParse(s.AsSpan(), style, provider, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Interval<T> result)
        => TryParse(s.AsSpan(), DefaultStyle, provider, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out Interval<T> result)
        => TryParse(s, NumberFormatInfo.CurrentInfo, out result);

    public static Interval<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
        => TryParse(s, style, provider, out Interval<T> result)
         ? result
         : throw IntervalExceptions.IntervalStringParsingError;
    public static Interval<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => Parse(s, DefaultStyle, provider);
    public static Interval<T> Parse(string s, NumberStyles style, IFormatProvider? provider)
        => Parse(s.AsSpan(), style, provider);
    public static Interval<T> Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), DefaultStyle, provider);
    public static Interval<T> Parse(string s)
        => Parse(s, NumberFormatInfo.CurrentInfo);

    /// <summary>
    /// Distance between two intervals, q([x],[y]) = max{|x_inf - y_inf|, |x_sup - y_sup|}
    /// (thesis eq. 14). It is a metric on the set of closed real intervals.
    /// </summary>
    [Pure]
    public static T Distance(Interval<T> x, Interval<T> y)
        => T.Max(T.Abs(x.Min - y.Min), T.Abs(x.Max - y.Max));

    /// <summary>
    /// Absolute value of the whole interval: Abs(X) = [mig(X), mag(X)] = {|x| : x in X}.
    /// </summary>
    public static Interval<T> Abs(Interval<T> value)
        => new(value.Mignitude, value.Magnitude);

    public static bool IsCanonical(Interval<T> value)
        => T.IsCanonical(value.Min) && T.IsCanonical(value.Max);

    public static bool IsComplexNumber(Interval<T> value)
        => T.IsComplexNumber(value.Min) || T.IsComplexNumber(value.Max);
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
        => T.IsRealNumber(value.Min) && T.IsRealNumber(value.Max);
    public static bool IsNormal(Interval<T> value)
        => T.IsNormal(value.Min) && T.IsNormal(value.Max);
    public static bool IsSubnormal(Interval<T> value)
        => T.IsSubnormal(value.Min) || T.IsSubnormal(value.Max);

    static bool INumberBase<Interval<T>>.IsZero(Interval<T> value)
        => value.IsZero;

    /// <summary>
    /// The interval with the greater <see cref="Magnitude"/>, propagating NaN as IEEE maxMagnitude does.
    /// </summary>
    public static Interval<T> MaxMagnitude(Interval<T> x, Interval<T> y)
        => IsNaN(x) ? x : IsNaN(y) ? y : MaxMagnitudeNumber(x, y);

    /// <inheritdoc cref="MaxMagnitude"/>
    /// <remarks>Unlike <see cref="MaxMagnitude"/>, a NaN operand loses to a number.</remarks>
    public static Interval<T> MaxMagnitudeNumber(Interval<T> x, Interval<T> y)
    {
        if (IsNaN(x)) return y;
        if (IsNaN(y)) return x;

        T magX = x.Magnitude, magY = y.Magnitude;
        return magX > magY ? x
             : magY > magX ? y
             : IsGreater(x, y) ? x : y;
    }

    /// <summary>
    /// The interval with the smaller <see cref="Mignitude"/>, propagating NaN as IEEE minMagnitude does.
    /// </summary>
    public static Interval<T> MinMagnitude(Interval<T> x, Interval<T> y)
        => IsNaN(x) ? x : IsNaN(y) ? y : MinMagnitudeNumber(x, y);

    /// <inheritdoc cref="MinMagnitude"/>
    /// <remarks>Unlike <see cref="MinMagnitude"/>, a NaN operand loses to a number.</remarks>
    public static Interval<T> MinMagnitudeNumber(Interval<T> x, Interval<T> y)
    {
        if (IsNaN(x)) return y;
        if (IsNaN(y)) return x;

        T migX = x.Mignitude, migY = y.Mignitude;
        return migX < migY ? x
             : migY < migX ? y
             : IsGreater(x, y) ? y : x;
    }

    /// <summary>Lexicographic order on the bounds, used only to break Min/MaxMagnitude ties deterministically.</summary>
    private static bool IsGreater(Interval<T> left, Interval<T> right)
        => left.Min > right.Min || (left.Min == right.Min && left.Max > right.Max);

    //Generic-math conversions. An Interval converts to a scalar only when it is a point interval;
    //anything else has no single value to convert to and is reported as unsupported.
    static bool INumberBase<Interval<T>>.TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out Interval<T> result)
        => TryConvertFrom(T.CreateChecked, value, out result);
    static bool INumberBase<Interval<T>>.TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out Interval<T> result)
        => TryConvertFrom(T.CreateSaturating, value, out result);
    static bool INumberBase<Interval<T>>.TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out Interval<T> result)
        => TryConvertFrom(T.CreateTruncating, value, out result);

    static bool INumberBase<Interval<T>>.TryConvertToChecked<TOther>(Interval<T> value, [MaybeNullWhen(false)] out TOther result)
        => TryConvertTo(TOther.CreateChecked, value, out result);
    static bool INumberBase<Interval<T>>.TryConvertToSaturating<TOther>(Interval<T> value, [MaybeNullWhen(false)] out TOther result)
        => TryConvertTo(TOther.CreateSaturating, value, out result);
    static bool INumberBase<Interval<T>>.TryConvertToTruncating<TOther>(Interval<T> value, [MaybeNullWhen(false)] out TOther result)
        => TryConvertTo(TOther.CreateTruncating, value, out result);

    private static bool TryConvertFrom<TOther>(Func<TOther, T> create, TOther value, [MaybeNullWhen(false)] out Interval<T> result)
            where TOther : INumberBase<TOther>
    {
        try
        {
            result = new Interval<T>(create(value));
            return true;
        }
        catch (NotSupportedException) //TOther has no conversion to T - the contract asks for false, not a throw.
        {
            result = default;
            return false;
        }
    }

    private static bool TryConvertTo<TOther>(Func<T, TOther> create, Interval<T> value, [MaybeNullWhen(false)] out TOther result)
            where TOther : INumberBase<TOther>
    {
        result = default;

        if (!value.IsPoint)
            return false;

        try
        {
            result = create(value.Min);
            return true;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }


    //Constants
    internal const char Separator = ';';
    internal const char OpeningBracket = '[';
    internal const char ClosingBracket = ']';
    /// <summary>Style used by the overloads that don't take one - the same default the IEEE-754 primitives use.</summary>
    internal const NumberStyles DefaultStyle = NumberStyles.Float | NumberStyles.AllowThousands;
    internal static readonly T _TTwo = T.One + T.One;
    internal static readonly Interval<T> _Zero = new(T.Zero);
    internal static readonly Interval<T> _One = new(T.One);
}
