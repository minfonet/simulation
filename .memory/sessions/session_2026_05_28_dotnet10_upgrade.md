# Session: Upgrade backend from .NET 8 to .NET 10

**Date:** 2026-05-28
**Lead agent:** lead.md

## Summary

Upgraded the backend ASP.NET Core project from .NET 8.0 to .NET 10.0.

## Changes Made

### 1. `src/backend/SimApi/SimApi.csproj`
- `TargetFramework`: `net8.0` → `net10.0`
- `Microsoft.AspNetCore.Authentication.JwtBearer`: `8.0.0` → `10.0.8`
- `Microsoft.EntityFrameworkCore.Design`: `8.0.0` → `10.0.8`
- `Npgsql.EntityFrameworkCore.PostgreSQL`: `8.0.0` → `10.0.1`
- `BCrypt.Net-Next`: `4.0.3` (unchanged — not a Microsoft package)

### 2. `docker/Dockerfile.backend`
- `mcr.microsoft.com/dotnet/sdk:8.0` → `mcr.microsoft.com/dotnet/sdk:10.0`
- `mcr.microsoft.com/dotnet/aspnet:8.0` → `mcr.microsoft.com/dotnet/aspnet:10.0`

### 3. `docs/06-engineering/run-and-debug.md`
- Prerequisites table: `.NET SDK 8.0+` → `.NET SDK 10.0+`
- Service matrix: `ASP.NET Core 8 API` → `ASP.NET Core 10 API`
- Project structure: `ASP.NET Core 8 API` → `ASP.NET Core 10 API`

## Not Changed (Intentionally)

- `simulation/driving-sim/driving-sim.csproj` — Godot project, not the backend. Godot 4 uses `Godot.NET.Sdk/4.4.1` with `net8.0`. .NET 10 is not yet supported by Godot.
- `.memory/memories/learnings/mem_godot4_csharp.md` — documents Godot project's target framework (still accurate).
- `.memory/sessions/session_2026_05_27_godot_scaffolding.md` — historical record, not modified.

## NuGet Versions Used

| Package | Version | Source |
|---|---|---|
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.8 | nuget.org (latest stable for .NET 10) |
| Microsoft.EntityFrameworkCore.Design | 10.0.8 | nuget.org (latest stable for .NET 10) |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.1 | nuget.org (latest stable for .NET 10) |
| BCrypt.Net-Next | 4.0.3 | Unchanged |

## Next Steps

- Install .NET SDK 10 on dev machine
- Run `dotnet restore` and `dotnet build` to verify
- Update run-and-debug.md status column from ❌ NO INSTALADO to ✅ after installation
