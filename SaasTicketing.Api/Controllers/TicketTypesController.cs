using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Data;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Api.Controllers;

[Route("t/{tenantSlug}/events/{eventId}/ticket-types")]
public class TicketTypesController : TenantControllerBase
{
    public TicketTypesController(TicketingDbContext dbContext, ITenantProvider tenantProvider) : base(dbContext, tenantProvider)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TicketType>>> List(Guid eventId)
    {
        RequireTenantId();
        var items = await DbContext.TicketTypes.Where(t => t.EventId == eventId).ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<ActionResult<TicketType>> Create(Guid eventId, TicketTypeRequest request)
    {
        var tenantId = RequireTenantId();
        var entity = new TicketType
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventId = eventId,
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity,
            IsActive = request.IsActive
        };
        DbContext.TicketTypes.Add(entity);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(List), new { tenantSlug = HttpContext.Request.RouteValues["tenantSlug"], eventId }, entity);
    }

    [HttpPut("{ticketTypeId}")]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<ActionResult> Update(Guid eventId, Guid ticketTypeId, TicketTypeRequest request)
    {
        RequireTenantId();
        var entity = await DbContext.TicketTypes.FirstOrDefaultAsync(t => t.Id == ticketTypeId && t.EventId == eventId);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.Price = request.Price;
        entity.Quantity = request.Quantity;
        entity.IsActive = request.IsActive;
        await DbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{ticketTypeId}")]
    [Authorize(Policy = "OwnerOrAdmin")]
    public async Task<ActionResult> Delete(Guid eventId, Guid ticketTypeId)
    {
        RequireTenantId();
        var entity = await DbContext.TicketTypes.FirstOrDefaultAsync(t => t.Id == ticketTypeId && t.EventId == eventId);
        if (entity is null)
        {
            return NotFound();
        }

        DbContext.TicketTypes.Remove(entity);
        await DbContext.SaveChangesAsync();
        return NoContent();
    }
}

public record TicketTypeRequest(string Name, decimal Price, int Quantity, bool IsActive);
