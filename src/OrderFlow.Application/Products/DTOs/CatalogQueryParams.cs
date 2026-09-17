namespace OrderFlow.Application.Products.DTOs;

public sealed record CatalogQueryParams(string? SearchTerm = null, int PageNumber = 1, int PageSize = 20);
