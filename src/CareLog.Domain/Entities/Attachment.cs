using CareLog.Domain.Common;

namespace CareLog.Domain.Entities;

public sealed class Attachment : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid RecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
}
