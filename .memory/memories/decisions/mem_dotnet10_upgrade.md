# .NET 10 Upgrade

**Date**: 2026-05-28  
**Scope**: Backend (`SimApi.csproj`), Docker (`Dockerfile.backend`), Docs (`run-and-debug.md`)  
**Verification**: QA Agent — PASS

## Decision

Upgrade the backend from .NET 8 to .NET 10 to stay current with the latest LTS-aligned release and leverage ASP.NET Core 10 improvements.

## Changes made

| File | Change |
|------|--------|
| `src/backend/SimApi/SimApi.csproj` | `TargetFramework` → `net10.0`; all `Microsoft.AspNetCore.*` / `EntityFrameworkCore.*` packages → `10.0.*`; `Npgsql.EntityFrameworkCore.PostgreSQL` → `10.0.*`; `BCrypt.Net-Next` left at `4.0.3` (unchanged) |
| `docker/Dockerfile.backend` | SDK base image → `mcr.microsoft.com/dotnet/sdk:10.0`; runtime base image → `mcr.microsoft.com/dotnet/aspnet:10.0` |
| `docs/06-engineering/run-and-debug.md` | Prerequisites SDK version → `10.0+`; service table → `ASP.NET Core 10 API` |

## What was NOT changed (intentionally)

- `simulation/driving-sim/driving-sim.csproj`: remains `net8.0` because Godot 4.4.1 ships its own .NET SDK and does not yet support .NET 10.
- All other backend `.cs` files: no code-level API changes were needed for the `net8.0` → `net10.0` migration.

## Verification

QA agent confirmed:
- No residual `net8` or `8.0` references in `src/backend/` or `docker/`
- All package versions are consistent with .NET 10
- Docs match the new version
- Godot project was not touched

## Rationale

- Staying on a supported .NET version
- Performance and security improvements in ASP.NET Core 10
- No breaking changes affected the current codebase
