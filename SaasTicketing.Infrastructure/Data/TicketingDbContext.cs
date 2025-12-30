using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SaasTicketing.Domain.Entities;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Infrastructure.Data;

public class TicketingDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public TicketingDbContext(DbContextOptions<TicketingDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Scan> Scans => Set<Scan>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    private Guid? CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureModel(modelBuilder);
        ApplyTenantFilters(modelBuilder);
        Seed(modelBuilder);
    }

    private void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Slug).IsRequired().HasMaxLength(64);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(128);
            entity.HasIndex(t => t.Slug).IsUnique();
        });

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(64);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(128);
        });

        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.HasKey(tu => new { tu.TenantId, tu.UserId });
            entity.Property(tu => tu.Role).IsRequired();
            entity.HasOne(tu => tu.Tenant)
                .WithMany()
                .HasForeignKey(tu => tu.TenantId);
            entity.HasOne(tu => tu.User)
                .WithMany(u => u.Tenants)
                .HasForeignKey(tu => tu.UserId);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Venue).HasMaxLength(256);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Events)
                .HasForeignKey(e => e.TenantId);
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasKey(tt => tt.Id);
            entity.Property(tt => tt.Name).IsRequired().HasMaxLength(128);
            entity.Property(tt => tt.Price).HasPrecision(18, 2);
            entity.HasOne(tt => tt.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(tt => tt.EventId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Total).HasPrecision(18, 2);
            entity.Property(o => o.PurchaserEmail).HasMaxLength(256);
            entity.HasOne(o => o.Event)
                .WithMany()
                .HasForeignKey(o => o.EventId);
            entity.HasOne(o => o.TicketType)
                .WithMany()
                .HasForeignKey(o => o.TicketTypeId);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Code).IsRequired().HasMaxLength(64);
            entity.Property(t => t.RowVersion).IsRowVersion();
            entity.HasIndex(t => t.Code).IsUnique();
            entity.HasOne(t => t.Event)
                .WithMany()
                .HasForeignKey(t => t.EventId);
            entity.HasOne(t => t.Order)
                .WithMany(o => o.Tickets)
                .HasForeignKey(t => t.OrderId);
            entity.HasOne(t => t.TicketType)
                .WithMany()
                .HasForeignKey(t => t.TicketTypeId);
        });

        modelBuilder.Entity<Scan>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.IdempotencyKey).IsRequired().HasMaxLength(128);
            entity.HasIndex(s => new { s.TenantId, s.IdempotencyKey }).IsUnique();
            entity.Property(s => s.TicketId).IsRequired(false);
            entity.HasOne(s => s.Ticket)
                .WithMany(t => t.Scans)
                .HasForeignKey(s => s.TicketId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(64);
            entity.Property(a => a.Data).HasMaxLength(2048);
        });
    }

    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        var tenantScopedTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(ITenantScoped).IsAssignableFrom(t.ClrType));

        foreach (var entityType in tenantScopedTypes)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var hasTenant = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
            var hasValue = Expression.Property(hasTenant, nameof(Nullable<Guid>.HasValue));
            var tenantIdProperty = Expression.Property(hasTenant, nameof(Nullable<Guid>.Value));
            var tenantProperty = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
            var equals = Expression.Equal(tenantProperty, tenantIdProperty);
            var body = Expression.AndAlso(hasValue, equals);
            var lambda = Expression.Lambda(body, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    private void Seed(ModelBuilder modelBuilder)
    {
        var acmeTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var demoTenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var acmePlanId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var demoPlanId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Tenant>().HasData(
            new Tenant { Id = acmeTenantId, Name = "Acme Events", Slug = "acme", Status = TenantStatus.Active, PlanId = acmePlanId },
            new Tenant { Id = demoTenantId, Name = "DemoPort", Slug = "demoport", Status = TenantStatus.Active, PlanId = demoPlanId }
        );

        modelBuilder.Entity<Plan>().HasData(
            new Plan { Id = acmePlanId, TenantId = acmeTenantId, Name = "Standard", Price = 0, MaxEvents = 10 },
            new Plan { Id = demoPlanId, TenantId = demoTenantId, Name = "Standard", Price = 0, MaxEvents = 10 }
        );

        var acmeEventId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var demoEventId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var now = new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        modelBuilder.Entity<Event>().HasData(
            new Event
            {
                Id = acmeEventId,
                TenantId = acmeTenantId,
                Name = "Acme Launch",
                Slug = "acme-launch",
                Venue = "Acme HQ",
                StartsAtUtc = now.AddDays(7),
                EndsAtUtc = now.AddDays(7).AddHours(4),
                Status = EventStatus.Published
            },
            new Event
            {
                Id = demoEventId,
                TenantId = demoTenantId,
                Name = "DemoPort Live",
                Slug = "demoport-live",
                Venue = "Downtown Hall",
                StartsAtUtc = now.AddDays(10),
                EndsAtUtc = now.AddDays(10).AddHours(3),
                Status = EventStatus.Published
            }
        );

        var acmeTicketTypeId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var demoTicketTypeId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        modelBuilder.Entity<TicketType>().HasData(
            new TicketType { Id = acmeTicketTypeId, TenantId = acmeTenantId, EventId = acmeEventId, Name = "General", Price = 49, Quantity = 100, IsActive = true },
            new TicketType { Id = demoTicketTypeId, TenantId = demoTenantId, EventId = demoEventId, Name = "VIP", Price = 99, Quantity = 50, IsActive = true }
        );

        var acmeOrderId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var demoOrderId = Guid.Parse("88888888-8888-8888-8888-888888888888");

        modelBuilder.Entity<Order>().HasData(
            new Order { Id = acmeOrderId, TenantId = acmeTenantId, EventId = acmeEventId, TicketTypeId = acmeTicketTypeId, Quantity = 2, Total = 98, Paid = true, CreatedAt = now.AddDays(-1), PurchaserEmail = "owner@acme.test" },
            new Order { Id = demoOrderId, TenantId = demoTenantId, EventId = demoEventId, TicketTypeId = demoTicketTypeId, Quantity = 1, Total = 99, Paid = true, CreatedAt = now.AddDays(-1), PurchaserEmail = "admin@demoport.test" }
        );

        var acmeTicketId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var demoTicketId = Guid.Parse("01010101-0101-0101-0101-010101010101");

        modelBuilder.Entity<Ticket>().HasData(
            new Ticket { Id = acmeTicketId, TenantId = acmeTenantId, EventId = acmeEventId, OrderId = acmeOrderId, TicketTypeId = acmeTicketTypeId, Code = "ACME-001", Status = TicketStatus.Issued, IssuedAt = now },
            new Ticket { Id = demoTicketId, TenantId = demoTenantId, EventId = demoEventId, OrderId = demoOrderId, TicketTypeId = demoTicketTypeId, Code = "DEMOPORT-001", Status = TicketStatus.Issued, IssuedAt = now }
        );

        var acmeAdminId = Guid.Parse("12121212-1212-1212-1212-121212121212");
        var acmeScannerId = Guid.Parse("13131313-1313-1313-1313-131313131313");
        var demoAdminId = Guid.Parse("14141414-1414-1414-1414-141414141414");
        var demoScannerId = Guid.Parse("15151515-1515-1515-1515-151515151515");

        modelBuilder.Entity<User>().HasData(
            new User { Id = acmeAdminId, Email = "admin@acme.test", DisplayName = "Acme Admin" },
            new User { Id = acmeScannerId, Email = "scanner@acme.test", DisplayName = "Acme Scanner" },
            new User { Id = demoAdminId, Email = "admin@demoport.test", DisplayName = "DemoPort Admin" },
            new User { Id = demoScannerId, Email = "scanner@demoport.test", DisplayName = "DemoPort Scanner" }
        );

        modelBuilder.Entity<TenantUser>().HasData(
            new TenantUser { TenantId = acmeTenantId, UserId = acmeAdminId, Role = TenantRole.Owner },
            new TenantUser { TenantId = acmeTenantId, UserId = acmeScannerId, Role = TenantRole.Scanner },
            new TenantUser { TenantId = demoTenantId, UserId = demoAdminId, Role = TenantRole.Owner },
            new TenantUser { TenantId = demoTenantId, UserId = demoScannerId, Role = TenantRole.Scanner }
        );
    }
}
