namespace OrderFlow.Application.Customers.DTOs;

public sealed record CreateCustomerRequest(
    string FullName,
    string Email,
    string Password,
    string PhoneNumber,
    string Address);
