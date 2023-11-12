using Xunit;
using FluentAssertions;
using System.Globalization;
using Xunit.Sdk;

namespace IntervalCSharp.Tests;
using Interval = Interval<double>;
public class IntervalTests
{
    public static IFormatProvider formatUS = CultureInfo.GetCultureInfo("en-US");

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
        sut.Inf.Should().Be(0.0);
        sut.Sup.Should().Be(0.0);
    }

    [Fact]
    public void One()
    {
        //ACT
        var sut = Interval.One;
        //ASSERT
        sut.Inf.Should().Be(1.0);
        sut.Sup.Should().Be(1.0);
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
    [InlineData(1e308)]
    public void ConstructorPoint(double point)
    {
        //ACT
        var sut = new Interval(point);
        //ASSERT
        sut.Inf.Should().Be(point);
        sut.Sup.Should().Be(point);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(0.0)]
    [InlineData(-1.1)]
    [InlineData(1e10)]
    [InlineData(1e308)]
    public void ImplicitOperatorNumber(double number)
    {
        //ACT
        Interval sut = number;
        //ASSERT
        sut.Inf.Should().Be(number);
        sut.Sup.Should().Be(number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1.0)]
    [InlineData(0.0)]
    [InlineData(-1.1)]
    [InlineData(1e10)]
    [InlineData(1e308)]
    public void ImplicitOperatorNullableNumber(double? number)
    {
        //ACT
        Interval? sut = number;
        //ASSERT
        if (number is null)
        {
            sut.Should().BeNull();
        }
        else
        {
            sut.Should().NotBeNull();
            sut.Value.Inf.Should().Be(number);
            sut.Value.Sup.Should().Be(number);
        }
    }

    [Fact]
    public void ImplicitOperatorTupleNumber()
    {
        //ACT 1
        Interval? sut = (Tuple<double, double>)null;
        sut.Should().BeNull();

        //ACT 2
        var t = Tuple.Create(1.0, 2.0);
        sut = t;
        sut.Should().NotBeNull();
        sut.Value.Inf.Should().Be(t.Item1);
        sut.Value.Sup.Should().Be(t.Item2);

    }

    [Fact]
    public void ConstructorPointMin()
    {
        var sut = new Interval(double.MinValue);
        //ASSERT
        sut.Inf.Should().Be(double.MinValue);
        sut.Sup.Should().Be(double.MinValue);
    }

    [Fact]
    public void ConstructorPointMax()
    {
        var sut = new Interval(double.MaxValue);
        //ASSERT
        sut.Inf.Should().Be(double.MaxValue);
        sut.Sup.Should().Be(double.MaxValue);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(double.MaxValue, double.MaxValue)]
    [InlineData(double.MinValue, double.MinValue)]
    [InlineData(double.MinValue, double.MaxValue)]
    public void ConstructorMinMax(double min, double max)
    {
        //ACT
        var sut = new Interval(min, max);
        //ASSERT
        sut.Inf.Should().Be(min);
        sut.Sup.Should().Be(max);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(double.MaxValue, double.MaxValue)]
    [InlineData(double.MinValue, double.MinValue)]
    [InlineData(double.MinValue, double.MaxValue)]
    public void ConstructorMinMaxSwapped(double min, double max)
    {
        //ACT
        var sut = new Interval(max, min);
        //ASSERT
        sut.Inf.Should().Be(min);
        sut.Sup.Should().Be(max);
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
        sut.Inf.Should().Be(min);
        sut.Sup.Should().Be(max);
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
        Interval.AdditiveIdentity.Should().Be(Interval.Zero);
    }


    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void AdditiveIdentityProof(double min, double max)
    {
        //ARRANGE
        var initial = new Interval(min, max);
        //ACT
        var sut = initial + Interval.AdditiveIdentity;
        //ASSERT
        sut.Should().Be(initial);
    }

    [Fact]
    public void MultiplicativeIdentity()
    {
        Interval.MultiplicativeIdentity.Should().Be(Interval.One);
    }


    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void MultiplicativeIdentityProof(double min, double max)
    {
        //ARRANGE
        var initial = new Interval(min, max);
        //ACT
        var sut = initial * Interval.MultiplicativeIdentity;
        //ASSERT
        sut.Should().Be(initial);
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
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void IsZero(double min, double max)
    {
        //ARRANGE
        var initial = new Interval(min, max);
        //ACT
        var sut = initial.IsZero;
        //ASSERT
        sut.Should().Be(min == 0.0 && max == 0.0);
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
    [InlineData(-3, 1)]
    [InlineData(-3, -1)]
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
        sut.Should().BeGreaterThanOrEqualTo(0.0);
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
        double expected = (min + max) / 2.0;
        var initial = new Interval(min, max);

        //ACT
        var sut = initial.Middle;
        //ASSERT
        sut.Should().Be(expected);
    }


    [Theory]
    [InlineData("[0;0]", "[0;0]", "[0;0]")]
    [InlineData("[1;1]", "[0;0.0]", "[1;1]")]
    [InlineData("[1;1]", "[1;1]", "[2;2]")]
    [InlineData("[-1;0]", "[2;2]", "[1;2]")]
    [InlineData("[-1.1;0.2]", "[2.1;2.2]", "[1;2.4000000000000004]")]
    [InlineData("[1e18;2e18]", "[1e18;2e18]", "[2e18;4e18]")]
    public void PlusOperator(string lefts, string rights, string expecteds)
    {
        //ARRANGE
        var left = Interval.Parse(lefts, formatUS);
        var right = Interval.Parse(rights, formatUS);
        var expected = Interval.Parse(expecteds, formatUS);

        //ACT
        var result = left + right;

        //ASSERT
        result.Should().Be(expected);
    }


    [Theory]
    [InlineData("[0;0]", "[0;0]", "[0;0]")]
    [InlineData("[1;1]", "[0;0.0]", "[1;1]")]
    [InlineData("[1;1]", "[1;1]", "[0;0]")]
    [InlineData("[-1;0]", "[2;2]", "[-3;-2]")]
    [InlineData("[-2;-1]", "[2;3]", "[-5;-3]")]
    [InlineData("[-4;-2]", "[-3;-1]", "[-3;1]")]
    public void MinusOperator(string lefts, string rights, string expecteds)
    {
        //ARRANGE
        var left = Interval.Parse(lefts, formatUS);
        var right = Interval.Parse(rights, formatUS);
        var expected = Interval.Parse(expecteds, formatUS);

        //ACT
        var result = left - right;

        //ASSERT
        result.Should().Be(expected);
    }


    [Theory]
    [InlineData("[0;0]", "[0;0]", "[0;0]")]
    [InlineData("[1;1]", "[0;0.0]", "[0;0]")]
    [InlineData("[1;1]", "[1;1]", "[1;1]")]
    [InlineData("[-1;0]", "[2;2]", "[-2;0]")]
    [InlineData("[-2;-1]", "[2;3]", "[-6;-2]")]
    [InlineData("[-4;-2]", "[-3;-1]", "[2;12]")]
    public void MultiplyOperator(string lefts, string rights, string expecteds)
    {
        //ARRANGE
        var left = Interval.Parse(lefts, formatUS);
        var right = Interval.Parse(rights, formatUS);
        var expected = Interval.Parse(expecteds, formatUS);

        //ACT
        var result = left * right;

        //ASSERT
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("[0;0]", "[0;0]")]
    [InlineData("[1;1]", "[0;0.0]")]
    [InlineData("[1;1]", "[-1;0]")]
    [InlineData("[1;1]", "[0;1]")]
    [InlineData("[-1;0]", "[-2;2]")]
    [InlineData("[0;0]", "[-2;2]")]
    public void DivisionOperator_ByZero_Throws(string lefts, string rights)
    {
        //ARRANGE
        var left = Interval.Parse(lefts, formatUS);
        var right = Interval.Parse(rights, formatUS);

        //ACT
        Interval result;
        ((Action)(() => result = left / right))
        .Should().Throw<DivideByZeroException>();
    }

    [Theory]
    [InlineData("[1;1]", "[1;1]", "[1;1]")]
    [InlineData("[-1;0]", "[2;2]", "[-0.5;0]")]// -0.5, 0
    [InlineData("[-2;-1]", "[2;4]", "[-1;-0.25]")]//-1,-0.5,-0.25 
    [InlineData("[-4;-2]", "[-0.4;-0.2]", "[4.999999999999999;20]")]//10, 20, 5, 10
    [InlineData("[12;12]", "[6;6]", "[2;2]")]//
    public void DivisionOperator(string lefts, string rights, string expecteds)
    {
        //ARRANGE
        var left = Interval.Parse(lefts, formatUS);
        var right = Interval.Parse(rights, formatUS);
        var expected = Interval.Parse(expecteds, formatUS);

        //ACT
        var result = left / right;

        //ASSERT
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    public void OperatorPlusPlus(double min, double max)
    {
        //ARRANGE
        var sut = new Interval(min, max);

        //ACT
        sut++;

        //ASSERT
        sut.Inf.Should().Be(min + 1.0);
        sut.Sup.Should().Be(max + 1.0);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void OperatorMinusMinus(double min, double max)
    {
        //ARRANGE
        var sut = new Interval(min, max);
        var org = sut;

        //ACT
        sut--;

        //ASSERT
        sut.Should().Be(org - Interval.One);
    }



    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void OperatorUnaryPlus(double min, double max)
    {
        //ARRANGE
        var sut = new Interval(min, max);

        //ACT
        var result = +sut;

        //ASSERT
        result.Should().Be(sut);
        result.Inf.Should().Be(min);
        result.Sup.Should().Be(max);
    }

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-1.1, 1.1)]
    [InlineData(-1.1, -0.01)]
    [InlineData(1e18, 1.1e18)]
    [InlineData(1e18, 1e18)]
    public void OperatorUnaryMinus(double min, double max)
    {
        //ARRANGE
        var sut = new Interval(min, max);

        //ACT
        var result = -sut;

        //ASSERT
        result.Inf.Should().Be(-max);
        result.Sup.Should().Be(-min);
    }


    [Theory]
    [InlineData(null, null, true)]
    [InlineData("[0;0]", "[0;0]", true)]
    [InlineData("[1;1]", "[1;1]", true)]
    [InlineData("[1e18;2e18]", "[1e18;2e18]", true)]
    [InlineData("[1.0e18;2e18]", "[1e18;2.0e18]", true)]

    [InlineData("[1;1]", null, false)]
    [InlineData(null, "[1;1]", false)]
    [InlineData("[1;1]", "[0;0.0]", false)]
    [InlineData("[-1;0]", "[2;2]", false)]
    [InlineData("[-1.1;0.2]", "[2.1;2.2]", false)]
    public void EqualNotEqualOperator(string lefts, string rights, bool expected)
    {
        //ARRANGE
        var left = lefts is null ? (Interval?)null : Interval.Parse(lefts, formatUS);
        var right = rights is null ? (Interval?)null : Interval.Parse(rights, formatUS);

        //ACT
        var result = left == right;
        var notResult = left != right;

        //ASSERT
        result.Should().Be(expected);
        notResult.Should().Be(!expected);
    }




    [Theory]
    [InlineData("[1.0;1.0]", 1.0, 1.0)]
    [InlineData("[ 0.0 ; 0.0 ]", 0.0, 0.0)]
    [InlineData("[-1.0;1.1 ]", -1.0, 1.1)]
    [InlineData("[-1.1; -0.01]", -1.1, -0.01)]
    [InlineData("[ 1e18;1.1e18]", 1e18, 1.1e18)]
    [InlineData("[1.0E18 ;+1.0E18]", 1e18, 1e18)]
    public void Parse_Valid(string str, double min, double max)
    {
        //ACT
        var sut = Interval.Parse(str, formatUS);
        //ASSERT
        sut.Inf.Should().Be(min);
        sut.Sup.Should().Be(max);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(";8")]
    [InlineData("1.0")]
    [InlineData("1;0")]
    [InlineData("[1.0")]
    [InlineData("[1;0")]
    [InlineData("1;0]")]
    [InlineData("[1.0]")]
    [InlineData("[1.0;]")]
    [InlineData("[1.0;a]")]
    [InlineData("[1.0;1.0a]")]
    [InlineData("[1;0;1,0]")]
    [InlineData("[1.0;;1.0]")]
    [InlineData("[1.0,1.0]")]
    public void Parse_InValid(string str)
    {
        //ACT
        ((Action)(() => Interval.Parse(str, formatUS)))
        .Should().Throw<IntervalFormatExceptions>();
    }

    [Theory]
    [InlineData("[1.0;1.0]", 1.0, 1.0, true)]
    [InlineData("[ 0.0 ; 0.0 ]", 0.0, 0.0, true)]
    [InlineData("[-1.0;1.1 ]", -1.0, 1.1, true)]
    [InlineData("[-1.1; -0.01]", -1.1, -0.01, true)]
    [InlineData("[ 1e18;1.1e18]", 1e18, 1.1e18, true)]
    [InlineData("[1.0E18 ;+1.0E18]", 1e18, 1e18, true)]

    [InlineData(null, 0, 0, false)]
    [InlineData("", 0, 0, false)]
    [InlineData("1.0", 0, 0, false)]
    [InlineData("[1.0", 0, 0, false)]
    [InlineData("[1.0]", 0, 0, false)]
    [InlineData("[1.0;]", 0, 0, false)]
    [InlineData("[1.0;a]", 0, 0, false)]
    [InlineData("[1.0;1.0a]", 0, 0, false)]
    [InlineData("[1;0;1,0]", 0, 0, false)]
    [InlineData("[1.0;;1.0]", 0, 0, false)]
    [InlineData("[1.0,1.0]", 0, 0, false)]
    public void TryParse(string str, double min, double max, bool expected)
    {
        //ACT
        var result = Interval.TryParse(str, formatUS, out var sut);
        //ASSERT
        result.Should().Be(expected);
        if (expected)
        {
            sut.Inf.Should().Be(min);
            sut.Sup.Should().Be(max);
        }
    }



}
