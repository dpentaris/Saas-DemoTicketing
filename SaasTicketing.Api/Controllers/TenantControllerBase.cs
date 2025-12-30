using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Data;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Api.Controllers;

[ApiController]
public abstract class TenantControllerBase : ControllerBase
{
    protected TenantControllerBase(TicketingDbContext dbContext, ITenantProvider tenantProvider)
    {
        DbContext = dbContext;
        TenantProvider = tenantProvider;
    }

    protected TicketingDbContext DbContext { get; }
    protected ITenantProvider TenantProvider { get; }

    protected Guid RequireTenantId()
    {
        return TenantProvider.TenantId ?? throw new InvalidOperationException("Tenant must be resolved");
    }

    protected Guid? CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    protected TenantRole? CurrentRole
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<TenantRole>(value, out var role) ? role : null;
        }
    }
}
