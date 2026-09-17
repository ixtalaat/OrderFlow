namespace OrderFlow.Application.Orders.DTOs;

public sealed record OrderQueryParams(int PageNumber = 1, int PageSize = 20);
