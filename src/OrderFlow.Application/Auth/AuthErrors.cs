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
            "Email is already registered.", StatusCodes.Status409Conflict);

    public static readonly Error UserCreationFailed =
        new(
            "Authentication.UserCreationFailed",
            "Failed to create user.", StatusCodes.Status400BadRequest);
}
