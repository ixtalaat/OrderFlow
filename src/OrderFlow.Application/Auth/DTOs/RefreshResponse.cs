namespace OrderFlow.Application.Auth.DTOs;

public sealed record RefreshResponse(string AccessToken, DateTime ExpiresAtUtc, string RefreshToken, DateTime RefreshExpiresAtUtc);
