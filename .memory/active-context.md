# ACTIVE CONTEXT

## Current Project State

Backend scaffolding complete. Ready for next tasks.

## MVP Reference

See `docs/01-product/mdvp.md` for full scope, endpoints, data models, and acceptance criteria.

## Active Decisions

- Agent workflow: LEAD delegates to IMPLEMENTER, then REVIEWER, then back to LEAD
- Vertical-slice approach: build one complete slice (auth → orgs → sessions → sim → telemetry → evaluation)
- Priority order: Backend API first, then Frontend, then Godot simulation
- Postgres with EF Core (vanilla, no TimescaleDB for MVP)
- JWT auth with refresh tokens
- REST polling for telemetry (no SignalR in MVP)
- No hardware integration in MVP

## Implementation Plan (Phase 1 — Backend Core)

1. ✅ Scaffold ASP.NET Core project + EF Core + Postgres setup
2. ⬜ Organization CRUD endpoints — DONE
3. ⬜ Auth system (register, login, refresh, JWT) — DONE
4. ⬜ User CRUD with role management — DONE
5. ⬜ SimulationSession CRUD — DONE
6. ⬜ Telemetry ingestion + retrieval endpoints — DONE
7. ⬜ Evaluation endpoints — DONE
8. ⬜ Next.js frontend
9. ⬜ Godot driving simulation
10. ⬜ Integration tests

## Scaffolding Created

| File | Purpose |
|---|---|
| `src/backend/SimApi/SimApi.csproj` | Project with EF Core, JWT, BCrypt packages |
| `src/backend/SimApi/Program.cs` | App setup: auth, EF Core, CORS, auto-migrate |
| `src/backend/SimApi/appsettings.json` | Connection string, JWT config |
| `src/backend/SimApi/Models/*.cs` | 5 models + enums |
| `src/backend/SimApi/Data/AppDbContext.cs` | EF Core context with relationships |
| `src/backend/SimApi/Services/*.cs` | JWT service, Password service |
| `src/backend/SimApi/DTOs/*.cs` | Request/response DTOs |
| `src/backend/SimApi/Controllers/*.cs` | Auth, Admin, Instructor, Trainee, Telemetry |
| `docker/Dockerfile.backend` | Multi-stage .NET publish |
| `docker/docker-compose.yml` | Postgres + backend services |

## Current Risks

- No .NET SDK on dev machine to build/test
- No frontend to call the API yet

## Current Focus

Frontend scaffolding (Next.js + TypeScript).

## Open Tasks

- [x] Scaffold ASP.NET Core + EF Core + Docker Compose
- [ ] Next.js frontend (auth + dashboards)
- [ ] Godot 4 driving simulation
- [ ] Docker Compose orchestration (add frontend)
