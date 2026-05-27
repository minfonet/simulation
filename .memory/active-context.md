# ACTIVE CONTEXT

## Current Project State

Backend scaffolding complete.
Frontend scaffolding (Next.js 16 + Tailwind v4) complete.
Godot simulation scaffolding complete.
Documentation updated (user-flows.md, role-based-ui.md, run-and-debug.md, architecture-review.md).

## MVP Reference

See `docs/01-product/mdvp.md` for full scope, endpoints, data models, and acceptance criteria.

## Workflow Rules (mandatory)

1. LEAD receives task → breaks into subtasks
2. LEAD delegates EACH subtask to IMPLEMENTER via Task tool (`subagent_type="implementer"`)
3. IMPLEMENTER writes code → returns summary
4. LEAD delegates REVIEW to REVIEWER via Task tool (`subagent_type="reviewer"`)
5. REVIEWER returns findings (PASS / PASS WITH CHANGES / REJECT)
6. LEAD delegates TESTING to QA via Task tool (`subagent_type="qa"`)
7. QA creates/updates tests and reports results (PASS / PASS WITH GAPS / FAIL)
8. LEAD either applies fixes or accepts → updates active-context.md

Exceptions (LEAD may implement directly): trivial single-file changes under 10 lines.

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
2. ✅ Organization CRUD endpoints
3. ✅ Auth system (register, login, refresh, JWT)
4. ✅ User CRUD with role management
5. ✅ SimulationSession CRUD
6. ✅ Telemetry ingestion + retrieval endpoints
7. ✅ Evaluation endpoints
 8. ✅ Next.js frontend
9. ✅ Godot driving simulation
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
| `docker/docker-compose.yml` | Postgres + backend + frontend services |
| `src/frontend/sim-web/` | Next.js 16 project (TypeScript, Tailwind v4) |
| `src/frontend/sim-web/lib/api.ts` | API client with JWT auth header |
| `src/frontend/sim-web/lib/auth-context.tsx` | Auth provider + useAuth hook |
| `src/frontend/sim-web/proxy.ts` | Route protection (Next.js 16 proxy) |
| `src/frontend/sim-web/app/login/page.tsx` | Login page |
| `src/frontend/sim-web/app/admin/*` | Admin dashboard, orgs, users pages |
| `src/frontend/sim-web/app/instructor/*` | Instructor dashboard, sessions, evaluations |
| `src/frontend/sim-web/app/trainee/*` | Trainee dashboard, sessions, evaluations |
| `src/frontend/sim-web/components/ui/*` | UI primitives (button, card, input, badge, etc.) |
| `src/frontend/sim-web/components/layout/*` | Sidebar, Navbar components |
| `src/frontend/sim-web/vitest.config.mts` | Vitest config (jsdom, path aliases, jest-dom setup) |
| `src/frontend/sim-web/lib/__tests__/setup.ts` | Test setup: imports jest-dom matchers |
| `src/frontend/sim-web/lib/__tests__/api.test.ts` | 10 tests: API client (auth headers, refresh, 204, errors) |
| `src/frontend/sim-web/lib/__tests__/auth-context.test.tsx` | 7 tests: AuthProvider (login, logout, validation, redirect) |
| `src/frontend/sim-web/lib/__tests__/proxy.test.ts` | 6 tests: proxy passthrough + matcher config |
| `docker/Dockerfile.frontend` | Multi-stage Next.js standalone build |
| `simulation/driving-sim/project.godot` | Godot 4 project configuration |
| `simulation/driving-sim/driving-sim.csproj` | C# project with Godot.NET.Sdk |
| `simulation/driving-sim/export_presets.cfg` | Windows desktop export preset |
| `simulation/driving-sim/Scripts/VehicleController.cs` | Car physics, WASD+space, telemetry collection |
| `simulation/driving-sim/Scripts/BackendClient.cs` | HTTP client: start session, batch telemetry, finish |
| `simulation/driving-sim/Scenes/Main.tscn` | Main scene: ground, car, camera, obstacles |

## Open Findings (from architecture review)

See `docs/99-reference/architecture-review.md` for 12 inconsistencies ranked by complexity.
✅ Items 3 (ContactMonitor), 4 (return type), 5 (DI), 8 (proxy.ts) — fixed 2026-05-27.
✅ Items 9 (token refresh frontend), 10 (auth/me endpoint) — fixed 2026-05-27.
✅ Items 1 (stack docs), 11 (API docs) — fixed 2026-05-27 (docs updated, empty files removed).

## Memory State

| Area | Files | Status |
|---|---|---|
| `active-context.md` | 1 | Current |
| `sessions/` | 5 (backend, frontend, godot, docs-review, fixes batch 1-2, fixes batch 3) | Complete for MVP phase |
| `memories/decisions/` | 6 (auth JWT, Docker Compose, EF Core EnsureCreated, Next.js 16 proxy, localStorage auth, workflow enforcement) | Covers all key decisions |
| `memories/bugs/` | 3 (proxy vs localStorage, ContactMonitor, DI inconsistency) | Documents all review findings |
| `memories/learnings/` | 3 (Tailwind v4, Next.js 16 changes, Godot 4 C#) | Captures framework-specific lessons |
| `memories/architecture/` | 1 (three-layer architecture) | System structure documented |

## Test Infrastructure (New — 2026-05-27)

| Layer | Framework | Status | Tests |
|---|---|---|---|
| Frontend (sim-web) | Vitest + Testing Library + jsdom | ✅ 26 passing | `api.test.ts` (10), `auth-context.test.tsx` (7), `proxy.test.ts` (6), setup |
| Backend (SimApi) | xUnit (planned) | ⬜ No .NET SDK | Scoped, need `dotnet` to run |
| Godot Simulation | N/A | ⬜ No Godot 4 | Scoped, need Godot to run |

### Frontend test details

- **Config**: `src/frontend/sim-web/vitest.config.mts` — jsdom env, `@/` alias, jest-dom setup
- **Setup**: `lib/__tests__/setup.ts` imports `@testing-library/jest-dom/vitest`
- **Scripts**: `npm test` (vitest run), `npm run test:watch` (vitest)
- **Key learnings**: Use `vi.hoisted()` for mock factory vars, dynamic `import()` over `require()`, avoid infinite retry loops in 401 mock tests

## Current Risks

- .NET SDK 10 needed on dev machine to build/test
- No Godot 4 installed to open/run the simulation project
- Workflow enforcement not yet tested (LEAD → IMPLEMENTER → REVIEWER)
- Backend tests scoped but un-runnable without `dotnet`

## Current Focus

.NET backend upgraded from 8.0 to 10.0. Next: install .NET SDK 10 on dev machine.

## Open Tasks

- [x] Scaffold ASP.NET Core + EF Core + Docker Compose
- [x] Next.js frontend (auth + dashboards)
- [x] Godot 4 driving simulation
- [x] Docker Compose orchestration (add frontend)
- [x] Frontend test infrastructure (Vitest + config + 26 passing tests)
- [x] Upgrade backend from .NET 8 to .NET 10 (csproj, Dockerfile, docs)
- [ ] Install .NET SDK 10 on dev machine
- [ ] Backend integration tests (blocked: no .NET SDK)
- [ ] E2E smoke tests (PowerShell/curl)
