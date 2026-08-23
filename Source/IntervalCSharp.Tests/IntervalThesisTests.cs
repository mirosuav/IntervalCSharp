using FluentAssertions;
using System.Globalization;
using Xunit;

namespace IntervalCSharp.Tests;
using Interval = Interval<double>;

/// <summary>
/// Checks the operations against the definitions in MasterThesis2007, section 3.2 and 3.4.
/// </summary>
public class IntervalThesisTests
{
    public static IFormatProvider formatUS = CultureInfo.GetCultureInfo("en-US");

    private static Interval Parse(string s) => Interval.Parse(s, formatUS);

    //=== 3.4: [x]^n =============================================================================

    [Theory]
    //n odd -> [inf^n, sup^n]
    [InlineData("[2;3]", 3, "[8;27]")]
    [InlineData("[-3;-2]", 3, "[-27;-8]")]
    [InlineData("[-2;3]", 3, "[-8;27]")]
    //n even and inf >= 0 -> [inf^n, sup^n]
    [InlineData("[2;3]", 2, "[4;9]")]
    [InlineData("[0;3]", 4, "[0;81]")]
    //n even and sup <= 0 -> [sup^n, inf^n]
    [InlineData("[-3;-2]", 2, "[4;9]")]
    [InlineData("[-3;0]", 2, "[0;9]")]
    //n even and 0 in [x] -> [0, abs([x])^n]
    [InlineData("[-3;2]", 2, "[0;9]")]
    [InlineData("[-2;3]", 2, "[0;9]")]
    [InlineData("[-1;1]", 4, "[0;1]")]
    //n == 0 is even, so an interval straddling zero takes the third case and widens to [0;1]
    //rather than the exact [1;1]. This reproduces the thesis case split and npoweri literally.
    [InlineData("[-3;2]", 0, "[0;1]")]
    [InlineData("[2;3]", 0, "[1;1]")]
    public void Pow_FollowsTheCaseSplit(string value, int n, string expected)
        => Parse(value).Pow(n).Should().Be(Parse(expected));

    [Fact]
    public void Pow_NegativeExponent_IsTheReciprocalOfThePositiveOne()
        => Parse("[2;4]").Pow(-1).Should().Be(Interval.One / Parse("[2;4]"));

    [Theory]
    [InlineData("[0.1;0.1]", 5, 1e-5)]
    [InlineData("[1.1;1.1]", 10, 2.5937424601000023)]
    public void Pow_EnclosesTheExactResult(string value, int n, double exact)
    {
        //ACT
        var result = Parse(value).Pow(n);

        //ASSERT - the whole point of directed rounding: the true value must lie inside
        result.Min.Should().BeLessThanOrEqualTo(exact);
        result.Max.Should().BeGreaterThanOrEqualTo(exact);
    }

    //=== 3.2 eq. 14: q([x],[y]) =================================================================

    [Theory]
    [InlineData("[1;2]", "[1;2]", 0.0)]
    [InlineData("[0;1]", "[2;5]", 4.0)]     // max(|0-2|, |1-5|)
    [InlineData("[-1;1]", "[0;0]", 1.0)]
    public void Distance(string x, string y, double expected)
    {
        //ASSERT - value, and symmetry, since the thesis proves q is a metric
        Interval.Distance(Parse(x), Parse(y)).Should().Be(expected);
        Interval.Distance(Parse(y), Parse(x)).Should().Be(expected);
    }

    [Fact]
    public void Distance_IsZeroOnlyForEqualIntervals()
    {
        Interval.Distance(Parse("[1;2]"), Parse("[1;2]")).Should().Be(0.0);
        Interval.Distance(Parse("[1;2]"), Parse("[1;3]")).Should().NotBe(0.0);
    }

    [Fact]
    public void Distance_SatisfiesTheTriangleInequality()
    {
        //ARRANGE
        var (x, y, z) = (Parse("[-2;1]"), Parse("[0;3]"), Parse("[5;9]"));

        //ASSERT
        Interval.Distance(x, z)
            .Should().BeLessThanOrEqualTo(Interval.Distance(x, y) + Interval.Distance(y, z));
    }

    //=== 3.2: division requires 0 not in [y] ====================================================

    [Fact]
    public void Division_ByIntervalContainingZero_IsUndefined()
        => ((Action)(() => { var _ = Parse("[1;2]") / Parse("[-1;1]"); }))
            .Should().Throw<DivideByZeroException>();
}
