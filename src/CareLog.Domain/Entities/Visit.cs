using CareLog.Domain.Common;
using CareLog.Domain.Enums;

namespace CareLog.Domain.Entities;

public sealed class Visit : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? AssignedNurseId { get; set; }
    public DateTimeOffset ScheduledStart { get; set; }
    public DateTimeOffset ScheduledEnd { get; set; }
    public string Address { get; set; } = string.Empty;
    public VisitStatus Status { get; set; } = VisitStatus.Planned;
    public DateTimeOffset? CheckInAt { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public void CheckIn(DateTimeOffset at)
    {
        Status = VisitStatus.InProgress;
        CheckInAt = at;
        UpdatedAt = at;
    }

    public void CheckOut(DateTimeOffset at)
    {
        Status = VisitStatus.Done;
        CheckOutAt = at;
        UpdatedAt = at;
    }
}
