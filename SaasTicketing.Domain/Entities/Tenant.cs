namespace SaasTicketing.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TenantStatus Status { get; set; }
    public Guid PlanId { get; set; }

    public Plan? Plan { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
