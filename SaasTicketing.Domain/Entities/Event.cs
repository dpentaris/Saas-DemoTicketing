namespace SaasTicketing.Domain.Entities;

public class Event : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public EventStatus Status { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
}
