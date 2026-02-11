using CareLog.Domain.Common;

namespace CareLog.Domain.Entities;

public sealed class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
