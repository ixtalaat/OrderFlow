namespace OrderFlow.Application.Auth.DTOs;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    IEnumerable<string> Roles);
