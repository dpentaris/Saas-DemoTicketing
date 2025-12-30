using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Data;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Api.Controllers;

[Route("t/{tenantSlug}/events")]
public class EventsController : TenantControllerBase
{
    public EventsController(TicketingDbContext dbContext, ITenantProvider tenantProvider) : base(dbContext, tenantProvider)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
    {
        RequireTenantId();
        var events = await DbContext.Events.AsNoTracking().ToListAsync();
        return Ok(events);
    }

    [HttpPost]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<ActionResult<Event>> Create(EventCreateRequest request)
    {
        var tenantId = RequireTenantId();
        var entity = new Event
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Slug = request.Slug,
            Venue = request.Venue,
            StartsAtUtc = request.StartsAtUtc,
            EndsAtUtc = request.EndsAtUtc,
            Status = EventStatus.Draft
        };

        DbContext.Events.Add(entity);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEvents), new { tenantSlug = HttpContext.Request.RouteValues["tenantSlug"] }, entity);
    }

    [HttpPut("{eventId}")]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<ActionResult> Update(Guid eventId, EventUpdateRequest request)
    {
        RequireTenantId();
        var entity = await DbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.Venue = request.Venue;
        entity.StartsAtUtc = request.StartsAtUtc;
        entity.EndsAtUtc = request.EndsAtUtc;

        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{eventId}/publish")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<ActionResult> Publish(Guid eventId)
    {
        RequireTenantId();
        var entity = await DbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Status = EventStatus.Published;
        DbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            Action = "EventPublished",
            EntityId = entity.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{eventId}/close")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<ActionResult> Close(Guid eventId)
    {
        RequireTenantId();
        var entity = await DbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Status = EventStatus.Closed;
        DbContext.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            Action = "EventClosed",
            EntityId = entity.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await DbContext.SaveChangesAsync();
        return NoContent();
    }
}

public record EventCreateRequest(string Name, string Slug, string Venue, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);
public record EventUpdateRequest(string Name, string Venue, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);
