using FluentAssertions;
using System.Globalization;
using Xunit;

namespace IntervalCSharp.Tests;
using Interval = Interval<double>;
public class IntervalFunctionsTests
{
    public static IFormatProvider formatUS = CultureInfo.GetCultureInfo("en-US");

    [Theory]
    [InlineData("[0;0]", "[0;0]")]
    [InlineData("[0;1]", "[0;1]")]
    [InlineData("[0;1e308]", "[0;1e308]")]
    [InlineData("[-1;1]", "[0;1]")]
    [InlineData("[-3;5]", "[0;5]")]
    [InlineData("[-3;1]", "[0;3]")]
    [InlineData("[-0.1;-0.01]", "[0.01;0.1]")]

    [InlineData("[-3;-1]", "[1;3]")]
    [InlineData("[1;3]", "[1;3]")]
    [InlineData("[1e302;1e308]", "[1e302;1e308]")]
    public void Abs(string lefts, string expecteds)
    {
        //ARRANGE
        var left = Interval.Parse(lefts, formatUS);
        var expected = Interval.Parse(expecteds, formatUS);

        //ACT
        var result = left.Abs();

        //ASSERT
        result.Should().Be(expected);
    }
}
