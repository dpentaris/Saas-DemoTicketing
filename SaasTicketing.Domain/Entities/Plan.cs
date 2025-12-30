namespace SaasTicketing.Domain.Entities;

public class Plan : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxEvents { get; set; }

    public Tenant? Tenant { get; set; }
}
