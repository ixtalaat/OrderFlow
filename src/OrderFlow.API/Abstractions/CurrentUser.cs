using System.Security.Claims;
using OrderFlow.Application.Common.Identity;

namespace OrderFlow.API.Abstractions;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
