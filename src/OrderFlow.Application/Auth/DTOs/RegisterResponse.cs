namespace OrderFlow.Application.Auth.DTOs;

public sealed record RegisterResponse(string UserId, string Email, bool RequiresEmailConfirmation);
