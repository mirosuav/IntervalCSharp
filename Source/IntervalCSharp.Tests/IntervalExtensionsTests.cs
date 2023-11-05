using FluentAssertions;
using System.Globalization;
using Xunit;

namespace IntervalCSharp.Tests;
using Interval = Interval<double>;

public class IntervalExtensionsTests
{
    public static IFormatProvider formatUS = CultureInfo.GetCultureInfo("en-US");

    [Theory]
    [InlineData("[0;0]", "[0;0]", true)]
    [InlineData("[0;1]", "[0;0]", true)]
    [InlineData("[-1;0]", "[0;0]", true)]
    [InlineData("[-1;1]", "[0;0]", true)]
    [InlineData("[2;5]", "[2;5]", true)]
    [InlineData("[-1e18;1e18]", "[0;1e18]", true)]

    [InlineData("[0;0]", "[1e18;1]", false)]
    [InlineData("[-1e18;-0.1]", "[0;0]", false)]
    [InlineData("[1;2]", "[2.1;3]", false)]
    [InlineData("[-2;2]", "[-4;-3]", false)]
    public void Contains(string lefts, string rights, bool expected)
    {
        //ARRANGE
        var left =  Interval.Parse(lefts, formatUS);
        var right =  Interval.Parse(rights, formatUS);

        //ACT
        var result = left.Contains(right);

        //ASSERT
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("[0;0]", "[0;0]", true)]
    [InlineData("[0;1]", "[0;0]", true)]
    [InlineData("[-1;0]", "[0;1]", true)]
    [InlineData("[-1;1]", "[0;0]", true)]
    [InlineData("[2;5]", "[-2;5]", true)]
    [InlineData("[2;5]", "[-2;3]", true)]
    [InlineData("[2;5]", "[3;7]", true)]
    [InlineData("[-1e18;1e18]", "[0;2e18]", true)]

    [InlineData("[0;0]", "[4.9406564584124654E-324;1]", false)]
    [InlineData("[-1e18;-0.1]", "[0;0]", false)]
    [InlineData("[1;2]", "[2.1;3]", false)]
    [InlineData("[-2;2]", "[-4;-3]", false)]
    public void Overlaps(string lefts, string rights, bool expected)
    {
        //ARRANGE
        var left = Interval.Parse(lefts, formatUS);
        var right = Interval.Parse(rights, formatUS);

        //ACT
        var result = left.Overlaps(right);

        //ASSERT
        result.Should().Be(expected);
    }
}
