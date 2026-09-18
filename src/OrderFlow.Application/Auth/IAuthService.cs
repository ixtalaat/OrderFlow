using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Auth;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);

    Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);

    Task<Result> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken);

    Task<Result> ResendConfirmationAsync(
        ResendConfirmationRequest request,
        CancellationToken cancellationToken);

    Task<Result<RefreshResponse>> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken);

    Task<Result> RevokeAsync(
        RevokeRequest request,
        CancellationToken cancellationToken);
}
