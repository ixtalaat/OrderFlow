namespace OrderFlow.Application.Products.DTOs;

public sealed record CatalogProductResponse(int Id, string Name, string Description, string Sku, string CategoryName, decimal CurrentCustomerPrice, int AvailableQuantity);
