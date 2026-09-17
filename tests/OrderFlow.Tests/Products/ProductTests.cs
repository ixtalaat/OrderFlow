using FluentAssertions;
using OrderFlow.Application.Products;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Products;

public class ProductTests
{
    [Fact]
    public void Create_Should_Normalize_Sku_And_Activate_Product()
    {
        var product = Product.Create("Widget", "A useful widget", " wd-001 ", 12.50m, 1);
        product.Sku.Should().Be("WD-001");
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_Should_Hide_Product()
    {
        var product = Product.Create("Widget", "A useful widget", "WD-001", 12.50m, 1);
        product.Deactivate();
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Create_Should_Reject_Invalid_Price()
    {
        var action = () => Product.Create("Widget", "Description", "WD-001", -1, 1);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Validator_Should_Reject_Missing_Required_Fields()
    {
        var result = new CreateProductCommandValidator().Validate(
            new CreateProductCommand("", "", "bad sku", -1, ""));
        result.IsValid.Should().BeFalse();
    }
}
