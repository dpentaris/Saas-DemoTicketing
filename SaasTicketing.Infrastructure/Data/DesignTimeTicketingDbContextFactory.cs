using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Infrastructure.Data;

public class DesignTimeTicketingDbContextFactory : IDesignTimeDbContextFactory<TicketingDbContext>
{
    public TicketingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TicketingDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=SaasTicketing;User Id=sa;Password=Your_strong_password_!;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        return new TicketingDbContext(optionsBuilder.Options, new TenantProvider());
    }
}
