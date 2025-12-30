using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using SaasTicketing.Api.Security;
using SaasTicketing.Infrastructure.Extensions;
using SaasTicketing.Infrastructure.Multitenancy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(DemoAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(DemoAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnerOrAdmin", policy => policy.RequireRole("Owner", "Admin"));
    options.AddPolicy("StaffOrAbove", policy => policy.RequireRole("Owner", "Admin", "Staff"));
    options.AddPolicy("Scanner", policy => policy.RequireRole("Owner", "Admin", "Staff", "Scanner"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();
