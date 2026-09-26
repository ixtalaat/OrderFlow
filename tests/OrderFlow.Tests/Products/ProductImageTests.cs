using FluentAssertions;
using FluentValidation;
using OrderFlow.Application.Products.Commands.SetProductImage;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Products;

public class ProductImageTests
{
    [Fact]
    public void SetImageUrl_Should_Store_Relative_Url()
    {
        var product = Product.Create("Widget", "Description", "WD-001", 10, 1);
        product.SetImageUrl("/product-images/abc.jpg");
        product.ImageUrl.Should().Be("/product-images/abc.jpg");
    }

    [Fact]
    public void SetImageUrl_Should_Clear_On_Null_Or_Whitespace()
    {
        var product = Product.Create("Widget", "Description", "WD-001", 10, 1);
        product.SetImageUrl("/product-images/abc.jpg");
        product.SetImageUrl("  ");
        product.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void SetImageUrl_Should_Reject_Too_Long_Url()
    {
        var product = Product.Create("Widget", "Description", "WD-001", 10, 1);
        var action = () => product.SetImageUrl(new string('a', 501));
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Image_Validator_Should_Accept_Png_Under_Limit()
    {
        using var content = new MemoryStream([0x89, 0x50, 0x4E, 0x47, 0x00]);
        var validator = new SetProductImageCommandValidator();
        validator.Validate(new SetProductImageCommand(1, content, "image/png", content.Length)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Image_Validator_Should_Reject_Empty_And_Oversize()
    {
        var validator = new SetProductImageCommandValidator();
        validator.Validate(new SetProductImageCommand(0, Stream.Null, "image/png", 0)).IsValid.Should().BeFalse();
        validator.Validate(new SetProductImageCommand(1, Stream.Null, "image/png", 6 * 1024 * 1024)).IsValid.Should().BeFalse();
    }
}
