using System.Security.Claims;
using CareLog.Api.Middleware;
using CareLog.Application.Common;
using CareLog.Application.Records;
using CareLog.Application.Visits;
using CareLog.Infrastructure.Data;
using CareLog.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapPost("/api/auth/login", async (
    LoginRequest req,
    UserManager<CareLog.Domain.Entities.ApplicationUser> users,
    SignInManager<CareLog.Domain.Entities.ApplicationUser> signIn,
    AppDbContext db,
    CareLog.Application.Abstractions.IJwtTokenService jwt) =>
{
    var user = await users.FindByEmailAsync(req.Email);
    if (user is null) return Results.Unauthorized();
    var ok = await signIn.CheckPasswordSignInAsync(user, req.Password, false);
    if (!ok.Succeeded) return Results.Unauthorized();
    var roles = await users.GetRolesAsync(user);
    var access = jwt.CreateAccessToken(user.Id, user.TenantId, roles);
    var refresh = jwt.CreateRefreshToken();
    db.RefreshTokens.Add(new() { TenantId = user.TenantId, UserId = user.Id, Token = refresh, ExpiresAt = DateTimeOffset.UtcNow.AddDays(14) });
    await db.SaveChangesAsync();
    return Results.Ok(new { access_token = access, refresh_token = refresh, tenant_id = user.TenantId });
});

app.MapPost("/api/auth/refresh", async (RefreshRequest req, AppDbContext db, CareLog.Application.Abstractions.IJwtTokenService jwt, UserManager<CareLog.Domain.Entities.ApplicationUser> users) =>
{
    var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == req.RefreshToken && !x.Revoked && x.ExpiresAt > DateTimeOffset.UtcNow);
    if (token is null) return Results.Unauthorized();
    var user = await users.FindByIdAsync(token.UserId);
    if (user is null) return Results.Unauthorized();
    var roles = await users.GetRolesAsync(user);
    return Results.Ok(new { access_token = jwt.CreateAccessToken(user.Id, user.TenantId, roles) });
});

var api = app.MapGroup("/api").RequireAuthorization();
api.MapGet("/patients", async (AppDbContext db) => await db.Patients.OrderBy(p => p.LastName).ToListAsync());
api.MapPost("/patients", [Authorize(Roles = "Admin,Coordinator,Nurse")] async (CareLog.Domain.Entities.Patient patient, AppDbContext db) =>
{
    db.Patients.Add(patient);
    await db.SaveChangesAsync();
    return Results.Created($"/api/patients/{patient.Id}", patient);
});

api.MapGet("/visits/week", async (DateTimeOffset start, AppDbContext db) =>
{
    var end = start.AddDays(7);
    return await db.Visits.Where(v => v.ScheduledStart >= start && v.ScheduledStart < end).ToListAsync();
});

api.MapPost("/visits", [Authorize(Roles = "Admin,Coordinator")] async (CreateVisitCommand cmd, IMediator mediator)
    => Results.Ok(await mediator.Send(cmd)));

api.MapPost("/visits/{id:guid}/check-in", [Authorize(Roles = "Nurse,Coordinator,Admin")] async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
{
    var visit = await db.Visits.FindAsync(id);
    if (visit is null) return Results.NotFound();
    visit.CheckIn(DateTimeOffset.UtcNow);
    db.AuditEvents.Add(new() { TenantId = visit.TenantId, EntityName = "Visit", EntityId = id, Action = "checkin", UserId = user.Identity?.Name ?? "unknown", DiffSummary = "checked in" });
    await db.SaveChangesAsync();
    return Results.Ok(visit);
});

api.MapPost("/records/upsert", [Authorize(Roles = "Nurse,Coordinator,Admin")] async (UpsertRecordCommand cmd, IMediator mediator)
    => Results.Ok(await mediator.Send(cmd)));

api.MapPost("/attachments", [Authorize(Roles = "Nurse,Coordinator,Admin")] async (IFormFile file, Guid recordId, AppDbContext db, CareLog.Application.Abstractions.IFileStorage storage) =>
{
    if (file.ContentType is not ("image/jpeg" or "image/png" or "application/pdf")) return Results.BadRequest("Only photo/PDF");
    await using var stream = file.OpenReadStream();
    var path = await storage.SaveAsync(stream, file.FileName, file.ContentType, default);
    var attachment = new CareLog.Domain.Entities.Attachment { RecordId = recordId, FileName = file.FileName, ContentType = file.ContentType, StoragePath = path };
    db.Attachments.Add(attachment);
    await db.SaveChangesAsync();
    return Results.Ok(attachment);
});

api.MapPost("/sync", [Authorize] async (CareLog.Application.Sync.SyncRequest request) =>
{
    // placeholder endpoint for mobile outbox sync ingest/ack
    var results = request.OutboxItems.Select(x => new CareLog.Application.Sync.SyncResult(x.LocalId, true, null)).ToList();
    return Results.Ok(results);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var users = scope.ServiceProvider.GetRequiredService<UserManager<CareLog.Domain.Entities.ApplicationUser>>();
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.EnsureSeededAsync(db, users, roles);
}

app.Run();

public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);

public partial class Program;
