namespace OrderFlow.Application.Customers.DTOs;

public sealed record UpdateCustomerRequest(
    string FullName,
    string PhoneNumber,
    string Address);
