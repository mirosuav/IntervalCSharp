using Xunit;
using FluentAssertions;

namespace IntervalCSharp.Tests;
using Interval = Interval<double>;
public class IntervalTests
{

    [Fact]
    public void Statics()
    {
        Interval._TTwo.Should().Be(2.0);
    }

    [Fact]
    public void Zero()
    {
        //ACT
        var sut = Interval.Zero;
        //ASSERT
        sut.Min.Should().Be(0);
        sut.Max.Should().Be(0);
    }

    [Fact]
    public void One()
    {
        //ACT
        var sut = Interval.One;
        //ASSERT
        sut.Min.Should().Be(1);
        sut.Max.Should().Be(1);
    }

    //Constructors tests

    [Fact]
    public void ConstructorEmpty()
    {
        //ACT
        var sut = new Interval();
        //ASSERT
        sut.Should().Be(Interval.Zero);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(0.0)]
    [InlineData(-1.1)]
    [InlineData(1e10)]
    public void ConstructorPoint(double point)
    {
        //ACT
        var sut = new Interval(point);
        //ASSERT
        sut.Min.Should().Be(point);
        sut.Max.Should().Be(point);
    }

    [Fact]
    public void ConstructorPointMin()
    {
        var sut = new Interval(double.MinValue);
        //ASSERT
        sut.Min.Should().Be(double.MinValue);
        sut.Max.Should().Be(double.MinValue);
    }

    [Fact]
    public void ConstructorPointMax()
    {
        var sut = new Interval(double.MaxValue);
        //ASSERT
        sut.Min.Should().Be(double.MaxValue);
        sut.Max.Should().Be(double.MaxValue);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    public void ConstructorMinMax(double min, double max)
    {
        //ACT
        var sut = new Interval(min, max);
        //ASSERT
        sut.Min.Should().Be(min);
        sut.Max.Should().Be(max);
    }


    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    public void ConstructorInterval(double min, double max)
    {
        //ARRANGE
        var initial = new Interval(min, max);
        //ACT
        var sut = new Interval(initial);
        //ASSERT
        sut.Min.Should().Be(min);
        sut.Max.Should().Be(max);
    }

    [Theory]
    [InlineData(false, 1.0, 1.0)]
    [InlineData(true, 0.0, 0.0)]
    [InlineData(true, 0.0, 0.0000000000001)]
    [InlineData(true, -double.Epsilon, double.Epsilon)]
    [InlineData(true, -1.1, 1.1)]
    [InlineData(false, -1.1, -0.01)]
    [InlineData(false, -1.1, null)]
    [InlineData(false, 1.1, null)]
    [InlineData(true, 0, null)]
    public void HasZero(bool hasZero, double min, double? max = null)
    {
        //ACT
        var sut = max is not null ? new Interval(min, max.Value) : new Interval(min);
        //ASSERT
        sut.HasZero.Should().Be(hasZero);
    }

    [Fact]
    public void AdditiveIdentity()
    {
        Interval.AdditiveIdentity.Should().Be(new Interval(0, 0));
    }

    [Fact]
    public void MultiplicativeIdentity()
    {
        Interval.MultiplicativeIdentity.Should().Be(new Interval(1, 1));
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void IsPoint(double min, double max)
    {
        //ARRANGE
        var initial = new Interval(min, max);
        //ACT
        var sut = initial.IsPoint;
        //ASSERT
        sut.Should().Be(min == max);
    }

    [Theory]
    [InlineData(1.0, 1.0, 0.0)]
    [InlineData(0.0, 0.0, 0.0)]
    [InlineData(-1.1, 1.1, 2.2)]
    [InlineData(-1, 0.0, 1.0)]
    [InlineData(1e18, 1.1e18, 0.1e18)]
    [InlineData(1e18, 1e18, 0.0)]
    public void Width(double min, double max, double expected)
    {
        //ARRANGE
        var sut = new Interval(min, max);
        //ASSERT
        sut.Width.Should().Be(expected);
    }


    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void Radius(double min, double max)
    {
        //ARRANGE
        var expected = (max - min) / 2.0;
        var initial = new Interval(min, max);
        //ACT
        var sut = initial.Radius;
        //ASSERT
        sut.Should().Be(expected);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void Middle(double min, double max)
    {
        //ARRANGE
        var expected = (min + max) / 2.0;
        var initial = new Interval(min, max);

        //ACT
        var sut = initial.Middle;
        //ASSERT
        sut.Should().Be(expected);
    }





}
