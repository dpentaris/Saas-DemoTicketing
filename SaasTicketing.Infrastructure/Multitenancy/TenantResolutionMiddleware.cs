using Microsoft.EntityFrameworkCore;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Data;

namespace SaasTicketing.Infrastructure.Multitenancy;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TicketingDbContext dbContext, ITenantProvider tenantProvider)
    {
        var slug = context.Request.RouteValues.TryGetValue("tenantSlug", out var slugValue)
            ? slugValue?.ToString()
            : null;

        if (string.IsNullOrWhiteSpace(slug))
        {
            await _next(context);
            return;
        }

        var tenant = await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug);
        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Tenant not found");
            return;
        }

        if (tenant.Status != TenantStatus.Active)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Tenant is not active");
            return;
        }

        await tenantProvider.SetTenantAsync(tenant);
        await _next(context);
    }
}
