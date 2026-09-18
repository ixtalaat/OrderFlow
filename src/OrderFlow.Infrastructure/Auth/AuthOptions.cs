namespace OrderFlow.Infrastructure.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public bool RequireConfirmedEmail { get; set; } = true;

    public int RefreshTokenDays { get; set; } = 7;
}
