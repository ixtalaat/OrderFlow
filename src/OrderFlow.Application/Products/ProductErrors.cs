using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Products;

public static class ProductErrors
{
    public static readonly Error NotFound = new("Product.NotFound", "Product not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateSku = new("Product.DuplicateSku", "SKU is already in use.", StatusCodes.Status409Conflict);
    public static readonly Error CategoryNotFound = new("Product.CategoryNotFound", "Category could not be created.", StatusCodes.Status400BadRequest);
}
