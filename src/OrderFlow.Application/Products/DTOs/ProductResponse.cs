namespace OrderFlow.Application.Products.DTOs;

public sealed record ProductResponse(int Id, string Name, string Description, string Sku, decimal Price, int CategoryId, string CategoryName, bool IsActive, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
