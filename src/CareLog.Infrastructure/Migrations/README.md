Run migrations from repo root:

```bash
dotnet ef migrations add InitialCreate --project src/CareLog.Infrastructure --startup-project src/CareLog.Api
dotnet ef database update --project src/CareLog.Infrastructure --startup-project src/CareLog.Api
```
