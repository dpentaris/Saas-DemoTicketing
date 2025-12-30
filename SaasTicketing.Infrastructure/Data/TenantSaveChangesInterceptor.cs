using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Infrastructure.Data;

public class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantSaveChangesInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyTenantScope(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyTenantScope(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantScope(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var tenantId = _tenantProvider.TenantId;

        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.Entity is ITenantScoped))
        {
            var scoped = (ITenantScoped)entry.Entity;
            switch (entry.State)
            {
                case EntityState.Added:
                    if (tenantId is null)
                    {
                        throw new InvalidOperationException("Tenant is required to add tenant scoped entities.");
                    }

                    if (scoped.TenantId == Guid.Empty)
                    {
                        scoped.TenantId = tenantId.Value;
                    }

                    if (scoped.TenantId != tenantId.Value)
                    {
                        throw new InvalidOperationException("Cross-tenant insert blocked.");
                    }
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    if (tenantId.HasValue && tenantId.Value != scoped.TenantId)
                    {
                        throw new InvalidOperationException("Cross-tenant write blocked.");
                    }
                    break;
            }
        }
    }
}
