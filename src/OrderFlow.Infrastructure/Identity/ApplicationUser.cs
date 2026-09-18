using Microsoft.AspNetCore.Identity;

namespace OrderFlow.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Bumped to invalidate all previously issued access tokens
    /// (deactivation, refresh-token reuse). Compared against the
    /// <c>token_version</c> claim during JWT validation.
    /// </summary>
    public int TokenVersion { get; set; }
}
