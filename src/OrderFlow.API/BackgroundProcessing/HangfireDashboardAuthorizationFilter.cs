using Hangfire.Dashboard;
using OrderFlow.Application.Common.Constants;

namespace OrderFlow.API.BackgroundProcessing;

public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true &&
            (httpContext.User.IsInRole(Roles.Admin) || httpContext.User.IsInRole(Roles.SalesEmployee));
    }
}
