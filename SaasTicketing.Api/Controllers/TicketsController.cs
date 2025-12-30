using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Data;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Api.Controllers;

[Route("t/{tenantSlug}/tickets")]
public class TicketsController : TenantControllerBase
{
    public TicketsController(TicketingDbContext dbContext, ITenantProvider tenantProvider) : base(dbContext, tenantProvider)
    {
    }

    [HttpGet]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<ActionResult<IEnumerable<Ticket>>> List([FromQuery] Guid? eventId, [FromQuery] TicketStatus? status)
    {
        RequireTenantId();
        var query = DbContext.Tickets.AsNoTracking().AsQueryable();
        if (eventId.HasValue)
        {
            query = query.Where(t => t.EventId == eventId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var results = await query.ToListAsync();
        return Ok(results);
    }

    [HttpPost("{ticketId}/void")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<ActionResult> Void(Guid ticketId)
    {
        RequireTenantId();
        var ticket = await DbContext.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null)
        {
            return NotFound();
        }

        ticket.Status = TicketStatus.Voided;
        ticket.UsedAt = null;
        DbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = ticket.TenantId,
            Action = "TicketVoided",
            EntityId = ticket.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await DbContext.SaveChangesAsync();
        return NoContent();
    }
}
