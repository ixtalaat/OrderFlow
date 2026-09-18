namespace OrderFlow.Application.Auth;

public interface IJwtTokenService
{
    string GenerateToken(
        string userId,
        string email,
        IEnumerable<string> roles,
        int tokenVersion);
}
