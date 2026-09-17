using FluentAssertions;
using OrderFlow.Application.Products.Commands.CreateProduct;
using OrderFlow.Application.Products.Commands.UpdateProduct;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Application.Products.Queries.GetCatalogProducts;
using OrderFlow.Application.Products.Queries.GetProducts;

namespace OrderFlow.Tests.Products;

public sealed class ProductValidatorTests
{
    private readonly CreateProductCommandValidator _createValidator = new();
    private readonly UpdateProductCommandValidator _updateValidator = new();
    private readonly GetProductsQueryValidator _queryValidator = new();
    private readonly GetCatalogProductsQueryValidator _catalogValidator = new();

    [Fact]
    public void CreateProductCommandValidator_Should_Pass_For_Valid_Command()
    {
        var command = new CreateProductCommand("Product", "Description", "SKU-123", 10m, "Category");
        _createValidator.Validate(command).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Description", "SKU-123", 10, "Category")]
    [InlineData("Product", "", "SKU-123", 10, "Category")]
    [InlineData("Product", "Description", "!!", 10, "Category")]
    [InlineData("Product", "Description", "SKU-123", -1, "Category")]
    [InlineData("Product", "Description", "SKU-123", 10, "")]
    public void CreateProductCommandValidator_Should_Fail_For_Invalid_Inputs(
        string name, string description, string sku, decimal price, string category)
    {
        var command = new CreateProductCommand(name, description, sku, price, category);
        _createValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateProductCommandValidator_Should_Pass_For_Valid_Command()
    {
        var command = new UpdateProductCommand(1, "Product", "Description", "SKU-123", 10m, "Category");
        _updateValidator.Validate(command).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, "Product", "Description", "SKU-123", 10, "Category")]
    [InlineData(1, "", "Description", "SKU-123", 10, "Category")]
    [InlineData(1, "Product", "Description", "!!", 10, "Category")]
    [InlineData(1, "Product", "Description", "SKU-123", -1, "Category")]
    public void UpdateProductCommandValidator_Should_Fail_For_Invalid_Inputs(
        int id, string name, string description, string sku, decimal price, string category)
    {
        var command = new UpdateProductCommand(id, name, description, sku, price, category);
        _updateValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 20, true)]
    [InlineData(0, 20, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    public void GetProductsQueryValidator_Should_Validate_Pagination(int pageNumber, int pageSize, bool expected)
    {
        var query = new GetProductsQuery(new ProductQueryParams(null, null, pageNumber, pageSize));
        _queryValidator.Validate(query).IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 20, true)]
    [InlineData(0, 20, false)]
    [InlineData(1, 101, false)]
    public void GetCatalogProductsQueryValidator_Should_Validate_Pagination(int pageNumber, int pageSize, bool expected)
    {
        var query = new GetCatalogProductsQuery(0, new CatalogQueryParams(null, pageNumber, pageSize));
        _catalogValidator.Validate(query).IsValid.Should().Be(expected);
    }
}
