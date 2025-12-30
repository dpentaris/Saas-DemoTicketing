namespace SaasTicketing.Domain.Entities;

public class TicketType : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public bool IsActive { get; set; }

    public Event? Event { get; set; }
}
