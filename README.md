# CareLog Monorepo

CareLog is a multi-tenant SaaS for home care field service operations.

## Architecture

- `src/CareLog.Domain`: entities + enums.
- `src/CareLog.Application`: MediatR commands/queries + validation + abstractions.
- `src/CareLog.Infrastructure`: EF Core PostgreSQL, Identity, JWT, multi-tenancy filters, storage provider, seed data.
- `src/CareLog.Api`: ASP.NET Core API (JWT auth, refresh token flow, tenant middleware, sync endpoint, Swagger).
- `src/CareLog.Web`: Blazor Server admin portal (calendar/patients/reports pages).
- `src/CareLog.Mobile`: .NET MAUI app (MVVM, offline SQLite outbox + sync retry backoff).
- `tests/*`: domain and integration tests.

## Multi-tenancy model

Shared database with `TenantId` on tenant scoped tables.

Enforcement layers:
1. `TenantResolutionMiddleware` reads `X-Tenant-Id` and sets scoped tenant context.
2. `AppDbContext` global query filters apply `TenantId == CurrentTenant` on all `ITenantScoped` entities.
3. Save pipeline auto-stamps missing `TenantId` values.

## Auth & RBAC

- ASP.NET Core Identity for users and role management.
- JWT access token + refresh token persistence (`RefreshTokens` table).
- Roles:
  - Admin
  - Coordinator
  - Nurse
  - BackOffice
  - ReadOnly

## Offline sync strategy (mobile)

- Records created offline are stored locally in SQLite outbox.
- Background sync posts outbox payloads to `/api/sync`.
- Retry uses exponential backoff (`2^retry`, capped at 60s).
- Conflict policy for records: last-write-wins with conflict warning returned to client and audit event written.

## Attachments

- File upload endpoint supports photo (`image/jpeg`, `image/png`) and PDFs.
- Local provider stores in API filesystem (`attachments/` folder).
- `IFileStorage` abstraction allows S3/Blob replacement later.

## Audit logging

`AuditEvents` table stores immutable event rows for sensitive operations with:
- who
- when
- tenant
- entity
- entity id
- diff summary

## Run with Docker Compose

```bash
./scripts/dev-up.sh
```

This starts:
- PostgreSQL on `5432`
- API on `8080`
- Web on `8081`

Stop:

```bash
./scripts/dev-down.sh
```

## Local development

```bash
# API
dotnet run --project src/CareLog.Api

# Web
dotnet run --project src/CareLog.Web

# Mobile (example)
dotnet build src/CareLog.Mobile
```

## EF Core migrations

```bash
dotnet ef migrations add InitialCreate --project src/CareLog.Infrastructure --startup-project src/CareLog.Api
dotnet ef database update --project src/CareLog.Infrastructure --startup-project src/CareLog.Api
```

## API docs and samples

- Swagger: `http://localhost:8080/swagger`
- HTTP request samples: `CareLog.http`

## Demo seeded credentials

- Email: `admin@demo.local`
- Password: `Passw0rd!`

## Notes

- This repo is structured for production evolution, but in this environment runtime verification is limited if .NET SDK is unavailable.
- Integration test contains a placeholder pattern for tenant isolation fixture wiring.
