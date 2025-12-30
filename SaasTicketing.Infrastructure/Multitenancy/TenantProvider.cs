using SaasTicketing.Domain.Entities;

namespace SaasTicketing.Infrastructure.Multitenancy;

public class TenantProvider : ITenantProvider
{
    private Tenant? _tenant;

    public Tenant? CurrentTenant => _tenant;
    public Guid? TenantId => _tenant?.Id;

    public Task SetTenantAsync(Tenant tenant)
    {
        _tenant = tenant;
        return Task.CompletedTask;
    }
}
