namespace SaasTicketing.Domain.Entities;

public class Ticket : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Event? Event { get; set; }
    public Order? Order { get; set; }
    public TicketType? TicketType { get; set; }
    public ICollection<Scan> Scans { get; set; } = new List<Scan>();
}
