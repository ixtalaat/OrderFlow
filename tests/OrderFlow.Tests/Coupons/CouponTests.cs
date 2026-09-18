using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Coupons;

public sealed class CouponTests
{
    private static readonly DateTime From = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_Should_Normalize_Code()
    {
        var coupon = Coupon.Create(" save10 ", 10, 0, From, To, null);
        coupon.Code.Should().Be("SAVE10");
        coupon.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_Should_Reject_Invalid_Inputs()
    {
        FluentActions.Invoking(() => Coupon.Create("", 10, 0, From, To, null))
            .Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => Coupon.Create("SAVE10", 0, 0, From, To, null))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => Coupon.Create("SAVE10", 101, 0, From, To, null))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => Coupon.Create("SAVE10", 10, -1, From, To, null))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => Coupon.Create("SAVE10", 10, 0, To, From, null))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CanRedeem_Should_Respect_Window_Minimum_And_Limit()
    {
        var coupon = Coupon.Create("SAVE10", 10, 50, From, To, 1);
        coupon.CanRedeem(100, From.AddDays(1)).Should().BeTrue();
        coupon.CanRedeem(49, From.AddDays(1)).Should().BeFalse();
        coupon.CanRedeem(100, From.AddDays(-1)).Should().BeFalse();
        coupon.CanRedeem(100, To).Should().BeFalse();
        coupon.Redeem();
        coupon.CanRedeem(100, From.AddDays(1)).Should().BeFalse();
        FluentActions.Invoking(coupon.Redeem).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deactivate_Should_Block_Redemption()
    {
        var coupon = Coupon.Create("SAVE10", 10, 0, From, null, null);
        coupon.Deactivate();
        coupon.CanRedeem(100, From.AddDays(1)).Should().BeFalse();
    }
}
