namespace SaasTicketing.Domain.Entities;

public class Scan : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? TicketId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTimeOffset ScannedAt { get; set; }
    public bool Accepted { get; set; }
    public string? Reason { get; set; }

    public Ticket? Ticket { get; set; }
}
