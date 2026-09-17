namespace OrderFlow.Application.Products.DTOs;

public sealed record ProductQueryParams(string? SearchTerm = null, bool? IsActive = null, int PageNumber = 1, int PageSize = 20);
