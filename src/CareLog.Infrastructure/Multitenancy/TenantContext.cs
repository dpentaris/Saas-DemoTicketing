using CareLog.Application.Abstractions;

namespace CareLog.Infrastructure.Multitenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; internal set; }
    public string UserId { get; internal set; } = "system";
}
