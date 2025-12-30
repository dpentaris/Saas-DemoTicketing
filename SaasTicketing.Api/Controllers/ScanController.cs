using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Data;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Api.Controllers;

[Route("t/{tenantSlug}/scan")]
public class ScanController : TenantControllerBase
{
    public ScanController(TicketingDbContext dbContext, ITenantProvider tenantProvider) : base(dbContext, tenantProvider)
    {
    }

    [HttpPost]
    [Authorize(Policy = "Scanner")]
    public async Task<ActionResult<ScanResponse>> Scan(ScanRequest request)
    {
        var tenantId = RequireTenantId();

        var existing = await DbContext.Scans.AsNoTracking().FirstOrDefaultAsync(s => s.IdempotencyKey == request.IdempotencyKey && s.TenantId == tenantId);
        if (existing is not null)
        {
            return Ok(new ScanResponse(existing.Accepted, existing.Reason ?? string.Empty));
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync();

        var ticket = await DbContext.Tickets.Include(t => t.Event).FirstOrDefaultAsync(t => t.Code == request.TicketCode);
        if (ticket is null)
        {
            await RecordScan(tenantId, null, request.IdempotencyKey, false, "Ticket not found");
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return BadRequest(new ScanResponse(false, "Ticket not found"));
        }

        if (ticket.Status == TicketStatus.Voided)
        {
            await RecordScan(tenantId, ticket.Id, request.IdempotencyKey, false, "Ticket voided");
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return BadRequest(new ScanResponse(false, "Ticket voided"));
        }

        if (ticket.Status == TicketStatus.Used)
        {
            await RecordScan(tenantId, ticket.Id, request.IdempotencyKey, false, "Ticket already used");
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return BadRequest(new ScanResponse(false, "Ticket already used"));
        }

        if (ticket.Event is null || ticket.Event.Status != EventStatus.Published)
        {
            await RecordScan(tenantId, ticket.Id, request.IdempotencyKey, false, "Event not open for scanning");
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return BadRequest(new ScanResponse(false, "Event not open for scanning"));
        }

        ticket.Status = TicketStatus.Used;
        ticket.UsedAt = DateTimeOffset.UtcNow;

        await RecordScan(tenantId, ticket.Id, request.IdempotencyKey, true, "OK");
        DbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Action = "TicketUsed",
            EntityId = ticket.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            Data = request.IdempotencyKey
        });

        await DbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(new ScanResponse(true, "OK"));
    }

    private Task RecordScan(Guid tenantId, Guid? ticketId, string idempotencyKey, bool accepted, string reason)
    {
        DbContext.Scans.Add(new Scan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TicketId = ticketId,
            IdempotencyKey = idempotencyKey,
            ScannedAt = DateTimeOffset.UtcNow,
            Accepted = accepted,
            Reason = reason
        });
        return Task.CompletedTask;
    }
}

public record ScanRequest(string TicketCode, string IdempotencyKey);
public record ScanResponse(bool Accepted, string Message);
