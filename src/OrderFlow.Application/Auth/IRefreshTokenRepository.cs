using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Auth;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> ListActiveByUserIdAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RemoveExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    Task RemoveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
