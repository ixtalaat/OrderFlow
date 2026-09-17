namespace OrderFlow.Application.Auth.DTOs;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string PhoneNumber = "Not provided",
    string Address = "Not provided");
