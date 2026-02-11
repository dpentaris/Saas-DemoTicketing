using CareLog.Domain.Common;

namespace CareLog.Domain.Entities;

public sealed class Record : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid VisitId { get; set; }
    public string SelectedServicesJson { get; set; } = "[]";
    public string VitalsJson { get; set; } = "{}";
    public string Notes { get; set; } = string.Empty;
    public string? SignaturePath { get; set; }
    public long Version { get; set; }
}
