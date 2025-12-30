namespace SaasTicketing.Domain.Entities;

public class AuditLog : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Data { get; set; }
}
