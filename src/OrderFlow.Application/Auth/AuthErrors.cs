using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Auth;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        new(
            "Authentication.InvalidCredentials",
            "Invalid email or password.", StatusCodes.Status401Unauthorized);

    public static readonly Error EmailAlreadyExists =
        new(
            "Authentication.EmailAlreadyExists",
            "Email is already registered.", StatusCodes.Status400BadRequest);

    public static readonly Error UserCreationFailed =
        new(
            "Authentication.UserCreationFailed",
            "Failed to create user.", StatusCodes.Status400BadRequest);

    public static readonly Error EmailNotConfirmed =
        new(
            "Authentication.EmailNotConfirmed",
            "Email address has not been confirmed.", StatusCodes.Status403Forbidden);

    public static readonly Error InvalidConfirmationToken =
        new(
            "Authentication.InvalidConfirmationToken",
            "Email confirmation failed.", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidRefreshToken =
        new(
            "Authentication.InvalidRefreshToken",
            "Refresh token is invalid or expired.", StatusCodes.Status401Unauthorized);
}
