using SaasTicketing.Domain.Entities;

namespace SaasTicketing.Infrastructure.Multitenancy;

public interface ITenantProvider
{
    Tenant? CurrentTenant { get; }
    Guid? TenantId { get; }
    Task SetTenantAsync(Tenant tenant);
}
