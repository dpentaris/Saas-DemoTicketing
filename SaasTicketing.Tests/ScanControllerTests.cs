using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaasTicketing.Api.Controllers;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Data;
using SaasTicketing.Tests;
using Xunit;

namespace SaasTicketing.Tests;

public class ScanControllerTests
{
    private static TicketingDbContext CreateContext(TestTenantProvider provider, string databaseName)
    {
        var options = new DbContextOptionsBuilder<TicketingDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        var context = new TicketingDbContext(options, provider);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    private static ScanController CreateController(TicketingDbContext context, TestTenantProvider provider)
    {
        var controller = new ScanController(context, provider)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }

    [Fact]
    public async Task DoubleScan_IsRejected()
    {
        var provider = new TestTenantProvider();
        await provider.SetTenantAsync(new Tenant
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Acme Events",
            Slug = "acme",
            Status = TenantStatus.Active,
            PlanId = Guid.Parse("33333333-3333-3333-3333-333333333333")
        });

        await using var context = CreateContext(provider, nameof(DoubleScan_IsRejected));
        var controller = CreateController(context, provider);

        var first = await controller.Scan(new ScanRequest("ACME-001", "idem-1"));
        var second = await controller.Scan(new ScanRequest("ACME-001", "idem-2"));

        Assert.IsType<OkObjectResult>(first.Result);
        var secondResult = Assert.IsType<BadRequestObjectResult>(second.Result);
        var response = Assert.IsType<ScanResponse>(secondResult.Value);
        Assert.False(response.Accepted);
    }

    [Fact]
    public async Task IdempotentScan_ReusesResult()
    {
        var provider = new TestTenantProvider();
        await provider.SetTenantAsync(new Tenant
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Acme Events",
            Slug = "acme",
            Status = TenantStatus.Active,
            PlanId = Guid.Parse("33333333-3333-3333-3333-333333333333")
        });

        await using var context = CreateContext(provider, nameof(IdempotentScan_ReusesResult));
        var controller = CreateController(context, provider);

        var first = await controller.Scan(new ScanRequest("ACME-001", "idem-same"));
        var second = await controller.Scan(new ScanRequest("ACME-001", "idem-same"));

        var firstOk = Assert.IsType<OkObjectResult>(first.Result);
        var firstResponse = Assert.IsType<ScanResponse>(firstOk.Value);
        Assert.True(firstResponse.Accepted);

        var secondOk = Assert.IsType<OkObjectResult>(second.Result);
        var secondResponse = Assert.IsType<ScanResponse>(secondOk.Value);
        Assert.True(secondResponse.Accepted);
        Assert.Equal(firstResponse.Message, secondResponse.Message);
    }

    [Fact]
    public async Task TenantIsolation_RespectsFilters()
    {
        var acmeProvider = new TestTenantProvider();
        await acmeProvider.SetTenantAsync(new Tenant
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Acme Events",
            Slug = "acme",
            Status = TenantStatus.Active,
            PlanId = Guid.Parse("33333333-3333-3333-3333-333333333333")
        });

        await using (var context = CreateContext(acmeProvider, nameof(TenantIsolation_RespectsFilters)))
        {
            var tickets = await context.Tickets.ToListAsync();
            Assert.All(tickets, t => Assert.Equal(acmeProvider.TenantId, t.TenantId));
        }

        var demoProvider = new TestTenantProvider();
        await demoProvider.SetTenantAsync(new Tenant
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "DemoPort",
            Slug = "demoport",
            Status = TenantStatus.Active,
            PlanId = Guid.Parse("44444444-4444-4444-4444-444444444444")
        });

        await using (var context = CreateContext(demoProvider, nameof(TenantIsolation_RespectsFilters)))
        {
            var tickets = await context.Tickets.ToListAsync();
            Assert.All(tickets, t => Assert.Equal(demoProvider.TenantId, t.TenantId));
        }
    }
}
