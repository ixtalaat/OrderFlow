namespace OrderFlow.Application.Products.DTOs;

public sealed record CreateProductRequest(string Name, string Description, string Sku, decimal Price, string CategoryName);
