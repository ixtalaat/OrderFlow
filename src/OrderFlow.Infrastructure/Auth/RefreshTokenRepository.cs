using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Auth;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Auth;

public sealed class RefreshTokenRepository(ApplicationDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> ListActiveByUserIdAsync(string userId, DateTime utcNow, CancellationToken cancellationToken = default)
        => await db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > utcNow)
            .ToListAsync(cancellationToken);

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        => db.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();

    public async Task RemoveExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var expired = await db.RefreshTokens.Where(x => x.ExpiresAtUtc <= utcNow).ToListAsync(cancellationToken);
        db.RefreshTokens.RemoveRange(expired);
    }

    public async Task RemoveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        db.RefreshTokens.RemoveRange(tokens);
    }
}
