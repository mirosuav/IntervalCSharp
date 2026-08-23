using FluentAssertions;
using System.Globalization;
using System.Numerics;
using Xunit;

namespace IntervalCSharp.Tests;
using Interval = Interval<double>;

/// <summary>
/// Covers the members of <see cref="INumberBase{TSelf}"/> that used to throw NotImplementedException.
/// </summary>
public class IntervalNumberBaseTests
{
    public static IFormatProvider formatUS = CultureInfo.GetCultureInfo("en-US");

    private static Interval Parse(string s) => Interval.Parse(s, formatUS);

    //CreateChecked and friends are static virtual members with a default body, so they are only
    //reachable through a generic type parameter - never off the closed type directly.
    private static TSelf CreateChecked<TSelf, TOther>(TOther value)
            where TSelf : INumberBase<TSelf>
            where TOther : INumberBase<TOther>
        => TSelf.CreateChecked(value);

    private static bool IsZero<TSelf>(TSelf value) where TSelf : INumberBase<TSelf>
        => TSelf.IsZero(value);

    [Fact]
    public void Radix_IsBinary()
        => Interval.Radix.Should().Be(2);

    [Theory]
    [InlineData("[-3;1]", 3.0, 0.0)]
    [InlineData("[1;3]", 3.0, 1.0)]
    [InlineData("[-3;-1]", 3.0, 1.0)]
    [InlineData("[0;0]", 0.0, 0.0)]
    public void MagnitudeAndMignitude(string value, double magnitude, double mignitude)
    {
        //ACT
        var sut = Parse(value);

        //ASSERT
        sut.Magnitude.Should().Be(magnitude);
        sut.Mignitude.Should().Be(mignitude);
    }

    [Theory]
    [InlineData("[-3;1]", "[0;2]", "[-3;1]")]   // mag 3 beats mag 2
    [InlineData("[0;2]", "[-3;1]", "[-3;1]")]   // argument order does not matter
    [InlineData("[0;1]", "[-1;0]", "[0;1]")]    // equal mag - tie broken towards the greater bounds
    public void MaxMagnitude_PicksLargerMagnitude(string x, string y, string expected)
        => Interval.MaxMagnitude(Parse(x), Parse(y)).Should().Be(Parse(expected));

    [Theory]
    [InlineData("[-3;1]", "[2;4]", "[-3;1]")]   // mig 0 (straddles zero) beats mig 2
    [InlineData("[2;4]", "[-3;1]", "[-3;1]")]
    [InlineData("[0;1]", "[-1;0]", "[-1;0]")]   // equal mig - tie broken towards the lesser bounds
    public void MinMagnitude_PicksSmallerMignitude(string x, string y, string expected)
        => Interval.MinMagnitude(Parse(x), Parse(y)).Should().Be(Parse(expected));

    [Fact]
    public void MinMaxMagnitude_PropagateNaN_ButTheNumberOverloadsDoNot()
    {
        //ARRANGE
        var number = Parse("[1;2]");
        var nan = new Interval(double.NaN);

        //ASSERT
        Interval.IsNaN(Interval.MaxMagnitude(number, nan)).Should().BeTrue();
        Interval.IsNaN(Interval.MinMagnitude(number, nan)).Should().BeTrue();

        Interval.MaxMagnitudeNumber(number, nan).Should().Be(number);
        Interval.MinMagnitudeNumber(number, nan).Should().Be(number);
    }

    [Theory]
    [InlineData("[1;2]", true)]
    [InlineData("[-1.5;1e308]", true)]
    public void IsCanonical(string value, bool expected)
        => Interval.IsCanonical(Parse(value)).Should().Be(expected);

    [Fact]
    public void IsComplexNumber_IsAlwaysFalseForRealBounds()
        => Interval.IsComplexNumber(Parse("[-1;1]")).Should().BeFalse();

    [Theory]
    [InlineData("[0;0]", true)]
    [InlineData("[0;1]", false)]
    public void IsZero_ThroughTheInterface(string value, bool expected)
        => IsZero(Parse(value)).Should().Be(expected);

    [Theory]
    [InlineData("[1.0;2.0]", 1.0, 2.0)]
    [InlineData("[ -1e18 ; 1e18 ]", -1e18, 1e18)]
    public void Parse_FromSpan(string s, double min, double max)
    {
        //ACT
        var sut = Interval.Parse(s.AsSpan(), formatUS);

        //ASSERT
        sut.Min.Should().Be(min);
        sut.Max.Should().Be(max);
    }

    [Fact]
    public void Parse_HonoursNumberStyles()
    {
        //ARRANGE
        const string thousands = "[1,000;2,000]";

        //ACT + ASSERT
        Interval.Parse(thousands, NumberStyles.Float | NumberStyles.AllowThousands, formatUS)
            .Should().Be(new Interval(1000.0, 2000.0));

        Interval.TryParse(thousands, NumberStyles.Float, formatUS, out _)
            .Should().BeFalse("the group separator is not allowed by NumberStyles.Float alone");
    }

    [Theory]
    [InlineData("[1;2]")]
    [InlineData("[-1.5;1e18]")]
    public void TryFormat_MatchesToString(string value)
    {
        //ARRANGE
        var sut = Parse(value);
        var expected = sut.ToString(null, formatUS);
        Span<char> destination = stackalloc char[expected.Length];

        //ACT
        var formatted = sut.TryFormat(destination, out int charsWritten, default, formatUS);

        //ASSERT
        formatted.Should().BeTrue();
        charsWritten.Should().Be(expected.Length);
        destination.ToString().Should().Be(expected);
    }

    [Fact]
    public void TryFormat_DestinationTooSmall_WritesNothing()
    {
        //ARRANGE
        Span<char> destination = stackalloc char[2];

        //ACT
        var formatted = Parse("[1;2]").TryFormat(destination, out int charsWritten, default, formatUS);

        //ASSERT
        formatted.Should().BeFalse();
        charsWritten.Should().Be(0);
    }

    [Fact]
    public void CreateChecked_FromScalar_MakesPointInterval()
        => CreateChecked<Interval, double>(2.5).Should().Be(new Interval(2.5));

    [Fact]
    public void CreateChecked_ToScalar_RequiresPointInterval()
    {
        //ACT + ASSERT
        CreateChecked<double, Interval>(new Interval(2.5)).Should().Be(2.5);

        ((Action)(() => CreateChecked<double, Interval>(Parse("[1;2]"))))
            .Should().Throw<NotSupportedException>("a proper interval has no single value to convert to");
    }
}
