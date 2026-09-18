using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Common.Identity;

public interface IIdentityService
{
    Task<Result<(string UserId, string Email, string FullName)>> CreateUserAsync(
        string email,
        string password,
        string fullName,
        string role,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateUserFullNameAsync(
        string userId,
        string fullName,
        CancellationToken cancellationToken = default);

    Task<bool> IsEmailUniqueAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> UserExistsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task IncrementTokenVersionAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<bool> AnonymizeUserAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
