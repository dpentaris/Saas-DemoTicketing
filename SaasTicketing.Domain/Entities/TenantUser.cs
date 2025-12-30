namespace SaasTicketing.Domain.Entities;

public class TenantUser : ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public TenantRole Role { get; set; }

    public Tenant? Tenant { get; set; }
    public User? User { get; set; }
}
