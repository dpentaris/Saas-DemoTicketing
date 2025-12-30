# SaasTicketingDemo

A lightweight SaaS ticketing skeleton built on .NET 8 that demonstrates multi-tenant architecture, tenant-aware APIs, and core ticketing workflows such as issuing and scanning tickets.

## Solution layout
- `SaasTicketing.Api` – ASP.NET Core Web API with tenant-aware endpoints and Swagger.
- `SaasTicketing.Domain` – Core domain entities and enums with nullable reference types enabled.
- `SaasTicketing.Infrastructure` – EF Core data access, multi-tenant enforcement, save-change interception, and seed data.
- `SaasTicketing.Tests` – Unit and integration-style tests covering isolation and scanning behavior.

## Prerequisites
- .NET 8 SDK
- Docker (for local SQL Server via `docker-compose`)

## Getting started
1. Start SQL Server:
   ```bash
   docker-compose up -d
   ```
2. Restore and build (from the repository root):
   ```bash
   dotnet restore
   dotnet build SaasTicketingDemo.sln
   ```
3. Apply migrations and run the API:
   ```bash
   dotnet ef database update --project SaasTicketing.Infrastructure --startup-project SaasTicketing.Api
   dotnet run --project SaasTicketing.Api
   ```
4. Open Swagger UI at `http://localhost:5000/swagger` (default Kestrel port) and health check at `/health`.

### Tenancy & authentication
- All business endpoints are under `/t/{tenantSlug}/...` and are validated by `TenantResolutionMiddleware`.
- Demo authentication uses headers:
  - `X-Demo-UserId`: a GUID of a seeded user (e.g., `12121212-1212-1212-1212-121212121212`).
  - `X-Demo-Role`: one of `Owner|Admin|Staff|Scanner`.

### Seeded tenants and users
- `acme` (active) with owner `admin@acme.test` (`12121212-1212-1212-1212-121212121212`).
- `demoport` (active) with owner `admin@demoport.test` (`14141414-1414-1414-1414-141414141414`).
- Each tenant also has a scanner user (`scanner@...`).

### Example cURL calls
Create an event for `acme`:
```bash
curl -X POST http://localhost:5000/t/acme/events \
  -H "X-Demo-UserId: 12121212-1212-1212-1212-121212121212" \
  -H "X-Demo-Role: Admin" \
  -H "Content-Type: application/json" \
  -d '{"name":"Spring Expo","slug":"spring-expo","venue":"HQ","startsAtUtc":"2025-05-01T12:00:00Z","endsAtUtc":"2025-05-01T16:00:00Z"}'
```

Issue tickets by marking an order as paid:
```bash
curl -X POST http://localhost:5000/t/acme/orders/{orderId}/mark-paid \
  -H "X-Demo-UserId: 12121212-1212-1212-1212-121212121212" \
  -H "X-Demo-Role: Staff"
```

Scan a ticket (idempotent):
```bash
curl -X POST http://localhost:5000/t/acme/scan \
  -H "X-Demo-UserId: 13131313-1313-1313-1313-131313131313" \
  -H "X-Demo-Role: Scanner" \
  -H "Content-Type: application/json" \
  -d '{"ticketCode":"ACME-001","idempotencyKey":"scan-001"}'
```

## Tests
Run all tests:
```bash
dotnet test SaasTicketingDemo.sln
```

## Notes
- Multi-tenancy is enforced via route-based resolution, EF Core query filters, and save-change interception to prevent cross-tenant writes.
- Ticket scanning is transactional, concurrency-safe via row versions, and idempotent by `IdempotencyKey`.
- Payments are mocked; marking an order as paid issues tickets immediately.
