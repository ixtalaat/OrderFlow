namespace OrderFlow.Application.Customers.DTOs;

public sealed record CustomerResponse(
    int Id,
    string UserId,
    string FullName,
    string Email,
    string PhoneNumber,
    string Address,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
