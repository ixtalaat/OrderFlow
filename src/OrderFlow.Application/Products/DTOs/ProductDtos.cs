namespace OrderFlow.Application.Products.DTOs;

public sealed record CreateProductRequest(string Name, string Description, string Sku, decimal Price, string CategoryName);
public sealed record UpdateProductRequest(string Name, string Description, string Sku, decimal Price, string CategoryName);
public sealed record ProductResponse(int Id, string Name, string Description, string Sku, decimal Price, int CategoryId, string CategoryName, bool IsActive, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
public sealed record ProductQueryParams(string? SearchTerm = null, bool? IsActive = null, int PageNumber = 1, int PageSize = 20);
