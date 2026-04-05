using Hangfire.Dashboard;

namespace OrdemCerta.Presentation.Filters;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    // IPs confiáveis que podem acessar o dashboard sem JWT
    private static readonly string[] TrustedIps = ["127.0.0.1", "::1", "172.18.0.1"];

    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();

        var remoteIp = http.Connection.RemoteIpAddress?.ToString();
        if (remoteIp != null && TrustedIps.Contains(remoteIp))
            return true;

        return http.User.Identity?.IsAuthenticated == true
            && http.User.IsInRole("Admin");
    }
}
