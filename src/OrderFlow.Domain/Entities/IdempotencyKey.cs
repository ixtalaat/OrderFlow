namespace OrderFlow.Domain.Entities;

public sealed class IdempotencyKey
{
    private IdempotencyKey() { }

    private IdempotencyKey(string userId, string key, string requestHash, int statusCode, string responseBody, DateTime expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User id is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));
        UserId = userId;
        Key = key;
        RequestHash = requestHash;
        ResponseStatusCode = statusCode;
        ResponseBody = responseBody;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string UserId { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public int ResponseStatusCode { get; private set; }
    public string ResponseBody { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    public static IdempotencyKey Create(string userId, string key, string requestHash, int statusCode, string responseBody, DateTime expiresAtUtc)
        => new(userId, key, requestHash, statusCode, responseBody, expiresAtUtc);

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;
}
