using System.Numerics;
using IntervalCSharp.Helpers;

namespace IntervalCSharp;

/// <summary>
/// Interval extensions of the real elementary functions, per thesis section 3.4.
/// </summary>
/// <remarks>
/// <para>
/// IEEE-754 mandates correctly rounded results <b>in every rounding mode</b> for <c>+ - * /</c> and
/// for square root, and for nothing else. So the FPU rounding mode bounds the result of the
/// arithmetic operators, of <see cref="Pow"/> (multiplication only) and of <see cref="Sqrt"/> - and
/// those three are computed with directed rounding, tight and rigorous.
/// </para>
/// <para>
/// It does <b>not</b> bound anything the platform math library computes. Measured on this runtime,
/// <c>Sin</c> evaluated under round-towards-negative-infinity returns values up to 1.18 million ulp
/// <i>above</i> the correctly rounded result, because the internal argument reduction is poisoned by
/// the rounding mode. The C++ original sets the mode and calls <c>sinl</c> anyway, which does not
/// produce a bound at all. Everything else here is therefore evaluated in round-to-nearest - the
/// mode the library is written for - and then widened outward by one ulp, which is sound as long as
/// the platform is faithfully rounded to within one ulp.
/// </para>
/// </remarks>
public static class Functions
{
    /// <summary>
    /// Natural power of an interval, implemented as:
    /// <code>
    /// [x]^n = [min^n, max^n]        when (n odd)  or (min >= 0)
    ///       = [max^n, min^n]        when (n even) and (max &lt;= 0)
    ///       = [0, abs([x])^n]       when (n even) and (0 in [x])
    /// </code>
    /// Negative exponents fall back on <c>[1,1] / [x]^(-n)</c>.
    /// </summary>
    /// <remarks>
    /// The exponent is an <see cref="int"/> because the thesis defines the interval power for
    /// n in N only, and because <see cref="Power"/> reaches the result through multiplication
    /// alone - see the note there.
    /// </remarks>
    public static Interval<T> Pow<T>(this Interval<T> value, int n)
            where T : struct, IFloatingPointIeee754<T>
    {
        if (n < 0)
            return Interval<T>.One / value.Pow(checked(-n));

        T lower, upper;

        try
        {
            if (!int.IsEvenInteger(n) || value.Min >= T.Zero)
            {
                FpuRounding.Down();
                lower = Power(value.Min, n);
                FpuRounding.Up();
                upper = Power(value.Max, n);
            }
            else if (value.Max <= T.Zero)
            {
                FpuRounding.Down();
                lower = Power(value.Max, n);
                FpuRounding.Up();
                upper = Power(value.Min, n);
            }
            else //n is even and the interval straddles zero
            {
                lower = T.Zero;
                FpuRounding.Up();
                upper = Power(value.Magnitude, n);
            }
        }
        finally
        {
            FpuRounding.Reset();
        }

        return new Interval<T>(lower, upper);
    }

    /// <summary>
    /// x^n by divide-and-conquer repeated squaring - the thesis' <c>adon</c>.
    /// </summary>
    /// <remarks>
    /// Multiplication is the only operation involved, so the active FPU rounding mode governs
    /// every step. <c>T.Pow</c> cannot be used here: it is evaluated as exp(n*log x) and its
    /// internal error is not bounded by the rounding mode, so it can return a value on the
    /// wrong side of the exact result and break the enclosure.
    /// </remarks>
    private static T Power<T>(T x, int n)
            where T : struct, IFloatingPointIeee754<T>
        => n < 1 ? T.One
         : n == 1 ? x
         : n == 2 ? x * x
         : int.IsEvenInteger(n) ? Sqr(Power(x, n / 2))
         : Sqr(Power(x, (n - 1) / 2)) * x;

    private static T Sqr<T>(T a) where T : struct, IFloatingPointIeee754<T> => a * a;

    /// <summary>
    /// Square root, thesis 3.4: <c>sqrt([x]) = [sqrt(min), sqrt(max)]</c>, defined for min >= 0.
    /// </summary>
    /// <remarks>
    /// The one elementary function IEEE-754 requires to be correctly rounded, so unlike its
    /// neighbours it is computed with directed rounding: rigorous and exactly tight.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The interval reaches below zero.</exception>
    public static Interval<T> Sqrt<T>(this Interval<T> value)
            where T : struct, IFloatingPointIeee754<T>
    {
        if (value.Min < T.Zero)
            throw IntervalExceptions.SquareRootOfNegativeInterval;

        try
        {
            FpuRounding.Down();
            T lower = T.Sqrt(value.Min);

            FpuRounding.Up();
            T upper = T.Sqrt(value.Max);

            return new Interval<T>(lower, upper);
        }
        finally
        {
            FpuRounding.Reset();
        }
    }

    /// <summary>
    /// Exponential, thesis 3.4: <c>e^[x] = [e^min, e^max]</c>.
    /// </summary>
    public static Interval<T> Exp<T>(this Interval<T> value)
            where T : struct, IFloatingPointIeee754<T>
        => Increasing(value, T.Exp);

    /// <summary>
    /// Natural logarithm, thesis 3.4: one of the functions increasing on its domain, so
    /// <c>ln([x]) = [ln(min), ln(max)]</c>.
    /// </summary>
    /// <remarks>
    /// The domain check is an addition - the C++ <c>ilog</c> has none and quietly returns NaN
    /// bounds for a negative interval, which then poisons every later operation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The interval reaches below zero.</exception>
    public static Interval<T> Log<T>(this Interval<T> value)
            where T : struct, IFloatingPointIeee754<T>
        => value.Min < T.Zero
         ? throw IntervalExceptions.LogarithmOfNegativeInterval
         : Increasing(value, T.Log);

    /// <summary>
    /// Tangent, thesis 3.4: increasing between its poles, so <c>tg([x]) = [tg(min), tg(max)]</c>.
    /// </summary>
    /// <remarks>
    /// The pole check is an addition - neither the thesis nor the C++ <c>itan</c> has one, and
    /// without it tan([1;2]) silently returns [-2.18; 1.56] while the true range is all of R.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The interval contains a pole.</exception>
    public static Interval<T> Tan<T>(this Interval<T> value)
            where T : struct, IFloatingPointIeee754<T>
        //The poles sit at (k + 1/2)*PI, which are exactly the boundaries between the half-periods
        //that HalfPeriod counts, so the interval is safe iff both ends land in the same one.
        => HalfPeriod(value.Min) != HalfPeriod(value.Max)
         ? throw IntervalExceptions.TangentOfIntervalContainingPole
         : Increasing(value, T.Tan);

    /// <summary>
    /// Cotangent, as <c>[1,1] / tg([x])</c> - the C++ original's <c>ictan</c>.
    /// </summary>
    /// <remarks>
    /// The division does the domain checking for free: it throws exactly when the tangent
    /// interval contains zero, and the zeros of tangent are the poles of cotangent.
    /// </remarks>
    public static Interval<T> Cot<T>(this Interval<T> value)
            where T : struct, IFloatingPointIeee754<T>
        => Interval<T>.One / value.Tan();

    /// <summary>
    /// Sine. Not one of the thesis' monotone functions, so this follows the C++ <c>isin</c>:
    /// split the line into half-periods of width PI, numbered so that part 0 is [-PI/2, PI/2].
    /// Sine increases on even parts and decreases on odd ones.
    /// </summary>
    public static Interval<T> Sin<T>(this Interval<T> value)
            where T : struct, IFloatingPointIeee754<T>
    {
        //Plain interval arithmetic, so this part really is rigorously rounded. Using an interval
        //for PI rather than the point double keeps a borderline argument on the conservative side:
        //a wider parts interval can only widen the result, never narrow it below the true range.
        Interval<T> parts = value / Pi<T>() + new Interval<T>(Half<T>());
        T first = T.Floor(parts.Min);
        T last = T.Floor(parts.Max);

        //A whole part lies in between, so both extremes are reached.
        if (last - first > T.One)
            return new Interval<T>(-T.One, T.One);

        //Consecutive parts: the single extreme on the boundary between them is reached.
        if (last == first + T.One)
            return AcrossExtreme(value, maximum: T.IsEvenInteger(first));

        return T.IsEvenInteger(first)
             ? Increasing(value, T.Sin)
             : Decreasing(value, T.Sin);
    }

    /// <summary>
    /// Cosine, as <c>sin(PI/2 - [x])</c> - the C++ original's <c>icos</c>.
    /// </summary>
    public static Interval<T> Cos<T>(this Interval<T> value)
            where T : struct, IFloatingPointIeee754<T>
        => (HalfPi<T>() - value).Sin();

    /// <summary>
    /// Sine over an interval crossing exactly one of its extremes: that bound is exactly +1 or
    /// -1, and the opposite one comes from whichever end of the interval is further from it.
    /// </summary>
    private static Interval<T> AcrossExtreme<T>(Interval<T> value, bool maximum)
            where T : struct, IFloatingPointIeee754<T>
    {
        T atMin, atMax;

        try
        {
            FpuRounding.Near();
            atMin = T.Sin(value.Min);
            atMax = T.Sin(value.Max);
        }
        finally
        {
            FpuRounding.Reset();
        }

        return maximum
             ? new Interval<T>(T.BitDecrement(T.Min(atMin, atMax)), T.One)
             : new Interval<T>(-T.One, T.BitIncrement(T.Max(atMin, atMax)));
    }

    /// <summary>
    /// <c>f([x]) = [f(min), f(max)]</c> - the thesis' rule for an f increasing on [x].
    /// </summary>
    private static Interval<T> Increasing<T>(Interval<T> value, Func<T, T> f)
            where T : struct, IFloatingPointIeee754<T>
        => Monotone(value.Min, value.Max, f);

    /// <summary>
    /// <c>f([x]) = [f(max), f(min)]</c> - the thesis' rule for an f decreasing on [x].
    /// </summary>
    private static Interval<T> Decreasing<T>(Interval<T> value, Func<T, T> f)
            where T : struct, IFloatingPointIeee754<T>
        => Monotone(value.Max, value.Min, f);

    /// <summary>
    /// Evaluates f at the two ends in round-to-nearest and widens the result outward by one ulp.
    /// See the class remarks for why the FPU rounding mode is useless for these functions.
    /// </summary>
    private static Interval<T> Monotone<T>(T at, T to, Func<T, T> f)
            where T : struct, IFloatingPointIeee754<T>
    {
        T lower, upper;

        try
        {
            FpuRounding.Near();
            lower = f(at);
            upper = f(to);
        }
        finally
        {
            FpuRounding.Reset();
        }

        return new Interval<T>(T.BitDecrement(lower), T.BitIncrement(upper));
    }

    /// <summary>
    /// Index of the half-period of width PI that x falls into, counting part 0 as [-PI/2, PI/2].
    /// </summary>
    private static T HalfPeriod<T>(T x) where T : struct, IFloatingPointIeee754<T>
        => T.Floor(x / T.Pi + Half<T>());

    /// <summary>PI as an interval enclosing the real constant, which no double equals.</summary>
    private static Interval<T> Pi<T>() where T : struct, IFloatingPointIeee754<T>
        => new(T.BitDecrement(T.Pi), T.BitIncrement(T.Pi));

    /// <summary>PI/2 likewise - halving is exact, so it stays an enclosure.</summary>
    private static Interval<T> HalfPi<T>() where T : struct, IFloatingPointIeee754<T>
        => new(T.BitDecrement(T.Pi) / Interval<T>._TTwo, T.BitIncrement(T.Pi) / Interval<T>._TTwo);

    /// <summary>One half - <see cref="INumberBase{TSelf}"/> has no constant for it.</summary>
    private static T Half<T>() where T : struct, IFloatingPointIeee754<T>
        => T.One / Interval<T>._TTwo;
}
