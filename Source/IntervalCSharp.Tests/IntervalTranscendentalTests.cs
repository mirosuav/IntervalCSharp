using FluentAssertions;
using System.Globalization;
using Xunit;

namespace IntervalCSharp.Tests;
using Interval = Interval<double>;

/// <summary>
/// The elementary functions of thesis section 3.4. The main assertion is enclosure - the result
/// must contain f(x) for every x in the argument - since that is the property the whole library
/// exists to provide; exact bounds are only pinned down where they are exactly representable.
/// </summary>
public class IntervalTranscendentalTests
{
    public static IFormatProvider formatUS = CultureInfo.GetCultureInfo("en-US");

    private static Interval Parse(string s) => Interval.Parse(s, formatUS);

    private static void AssertEncloses(Interval result, Interval domain, Func<double, double> f, int samples = 401)
    {
        for (int i = 0; i <= samples; i++)
        {
            double x = i == 0 ? domain.Min
                     : i == samples ? domain.Max
                     : domain.Min + (domain.Max - domain.Min) * ((double)i / samples);

            double y = f(x);

            y.Should().BeGreaterThanOrEqualTo(result.Min, "f({0}) must be enclosed by {1}", x, result);
            y.Should().BeLessThanOrEqualTo(result.Max, "f({0}) must be enclosed by {1}", x, result);
        }
    }

    //=== Sqrt ===================================================================================

    [Theory]
    [InlineData("[4;9]")]
    [InlineData("[0;1]")]
    [InlineData("[1e-8;1e8]")]
    public void Sqrt_Encloses(string s)
    {
        var domain = Parse(s);
        AssertEncloses(domain.Sqrt(), domain, Math.Sqrt);
    }

    [Fact]
    public void Sqrt_OfExactSquares_IsExact()
        => Parse("[4;9]").Sqrt().Should().Be(new Interval(2.0, 3.0));

    [Fact]
    public void Sqrt_ReachingBelowZero_Throws()
        => ((Action)(() => Parse("[-1;4]").Sqrt())).Should().Throw<ArgumentOutOfRangeException>();

    //=== Exp / Log ==============================================================================

    [Theory]
    [InlineData("[0;1]")]
    [InlineData("[-3;3]")]
    [InlineData("[-20;0]")]
    public void Exp_Encloses(string s)
    {
        var domain = Parse(s);
        AssertEncloses(domain.Exp(), domain, Math.Exp);
    }

    [Fact]
    public void Exp_OfZero_EnclosesOneToWithinOneUlp()
        //Not exactly [1;1]: everything the platform math library computes is widened outward
        //by one ulp, because the FPU rounding mode does not bound its error. See Functions.
        => Parse("[0;0]").Exp()
            .Should().Be(new Interval(Math.BitDecrement(1.0), Math.BitIncrement(1.0)));

    [Theory]
    [InlineData("[1;10]")]
    [InlineData("[0.5;2]")]
    [InlineData("[1e-8;1e8]")]
    public void Log_Encloses(string s)
    {
        var domain = Parse(s);
        AssertEncloses(domain.Log(), domain, Math.Log);
    }

    [Fact]
    public void Log_OfOne_EnclosesZeroToWithinOneUlp()
        => Parse("[1;1]").Log()
            .Should().Be(new Interval(Math.BitDecrement(0.0), Math.BitIncrement(0.0)));

    [Fact]
    public void Log_ReachingBelowZero_Throws()
        => ((Action)(() => Parse("[-1;1]").Log())).Should().Throw<ArgumentOutOfRangeException>();

    //=== Tan / Cot ==============================================================================

    [Theory]
    [InlineData("[0;1]")]
    [InlineData("[-1;1]")]
    [InlineData("[2;3]")]      // the part between PI/2 and 3PI/2
    public void Tan_Encloses(string s)
    {
        var domain = Parse(s);
        AssertEncloses(domain.Tan(), domain, Math.Tan);
    }

    [Theory]
    [InlineData("[1;2]")]      // crosses PI/2
    [InlineData("[0;4]")]
    [InlineData("[-2;2]")]
    public void Tan_AcrossPole_Throws(string s)
        => ((Action)(() => Parse(s).Tan())).Should().Throw<ArgumentOutOfRangeException>();

    [Theory]
    [InlineData("[0.5;1]")]
    [InlineData("[2;3]")]
    public void Cot_Encloses(string s)
    {
        var domain = Parse(s);
        AssertEncloses(domain.Cot(), domain, x => 1.0 / Math.Tan(x));
    }

    [Fact]
    public void Cot_AcrossItsOwnPole_Throws()
        //[-0.5;0.5] clears tangent's poles, but its tangent straddles zero - cotangent's pole.
        => ((Action)(() => Parse("[-0.5;0.5]").Cot())).Should().Throw<DivideByZeroException>();

    //=== Sin / Cos ==============================================================================

    [Theory]
    [InlineData("[0;0.5]")]    // increasing, one part
    [InlineData("[2;3]")]      // decreasing, one part
    [InlineData("[-7;-6]")]    // increasing, one part, negative
    [InlineData("[1;2]")]      // across a maximum
    [InlineData("[4;5]")]      // across a minimum
    [InlineData("[0;10]")]     // a whole part in between
    public void Sin_Encloses(string s)
    {
        var domain = Parse(s);
        AssertEncloses(domain.Sin(), domain, Math.Sin);
    }

    [Fact]
    public void Sin_OverAWholePart_IsTheFullRange()
        => Parse("[0;10]").Sin().Should().Be(new Interval(-1.0, 1.0));

    [Fact]
    public void Sin_AcrossMaximum_PinsTheUpperBoundExactly()
    {
        var sut = Parse("[1;2]").Sin();

        sut.Max.Should().Be(1.0);
        sut.Min.Should().BeLessThanOrEqualTo(Math.Sin(1.0));
    }

    [Fact]
    public void Sin_AcrossMinimum_PinsTheLowerBoundExactly()
    {
        var sut = Parse("[4;5]").Sin();

        sut.Min.Should().Be(-1.0);
        sut.Max.Should().BeGreaterThanOrEqualTo(Math.Sin(4.0));
    }

    [Theory]
    [InlineData("[0;1]")]
    [InlineData("[-1;1]")]
    [InlineData("[3;4]")]
    [InlineData("[0;10]")]
    public void Cos_Encloses(string s)
    {
        var domain = Parse(s);
        AssertEncloses(domain.Cos(), domain, Math.Cos);
    }

    [Fact]
    public void Cos_OverAWholePart_IsTheFullRange()
        => Parse("[0;10]").Cos().Should().Be(new Interval(-1.0, 1.0));
}
