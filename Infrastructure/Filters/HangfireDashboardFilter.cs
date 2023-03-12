using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Hangfire.API.Infrastructure.Filters
{
    public class HangfireDashboardFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            //if (context is not null)
            //{
            //    var httpContext = context.GetHttpContext();
            //    return httpContext.User.Identity.IsAuthenticated;
            //}
            //else
            //{
            //    return false;
            //}
            return true;
        }
    }
}
