using CareLog.Domain.Common;

namespace CareLog.Domain.Entities;

public sealed class ServiceCatalogItem : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DefaultChecklistJson { get; set; }
}
