using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Tests;

public class TestTenantProvider : ITenantProvider
{
    public Tenant? CurrentTenant { get; private set; }
    public Guid? TenantId => CurrentTenant?.Id;

    public Task SetTenantAsync(Tenant tenant)
    {
        CurrentTenant = tenant;
        return Task.CompletedTask;
    }
}
