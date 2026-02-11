using CareLog.Domain.Common;

namespace CareLog.Domain.Entities;

public sealed class RefreshToken : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool Revoked { get; set; }
}
