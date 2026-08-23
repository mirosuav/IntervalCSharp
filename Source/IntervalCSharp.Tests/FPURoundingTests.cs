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
        var r1get = FpuRounding.Get();
        FpuRounding.Down();
        double rDown = d1 * d2;

        var r2get = FpuRounding.Get();
        FpuRounding.Up();
        double rUp= d1 * d2;

        var r3get = FpuRounding.Get();
        FpuRounding.Down();
        double rDown2 = d1 * d2;

        var r4get = FpuRounding.Get();

        //ASSERT
        rDown2.Should().Be(rDown);
        rDown.Should().BeLessThan(rUp);

        r2get.Should().Be(RoundingMode.Down);
        r3get.Should().Be(RoundingMode.Up);
        r4get.Should().Be(RoundingMode.Down);

        FpuRounding.Reset();

        FpuRounding.Get().Should().Be(FpuRounding.InitialRoundingMode);

    }


    [Theory]
    [InlineData(0.2, 2.2)]
    [InlineData(2345678901234567890, 8765432109876543210)]
    [InlineData(1e308, 1e308)]
    public void AddDoubles_RoundedDown_IsDifferentThanRoundedUp(double d1, double d2)
    {
        //ACT
        FpuRounding.Down();
        double rDown = d1 + d2;

        FpuRounding.Up();
        double rUp = d1 + d2;

        //ASSERT
        rDown.Should().BeLessThan(rUp);
    }



    [Theory]
    [InlineData(RoundingMode.Truncate)]
    [InlineData(RoundingMode.Up)]
    [InlineData(RoundingMode.Down)]
    public void SetRoundingMode_SetsMode(RoundingMode mode)
    {
        //ACT
        FpuRounding.Set(mode);

        //ASSERT
        FpuRounding.Get().Should().Be(mode);

        FpuRounding.Reset();
    }


}
