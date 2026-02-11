using CareLog.Domain.Common;

namespace CareLog.Domain.Entities;

public sealed class AuditEvent : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public string DiffSummary { get; set; } = string.Empty;
}
