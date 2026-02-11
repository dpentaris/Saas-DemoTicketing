using CareLog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareLog.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<Visit> Visits { get; }
    DbSet<Record> Records { get; }
    DbSet<ServiceCatalogItem> ServiceCatalogItems { get; }
    DbSet<Attachment> Attachments { get; }
    DbSet<AuditEvent> AuditEvents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
