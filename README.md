# CareFlux Platform

CareFlux is a Home Care SaaS multi-platform starter built with .NET and .NET MAUI.

## Repository layout

- `src/backend` - Backend services and core layers (Domain/Application/Infrastructure/API)
- `src/shared` - Shared contracts/DTOs
- `src/apps` - Client apps (MAUI)
- `docker/postgres-compose.yml` - Local PostgreSQL stack

## Prerequisites

- .NET SDK 8.0+
- .NET MAUI workloads (for mobile/desktop app development)
- Docker (for local PostgreSQL)

## Run PostgreSQL locally

```bash
docker compose -f docker/postgres-compose.yml up -d
```

## Run the API

```bash
dotnet restore src/backend/CareFlux.Api/CareFlux.Api.csproj
dotnet run --project src/backend/CareFlux.Api/CareFlux.Api.csproj
```

API health endpoint:

- `GET http://localhost:5000/health`

## Run the MAUI app

```bash
dotnet workload restore src/apps/CareFlux.Maui/CareFlux.Maui.csproj
dotnet build src/apps/CareFlux.Maui/CareFlux.Maui.csproj
```

Examples:

```bash
# Android
dotnet build src/apps/CareFlux.Maui/CareFlux.Maui.csproj -f net8.0-android

# Windows
dotnet build src/apps/CareFlux.Maui/CareFlux.Maui.csproj -f net8.0-windows10.0.19041.0
```

> iOS builds require a Mac build host.

## Build solution

```bash
dotnet build src/CareFlux.sln
```
