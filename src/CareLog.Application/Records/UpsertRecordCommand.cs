using CareLog.Application.Abstractions;
using CareLog.Domain.Entities;
using MediatR;

namespace CareLog.Application.Records;

public sealed record UpsertRecordCommand(Guid? RecordId, Guid VisitId, string ServicesJson, string VitalsJson, string Notes, long ClientVersion)
    : IRequest<(Guid Id, bool Conflict, string Warning)>;

public sealed class UpsertRecordHandler(IAppDbContext db, ITenantContext tenant) : IRequestHandler<UpsertRecordCommand, (Guid Id, bool Conflict, string Warning)>
{
    public async Task<(Guid Id, bool Conflict, string Warning)> Handle(UpsertRecordCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var conflict = false;
        var warning = string.Empty;

        Record record;
        if (request.RecordId.HasValue)
        {
            record = await db.Records.FindAsync([request.RecordId.Value], cancellationToken) ?? throw new InvalidOperationException("Record missing");
            if (record.Version > request.ClientVersion)
            {
                conflict = true;
                warning = "Server had newer data. Applied last-write-wins and audit logged.";
            }
            record.SelectedServicesJson = request.ServicesJson;
            record.VitalsJson = request.VitalsJson;
            record.Notes = request.Notes;
            record.Version++;
            record.UpdatedAt = now;
        }
        else
        {
            record = new Record
            {
                TenantId = tenant.TenantId,
                VisitId = request.VisitId,
                SelectedServicesJson = request.ServicesJson,
                VitalsJson = request.VitalsJson,
                Notes = request.Notes,
                Version = 1
            };
            db.Records.Add(record);
        }

        db.AuditEvents.Add(new AuditEvent
        {
            TenantId = tenant.TenantId,
            EntityId = record.Id,
            EntityName = nameof(Record),
            Action = request.RecordId.HasValue ? "update" : "create",
            UserId = tenant.UserId,
            DiffSummary = $"version={record.Version}; conflict={conflict}"
        });

        await db.SaveChangesAsync(cancellationToken);
        return (record.Id, conflict, warning);
    }
}
