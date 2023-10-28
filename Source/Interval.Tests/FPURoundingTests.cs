using FluentAssertions;
using Xunit;

namespace Interval.Tests;

public class FPURoundingTests
{
    [Theory]
    [InlineData(0.1,0.2)]
    [InlineData(2345678901234567890,8765432109876543210)]
    [InlineData(1e308,1e308)]
    public void RoundedDown_IsDifferentThanROundedUp(double d1, double d2)
    {
        //ACT
        var r1get = FPUControl.GetRoundingMode();
        FPUControl.SetRoundingDOWN();
        double rDown = d1 * d2;

        var r2get = FPUControl.GetRoundingMode();
        FPUControl.SetRoundingUP();
        double rUp= d1 * d2;

        var r3get = FPUControl.GetRoundingMode();
        FPUControl.SetRoundingDOWN();
        double rDown2 = d1 * d2;

        var r4get = FPUControl.GetRoundingMode();

        //ASSERT
        rDown2.Should().Be(rDown);
        rDown.Should().BeLessThan(rUp);

        r2get.Should().Be(FPUControl.RoundingMode.Down);
        r3get.Should().Be(FPUControl.RoundingMode.Up);
        r4get.Should().Be(FPUControl.RoundingMode.Down);

        FPUControl.RevertRoundingMode();

        FPUControl.GetRoundingMode().Should().Be(FPUControl.InitialRoundingMode);

    }

    [Theory]
    [InlineData(FPUControl.RoundingMode.Truncate)]
    [InlineData(FPUControl.RoundingMode.Up)]
    [InlineData(FPUControl.RoundingMode.Down)]
    public void SetRoundingMode_SetsMode(FPUControl.RoundingMode mode)
    {
        //ACT
        FPUControl.SetRoundingMode(mode);

        //ASSERT
        FPUControl.GetRoundingMode().Should().Be(mode);

        FPUControl.RevertRoundingMode();
    }


}
