using FluentAssertions;
using OrderFlow.Application.Pricing.Commands.CreatePricingRule;
using OrderFlow.Application.Pricing.Commands.UpdatePricingRule;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Pricing;

public sealed class PricingRuleValidatorTests
{
    private readonly CreatePricingRuleCommandValidator _createValidator = new();
    private readonly UpdatePricingRuleCommandValidator _updateValidator = new();
    private static readonly DateTime From = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreatePricingRuleCommandValidator_Should_Pass_For_Valid_Command()
    {
        var command = new CreatePricingRuleCommand(1, CustomerTier.Wholesale, 10, From, To);
        _createValidator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreatePricingRuleCommandValidator_Should_Pass_For_Open_Ended_Validity()
    {
        var command = new CreatePricingRuleCommand(1, CustomerTier.Vip, 15, From, null);
        _createValidator.Validate(command).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 10, false)]
    [InlineData(1, -1, false)]
    [InlineData(1, 101, false)]
    [InlineData(1, 0, true)]
    [InlineData(1, 100, true)]
    public void CreatePricingRuleCommandValidator_Should_Validate_Product_And_Discount(
        int productId, decimal discount, bool expected)
    {
        var command = new CreatePricingRuleCommand(productId, CustomerTier.Regular, discount, From, To);
        _createValidator.Validate(command).IsValid.Should().Be(expected);
    }

    [Fact]
    public void CreatePricingRuleCommandValidator_Should_Fail_When_Validity_Ends_Before_Start()
    {
        var command = new CreatePricingRuleCommand(1, CustomerTier.Regular, 10, To, From);
        _createValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdatePricingRuleCommandValidator_Should_Pass_For_Valid_Command()
    {
        var command = new UpdatePricingRuleCommand(1, CustomerTier.Wholesale, 10, From, To);
        _updateValidator.Validate(command).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 10, true, false)]
    [InlineData(1, 101, true, false)]
    [InlineData(1, 10, false, false)]
    public void UpdatePricingRuleCommandValidator_Should_Validate_Id_Discount_And_Window(
        int id, decimal discount, bool validWindow, bool expected)
    {
        var command = new UpdatePricingRuleCommand(
            id, CustomerTier.Regular, discount, From, validWindow ? To : From.AddDays(-1));
        _updateValidator.Validate(command).IsValid.Should().Be(expected);
    }
}
