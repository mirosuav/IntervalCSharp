using FluentAssertions;
using IntervalCSharp.Helpers;
using Xunit;

namespace IntervalCSharp.Tests;

public class FPURoundingTests
{
    [Theory]
    [InlineData(0.1,0.2)]
    [InlineData(2345678901234567890,8765432109876543210)]
    [InlineData(1e308,1e308)]
    public void RoundedDown_IsDifferentThanROundedUp(double d1, double d2)
    {
        //ACT
        var r1get = FPURounding.Get();
        FPURounding.Down();
        double rDown = d1 * d2;

        var r2get = FPURounding.Get();
        FPURounding.Up();
        double rUp= d1 * d2;

        var r3get = FPURounding.Get();
        FPURounding.Down();
        double rDown2 = d1 * d2;

        var r4get = FPURounding.Get();

        //ASSERT
        rDown2.Should().Be(rDown);
        rDown.Should().BeLessThan(rUp);

        r2get.Should().Be(FPURounding.RoundingMode.Down);
        r3get.Should().Be(FPURounding.RoundingMode.Up);
        r4get.Should().Be(FPURounding.RoundingMode.Down);

        FPURounding.Reset();

        FPURounding.Get().Should().Be(FPURounding.InitialRoundingMode);

    }


    [Theory]
    [InlineData(0.2, 2.2)]
    [InlineData(2345678901234567890, 8765432109876543210)]
    [InlineData(1e308, 1e308)]
    public void AddDoubles_RoundedDown_IsDifferentThanRoundedUp(double d1, double d2)
    {
        //ACT
        FPURounding.Down();
        double rDown = d1 + d2;

        FPURounding.Up();
        double rUp = d1 + d2;

        //ASSERT
        rDown.Should().BeLessThan(rUp);
    }



    [Theory]
    [InlineData(FPURounding.RoundingMode.Truncate)]
    [InlineData(FPURounding.RoundingMode.Up)]
    [InlineData(FPURounding.RoundingMode.Down)]
    public void SetRoundingMode_SetsMode(FPURounding.RoundingMode mode)
    {
        //ACT
        FPURounding.Set(mode);

        //ASSERT
        FPURounding.Get().Should().Be(mode);

        FPURounding.Reset();
    }


}
