namespace OrderFlow.Application.Auth.DTOs;

public sealed record ConfirmEmailRequest(string Email, string Token);
