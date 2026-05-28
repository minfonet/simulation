# Driving Simulation Platform — MVP

A full-stack driving simulation platform for driver training and evaluation. Built with ASP.NET Core 10, Next.js 16, and Godot 4.

**Test status:** 101 unit/integration tests + 19 E2E smoke tests — all passing ✅

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Quick Start (Docker Compose)](#quick-start-docker-compose)
- [Manual Development Setup](#manual-development-setup)
- [Running the E2E Smoke Test](#running-the-e2e-smoke-test)
- [Godot Simulation](#godot-simulation)
- [Debugging Guide](#debugging-guide)
- [Supported Use Cases (MVP Flows)](#supported-use-cases-mvp-flows)
- [MVP Delivered Scope](#mvp-delivered-scope)
- [Test Summary](#test-summary)
- [Project Structure](#project-structure)

---

## Architecture Overview

```
┌─────────────┐     ┌──────────────┐     ┌──────────────┐
│  Next.js 16  │────▶│ ASP.NET Core │────▶│  PostgreSQL  │
│  Frontend    │     │  Backend API  │     │    16        │
│  :3000       │     │  :8080        │     │  :5432       │
└─────────────┘     └──────┬───────┘     └──────────────┘
                           │
                           │ HTTP (telemetry, events)
                           │
                    ┌──────▼───────┐
                    │  Godot 4      │
                    │  Simulation   │
                    │  (local only) │
                    └──────────────┘
```

### Key design decisions

- **Architecture boundaries:** Telemetry goes through `ITelemetryIngestor`/`ITelemetryStore` (not controller-to-DB). Controllers stay thin.
- **Auth:** JWT (access + refresh tokens), stored in `localStorage`.
- **No mandatory time limit:** Sessions end only by explicit finish action.
- **REST polling for telemetry** (no SignalR in MVP).
- **No hardware integration** in MVP (keyboard only: WASD + space).

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| Docker Desktop | Any recent | Required for Docker Compose workflow |
| .NET SDK | 10.0+ | `dotnet --version` to verify |
| Node.js | 20+ | `node --version` to verify |
| Godot 4 | 4.6.3 mono | Only needed for real simulation (see [Godot Simulation](#godot-simulation)) |
| PowerShell | 7+ | For E2E smoke test script |

---

## Quick Start (Docker Compose)

The fastest way to run everything:

```powershell
# 1. Start all services
cd docker
docker compose up -d

# 2. Verify services are healthy
curl http://localhost:8080/health
# → {"status":"healthy"}

# 3. Open the frontend
# → http://localhost:3000
```

This starts three containers:
- **PostgreSQL 16** on port `5432`
- **Backend API** on port `8080`
- **Frontend** on port `3000`

### Stop services

```powershell
cd docker
docker compose down
# Add -v to also delete database volume:
docker compose down -v
```

### Rebuild after changes

```powershell
cd docker
docker compose up -d --build
```

---

## Manual Development Setup

Run services individually for faster iteration:

### Backend API

```powershell
# 1. Start only the database via Docker
cd docker
docker compose up -d db

# 2. Run the backend (auto-migrates on startup)
cd ../src/backend/SimApi
dotnet run
# → Listening on http://localhost:8080
```

### Frontend

```powershell
# 1. Install dependencies (first time only)
cd src/frontend/sim-web
npm install

# 2. Start development server
npm run dev
# → Listening on http://localhost:3000
# → API proxy to http://localhost:8080
```

The frontend `.env.local` file points `NEXT_PUBLIC_API_URL=http://localhost:8080`.

---

## Running the E2E Smoke Test

The smoke test exercises the complete happy path **via API** (no Godot required):

```powershell
# Ensure Docker Compose is running (backend on :8080)
cd docker
docker compose up -d

# Run the smoke test (from repo root)
cd ..
.\tests\e2e\smoke-test.ps1
```

### Expected output (19/19 passing):

```
=== 1. Health Check ===
  [PASS] Health endpoint returns healthy
=== 2. Bootstrap Organization ===
  [PASS] Bootstrap organization exists
...
=== 9b. Send Telemetry with Collision ===
  [PASS] Telemetry ingested: 1 points (1 critical events)
=== 9c. Get Critical Events ===
  [PASS] Retrieved 1 critical events (type=collision, severity=medium)
=== 11. Get Instructor Report ===
  [PASS] Report: 3 telemetry points, 1 collisions, 1 critical events
=== 14. Get Trainee Report ===
  [PASS] Trainee report: score=92.5, isEvaluated=True
=== RESULTS ===
Tests passed: 19
Tests failed: 0
```

### What the smoke test covers

| Step | What |
|------|------|
| 1 | Health check |
| 2 | Bootstrap org seed via Docker PostgreSQL |
| 3–5 | Admin register, create org, invite users |
| 6 | Both roles login |
| 7–8 | Instructor creates session → Trainee starts it |
| 9 | Normal telemetry (2 points, no collision) |
| 9b | Collision telemetry (1 point, collision=true) |
| 9c | Critical events retrieval |
| 10 | Finish session |
| 11 | Instructor report (summary + collisions + events) |
| 12 | Evaluate session |
| 13 | Telemetry retrieval |
| 14 | Trainee report (score + isEvaluated) |
| 15 | Auth/me verification |

> **Note:** The smoke test is self-seeding: it creates its own bootstrap org, admin, organization, and users. Each run uses unique email addresses (`*-{timestamp}@test.com`).

---

## Godot Simulation

The Godot simulation runs **locally on your machine**, not inside Docker.

### Setup

Godot 4.6.3 mono is required. Verify installation:

```powershell
# Path used in development
& "C:\Projects\ia\core\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64.exe" --version
```

### Launch flow

1. Open the frontend at `http://localhost:3000`
2. Login as **Trainee**
3. Navigate to `/trainee/sessions`
4. Click **"Start"** on a session → session becomes `Active`
5. A **Launch Card** appears with:
   - CLI command: `godot --session-id {id} --api-url {url} --token {token}`
   - **"Copy Command"** button
   - **"Download Launch Script"** button (.ps1)
6. Copy the command and run it in a terminal:

```powershell
& "C:\Projects\ia\core\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64.exe" `
  --session-id "00000000-0000-0000-0000-000000000001" `
  --api-url "http://localhost:8080" `
  --token "eyJhbGciOiJIUzI1NiIs..."
```

### Controls inside Godot

| Key | Action |
|-----|--------|
| W | Accelerate |
| S | Brake / Reverse |
| A | Steer left |
| D | Steer right |
| Space | Handbrake |
| F | Finish simulation (sends finish to API) |

### Godot architecture

- **`BackendClient.cs`** — HTTP client: parses CLI args, sends telemetry batches, starts/finishes sessions.
- **`VehicleController.cs`** — Car physics (RigidBody3D), telemetry collection, collision detection.
- **`Main.tscn`** — Scene with ground plane, car, camera, and obstacle cubes.

### Without Godot (API-only simulation)

You can simulate the full flow without Godot by sending telemetry directly via the API:

```powershell
# After starting a session, send telemetry via curl:
curl -X POST http://localhost:8080/api/telemetry `
  -H "Authorization: Bearer $token" `
  -H "Content-Type: application/json" `
  -d '{"sessionId":"...","points":[{"timestamp":"...","speed":45.5,"steeringAngle":0.15,"positionX":10,"positionY":0,"positionZ":20,"collision":false}]}'
```

---

## Debugging Guide

### Backend

```powershell
# Run with hot reload
cd src/backend/SimApi
dotnet watch run

# Run tests
cd tests/SimApi.IntegrationTests
dotnet test
```

Debug endpoints:

| URL | Purpose |
|-----|---------|
| `GET /health` | Health check (returns `{"status":"healthy"}`) |
| `GET /api/auth/me` | Current user identity (requires JWT) |
| `POST /api/auth/login` | Get JWT tokens |

### Frontend

```powershell
cd src/frontend/sim-web

# Run tests
npm test

# Run tests in watch mode
npm run test:watch

# Build for production
npm run build
```

### Database

```powershell
# Connect to PostgreSQL directly
docker compose exec db psql -U postgres -d simulation

# List all tables
\dt

# Query organizations
SELECT * FROM "Organizations";

# Query sessions
SELECT * FROM "SimulationSessions";
```

### Docker logs

```powershell
# All services
docker compose logs -f

# Specific service
docker compose logs -f backend
docker compose logs -f frontend
docker compose logs -f db
```

### Common issues

| Issue | Solution |
|-------|----------|
| Docker Desktop not running | Start Docker Desktop, wait for engine |
| Port 8080/3000 already in use | `netstat -ano \| findstr :8080` → kill process or change port |
| Backend fails to connect to DB | Wait for Postgres healthcheck, then restart backend |
| Smoke test fails with 400 on session create | Check `scenario` parameter — must be `"default"` |

---

## Supported Use Cases (MVP Flows)

### Flow 1: Admin creates organization

```
Admin → /admin/organizations → Create org → Org appears in list
```

**Backend:** `POST /api/admin/organizations` → `201`  
**Frontend:** `/admin/organizations` page with create/delete  

### Flow 2: Instructor creates session with base preset

```
Instructor → /instructor/sessions → Select trainee → Select "Default" preset → Create
```

**Backend:** `GET /api/instructor/scenario-presets` → `POST /api/instructor/sessions`  
**Frontend:** Dropdown populated from presets endpoint  
**Preset validation:** Invalid presets return `400`  

### Flow 3: Trainee launches Godot simulation (no time limit)

```
Trainee → /trainee/sessions/{id} → Start → Launch card → Run Godot locally
```

**Handoff:** CLI command `--session-id {id} --api-url {url} --token {token}`  
**Godot:** Parses args, starts session, sends telemetry, finishes on command  
**No time limit:** Session ends only on explicit finish  

### Flow 4: Telemetry and critical events collection

```
Godot → POST /api/telemetry → collision=true → Auto-generate CriticalEvent
Instructor → GET /api/telemetry/session/{id}/events → View critical events
```

**Critical events:** Auto-derived from `collision=true` in telemetry  
**Architecture boundary:** `ITelemetryIngestor` → `ITelemetryStore`  

### Flow 5: Evaluator reviews report and grades session

```
Instructor → /instructor/sessions/{id} → Report card → Score + comments → Evaluate
Trainee → /trainee/sessions/{id} → View score and instructor notes
```

**Report contains:** Total telemetry points, avg/max/min speed, collision count, critical events list  
**Evaluation:** Score (0–100) + comments, persisted in `Evaluation` table  

---

## MVP Delivered Scope

### Milestone P0 ✅ — Foundation

| Feature | Status |
|---------|--------|
| Auth (register, login, JWT refresh) | ✅ |
| Admin org CRUD | ✅ |
| User invitation (Admin → Instructor + Trainee) | ✅ |
| Session CRUD (create, start, finish) | ✅ |
| Base scenario preset (`"default"` → Godot `Main.tscn`) | ✅ |
| Godot launch handoff (CLI args: `--session-id`, `--api-url`, `--token`) | ✅ |
| Godot auth/token strategy (Bearer JWT) | ✅ |
| Docker Compose (Postgres + Backend + Frontend) | ✅ |
| Backend integration tests (50) | ✅ |
| E2E smoke test script | ✅ |

### Milestone P1 ✅ — Reporting

| Feature | Status |
|---------|--------|
| Critical event model (sessionId, timestamp, type, severity, metadata) | ✅ |
| Auto-generation from collision telemetry | ✅ |
| `ITelemetryIngestor` / `ITelemetryStore` boundaries | ✅ |
| Instructor report endpoint (`GET /api/instructor/sessions/{id}/report`) | ✅ |
| Trainee report endpoint (`GET /api/trainee/sessions/{id}/report`) | ✅ |
| Frontend report card (metrics + events) | ✅ |
| Evaluation (score + comments) | ✅ |
| Backend integration tests (66 — incl. 16 for events + reports) | ✅ |

### Post-MVP (not in scope)

- Collision deduplication
- Multiple scenario presets
- Auto-launch Godot from browser
- SignalR real-time telemetry
- Hardware integration (steering wheel, pedals)
- TimescaleDB / time-series optimized storage
- Advanced analytics or charts
- Rule violation detection (speeding, off-road)
- Braking/acceleration telemetry fields

---

## Test Summary

| Layer | Framework | Count | Status |
|-------|-----------|-------|--------|
| **Backend API** | xUnit + WebApplicationFactory | 66 | ✅ All passing |
| **Frontend (lib/)** | Vitest + Testing Library | 23 | ✅ All passing |
| **Godot Simulation** | xUnit (BackendClient) | 12 | ✅ All passing |
| **E2E Smoke** | PowerShell (API-only) | 19 | ✅ All passing |
| **Total** | | **120** | **✅ 120/120 passing** |

### Running all tests

```powershell
# Backend
cd tests/SimApi.IntegrationTests
dotnet test

# Frontend
cd src/frontend/sim-web
npm test

# Godot simulation
cd tests/GodotSim.Tests
dotnet test

# E2E (requires Docker Compose running)
cd repo-root
.\tests\e2e\smoke-test.ps1
```

---

## Project Structure

```
simulation/
├── docker/
│   ├── docker-compose.yml       # Postgres + Backend + Frontend
│   ├── Dockerfile.backend       # .NET 10 multi-stage build
│   └── Dockerfile.frontend      # Next.js 16 multi-stage build
├── src/
│   ├── backend/
│   │   └── SimApi/
│   │       ├── Controllers/     # Auth, Admin, Instructor, Trainee, Telemetry
│   │       ├── Models/          # Organization, User, SimulationSession, etc.
│   │       ├── Services/        # JwtService, PasswordService
│   │       ├── DTOs/            # Request/response data transfer objects
│   │       ├── Data/            # AppDbContext, migrations
│   │       └── Program.cs       # App entry point
│   └── frontend/
│       └── sim-web/
│           ├── app/             # Next.js App Router pages
│           │   ├── admin/       # Admin dashboard, orgs, users
│           │   ├── instructor/  # Instructor dashboard, sessions, evaluations
│           │   └── trainee/     # Trainee dashboard, sessions, evaluations
│           ├── components/      # UI primitives + layout
│           └── lib/             # API client, auth context
├── simulation/
│   └── driving-sim/             # Godot 4 project
│       ├── Scripts/             # VehicleController, BackendClient
│       └── Scenes/              # Main.tscn, obstacle scenes
├── tests/
│   ├── e2e/
│   │   └── smoke-test.ps1      # 19-step E2E happy path
│   ├── SimApi.IntegrationTests/ # 66 backend integration tests
│   └── GodotSim.Tests/         # 12 Godot unit tests
├── docs/
│   └── 01-product/
│       ├── mdvp.md              # Full MVP definition
│       ├── user-flows.md        # All user flows with diagrams
│       ├── flow-gap-plan.md     # Gap analysis and remaining items
│       └── execution-readiness.md  # Per-flow execution checklist
└── .memory/                     # Agent memory and sessions
```

---

## API Endpoints

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| `GET` | `/health` | None | Health check |
| `POST` | `/api/auth/register` | None | Register user |
| `POST` | `/api/auth/login` | None | Login |
| `POST` | `/api/auth/refresh` | None | Refresh JWT |
| `GET` | `/api/auth/me` | JWT | Current user identity |
| `GET` | `/api/admin/organizations` | Admin | List organizations |
| `POST` | `/api/admin/organizations` | Admin | Create organization |
| `DELETE` | `/api/admin/organizations/{id}` | Admin | Delete organization |
| `POST` | `/api/admin/organizations/{id}/users` | Admin | Invite user |
| `GET` | `/api/instructor/sessions` | Instructor | List sessions |
| `POST` | `/api/instructor/sessions` | Instructor | Create session |
| `GET` | `/api/instructor/sessions/{id}` | Instructor | Get session detail |
| `GET` | `/api/instructor/scenario-presets` | Instructor | List presets |
| `POST` | `/api/instructor/sessions/{id}/evaluate` | Instructor | Evaluate session |
| `GET` | `/api/instructor/sessions/{id}/report` | Instructor | Get report |
| `GET` | `/api/instructor/evaluations` | Instructor | List evaluations |
| `GET` | `/api/instructor/trainees` | Instructor | List trainees |
| `GET` | `/api/trainee/sessions` | Trainee | List sessions |
| `POST` | `/api/trainee/sessions/{id}/start` | Trainee | Start session |
| `POST` | `/api/trainee/sessions/{id}/finish` | Trainee | Finish session |
| `GET` | `/api/trainee/sessions/{id}` | Trainee | Get session detail |
| `GET` | `/api/trainee/sessions/{id}/report` | Trainee | Get report |
| `GET` | `/api/trainee/evaluations` | Trainee | List evaluations |
| `POST` | `/api/telemetry` | Trainee | Ingest telemetry batch |
| `GET` | `/api/telemetry/session/{id}` | Trainee/Instructor | Retrieve telemetry |
| `GET` | `/api/telemetry/session/{id}/events` | Trainee/Instructor | Get critical events |

---

## License

Internal project — no license specified.
