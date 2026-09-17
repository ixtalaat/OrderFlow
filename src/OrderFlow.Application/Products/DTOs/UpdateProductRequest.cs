namespace OrderFlow.Application.Products.DTOs;

public sealed record UpdateProductRequest(string Name, string Description, string Sku, decimal Price, string CategoryName);
