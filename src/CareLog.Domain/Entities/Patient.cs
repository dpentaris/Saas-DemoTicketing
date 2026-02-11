using CareLog.Domain.Common;

namespace CareLog.Domain.Entities;

public sealed class Patient : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Allergies { get; set; } = string.Empty;
    public string EmergencyContact { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
