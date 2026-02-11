using CareLog.Domain.Entities;
using CareLog.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CareLog.Infrastructure.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(AppDbContext db, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        await db.Database.MigrateAsync();
        foreach (var role in new[] { UserRole.Admin, UserRole.Coordinator, UserRole.Nurse, UserRole.BackOffice, UserRole.ReadOnly })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        }

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Slug == "demo");
        if (tenant is null)
        {
            tenant = new Tenant { Name = "Demo Care", Slug = "demo" };
            db.Tenants.Add(tenant);
            db.ServiceCatalogItems.AddRange(
                new ServiceCatalogItem { TenantId = tenant.Id, Name = "Wound dressing" },
                new ServiceCatalogItem { TenantId = tenant.Id, Name = "Injection" },
                new ServiceCatalogItem { TenantId = tenant.Id, Name = "Vitals check" });
            await db.SaveChangesAsync();
        }

        var admin = await users.FindByEmailAsync("admin@demo.local");
        if (admin is null)
        {
            admin = new ApplicationUser { UserName = "admin@demo.local", Email = "admin@demo.local", FullName = "Demo Admin", TenantId = tenant.Id };
            await users.CreateAsync(admin, "Passw0rd!");
            await users.AddToRolesAsync(admin, [UserRole.Admin, UserRole.Coordinator]);
        }
    }
}
