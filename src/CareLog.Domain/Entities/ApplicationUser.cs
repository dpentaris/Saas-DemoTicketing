using Microsoft.AspNetCore.Identity;

namespace CareLog.Domain.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
}
