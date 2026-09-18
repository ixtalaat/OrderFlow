namespace OrderFlow.Domain.Entities;

public sealed class RefreshToken
{
    private RefreshToken() { }

    private RefreshToken(string userId, string tokenHash, DateTime expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User id is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public static RefreshToken Create(string userId, string tokenHash, DateTime expiresAtUtc)
        => new(userId, tokenHash, expiresAtUtc);

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

    public bool IsActive(DateTime utcNow) => RevokedAtUtc is null && !IsExpired(utcNow);

    public void Revoke() => RevokedAtUtc ??= DateTime.UtcNow;

    public void MarkReplaced(string newTokenHash)
    {
        if (string.IsNullOrWhiteSpace(newTokenHash)) throw new ArgumentException("Replacement hash is required.", nameof(newTokenHash));
        Revoke();
        ReplacedByTokenHash = newTokenHash;
    }
}
