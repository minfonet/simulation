# Session: Consistency fixes after project audit

**Date:** 2026-05-28

## Summary

Applied the recommended corrections from the project consistency audit: updated obsolete runtime docs, fixed E2E smoke test bootstrap/auth issues, aligned Godot SDK with installed runtime, removed static test DbContext override, and corrected memory/test counts.

## Changes Made

### Backend Test Infrastructure

- Removed `Program.DbContextConfigOverride` static mutable hook from `src/backend/SimApi/Program.cs`.
- Added `Testing` environment guard so production Npgsql registration is skipped during integration tests.
- Updated `tests/SimApi.IntegrationTests/IntegrationTestBase.cs` to use `WebApplicationFactory.WithWebHostBuilder`, `UseEnvironment("Testing")`, and per-test SQLite injection.
- Removed `tests/SimApi.IntegrationTests/xunit.runner.json`; forced sequential execution is no longer needed.

### E2E Smoke Test

- Rewrote `tests/e2e/smoke-test.ps1`.
- Fixed Authorization header assignment (`$headers["Authorization"]`).
- Added automatic bootstrap organization seed through Docker Compose PostgreSQL.
- Added unique test users per run and fail-fast assertions for tokens, IDs, statuses, telemetry count, and `/auth/me`.
- Validated PowerShell syntax successfully.

### Godot

- Updated `simulation/driving-sim/driving-sim.csproj` from `Godot.NET.Sdk/4.4.1` to `Godot.NET.Sdk/4.6.3`.
- Fixed `Godot.HttpClient` vs `System.Net.Http.HttpClient` ambiguity with a type alias.
- Added `JsonPropertyName` attributes to `VehicleController.TelemetryPoint` to preserve backend-compatible camelCase telemetry JSON.
- Fixed Godot test compatibility by replacing `JsonElement.GetPropertyCount()` with `EnumerateObject()`.

### Agents and Docs

- Updated `.opencode/agents/qa.md` to remove obsolete `.NET/Godot unavailable` blockers and document actual test commands.
- Updated `docs/06-engineering/run-and-debug.md` prerequisites, testing commands, manual happy path bootstrap, and Docker Compose DB inspection commands.
- Updated `.memory/active-context.md` with current test results and remaining risks.

## Verification

| Command | Result |
|---|---|
| `dotnet test tests/SimApi.IntegrationTests/SimApi.IntegrationTests.csproj` | 50/50 passing |
| `dotnet test tests/GodotSim.Tests/GodotSim.Tests.csproj` | 12/12 passing |
| `npm test` in `src/frontend/sim-web` | 23/23 passing |
| PowerShell parser on `tests/e2e/smoke-test.ps1` | Syntax OK |

## Remaining Work

- Run `tests/e2e/smoke-test.ps1` against live Docker Compose services.
- Add PostgreSQL/Testcontainers coverage for provider-specific behavior.
- Add frontend page component tests for admin/instructor/trainee pages.
