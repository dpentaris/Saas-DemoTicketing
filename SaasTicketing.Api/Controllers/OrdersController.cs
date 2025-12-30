using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Data;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Api.Controllers;

[Route("t/{tenantSlug}/orders")]
public class OrdersController : TenantControllerBase
{
    public OrdersController(TicketingDbContext dbContext, ITenantProvider tenantProvider) : base(dbContext, tenantProvider)
    {
    }

    [HttpPost]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<ActionResult<Order>> Create(OrderCreateRequest request)
    {
        var tenantId = RequireTenantId();
        var evt = await DbContext.Events.FirstOrDefaultAsync(e => e.Id == request.EventId);
        if (evt is null || evt.Status == EventStatus.Closed)
        {
            return BadRequest("Event unavailable");
        }

        var ticketType = await DbContext.TicketTypes.FirstOrDefaultAsync(t => t.Id == request.TicketTypeId && t.EventId == request.EventId);
        if (ticketType is null || !ticketType.IsActive)
        {
            return BadRequest("Ticket type unavailable");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventId = request.EventId,
            TicketTypeId = request.TicketTypeId,
            Quantity = request.Quantity,
            PurchaserEmail = request.Email,
            Total = ticketType.Price * request.Quantity,
            Paid = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Create), new { tenantSlug = HttpContext.Request.RouteValues["tenantSlug"] }, order);
    }

    [HttpPost("{orderId}/mark-paid")]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<ActionResult<IEnumerable<Ticket>>> MarkPaid(Guid orderId)
    {
        var tenantId = RequireTenantId();
        var order = await DbContext.Orders.Include(o => o.TicketType).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return NotFound();
        }

        if (order.Paid)
        {
            var existing = await DbContext.Tickets.Where(t => t.OrderId == order.Id).ToListAsync();
            return Ok(existing);
        }

        order.Paid = true;
        var issuedTickets = new List<Ticket>();
        for (var i = 0; i < order.Quantity; i++)
        {
            var code = $"{TenantProvider.CurrentTenant?.Slug.ToUpperInvariant()}-{order.Id.ToString().Split('-').First()}-{i + 1:000}";
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                EventId = order.EventId,
                OrderId = order.Id,
                TicketTypeId = order.TicketTypeId,
                Code = code,
                Status = TicketStatus.Issued,
                IssuedAt = DateTimeOffset.UtcNow
            };
            issuedTickets.Add(ticket);
            DbContext.Tickets.Add(ticket);
            DbContext.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Action = "TicketIssued",
                EntityId = ticket.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                Data = order.PurchaserEmail
            });
        }

        await DbContext.SaveChangesAsync();
        return Ok(issuedTickets);
    }
}

public record OrderCreateRequest(Guid EventId, Guid TicketTypeId, int Quantity, string Email);
