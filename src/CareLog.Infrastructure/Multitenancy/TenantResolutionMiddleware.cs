using System.Security.Claims;
using CareLog.Infrastructure.Multitenancy;

namespace CareLog.Api.Middleware;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, TenantContext tenant)
    {
        var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!Guid.TryParse(tenantHeader, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Missing X-Tenant-Id header");
            return;
        }

        tenant.TenantId = tenantId;
        tenant.UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        await next(context);
    }
}
