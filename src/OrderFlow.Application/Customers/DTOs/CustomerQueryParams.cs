namespace OrderFlow.Application.Customers.DTOs;

public sealed record CustomerQueryParams(
    string? SearchTerm = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 10);
