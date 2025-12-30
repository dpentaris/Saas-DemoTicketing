namespace SaasTicketing.Domain.Entities;

public class Order : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EventId { get; set; }
    public Guid TicketTypeId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public bool Paid { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string PurchaserEmail { get; set; } = string.Empty;

    public Event? Event { get; set; }
    public TicketType? TicketType { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
