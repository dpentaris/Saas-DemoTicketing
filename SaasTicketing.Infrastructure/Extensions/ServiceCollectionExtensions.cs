using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaasTicketing.Infrastructure.Data;
using SaasTicketing.Infrastructure.Multitenancy;

namespace SaasTicketing.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<TenantSaveChangesInterceptor>();

        services.AddDbContext<TicketingDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost,1433;Database=SaasTicketing;User Id=sa;Password=Your_strong_password_!;TrustServerCertificate=True";
            options.UseSqlServer(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<TenantSaveChangesInterceptor>());
        });

        services.AddHealthChecks();
        return services;
    }
}
