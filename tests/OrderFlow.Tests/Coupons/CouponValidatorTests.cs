using FluentAssertions;
using OrderFlow.Application.Coupons.Commands.CreateCoupon;
using OrderFlow.Application.Coupons.Commands.UpdateCoupon;

namespace OrderFlow.Tests.Coupons;

public sealed class CouponValidatorTests
{
    private readonly CreateCouponCommandValidator _createValidator = new();
    private readonly UpdateCouponCommandValidator _updateValidator = new();
    private static readonly DateTime From = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateCouponCommandValidator_Should_Pass_For_Valid_Command()
    {
        var command = new CreateCouponCommand("SAVE10", 10, 50, From, To, 100);
        _createValidator.Validate(command).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", 10, 0, true, false)]
    [InlineData("SAVE10", 0, 0, true, false)]
    [InlineData("SAVE10", 101, 0, true, false)]
    [InlineData("SAVE10", 10, -1, true, false)]
    [InlineData("SAVE10", 10, 0, false, false)]
    public void CreateCouponCommandValidator_Should_Validate_Inputs(
        string code, decimal discount, decimal minTotal, bool validWindow, bool expected)
    {
        var command = new CreateCouponCommand(code, discount, minTotal, From, validWindow ? To : From.AddDays(-1), null);
        _createValidator.Validate(command).IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 10, true, false)]
    [InlineData(1, 10, true, true)]
    [InlineData(1, 0, true, false)]
    [InlineData(1, 10, false, false)]
    public void UpdateCouponCommandValidator_Should_Validate_Inputs(
        int id, decimal discount, bool validWindow, bool expected)
    {
        var command = new UpdateCouponCommand(id, discount, 0, From, validWindow ? To : From.AddDays(-1), null);
        _updateValidator.Validate(command).IsValid.Should().Be(expected);
    }
}
