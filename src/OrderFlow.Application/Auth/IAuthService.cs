using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);

    Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}
