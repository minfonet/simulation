# Run & Debug Guide — MVP

## Prerequisites

### Required
| Dependency | Version | Check | Status on this machine |
|---|---|---|---|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Latest | `docker --version` | ✅ v29.5.2 |
| [Node.js](https://nodejs.org/) | 20+ | `node --version` | ✅ v24 |
| [Git](https://git-scm.com/) | Any | `git --version` | ✅ |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ | `dotnet --version` | ✅ 10.0.300 |
| [Godot 4 .NET](https://godotengine.org/download) | 4.6.3 Mono | `godot --version` | ✅ `C:\Projects\ia\core\Godot_v4.6.3-stable_mono_win64` |

### Optional
| Tool | Purpose | Status |
|---|---|---|
| [VS Code](https://code.visualstudio.com/) + C# Dev Kit | Edit / debug backend | ✅ VS Code v1.121, ❌ C# Dev Kit |
| [Rider](https://www.jetbrains.com/rider/) | .NET IDE | ❌ |
| [pgAdmin](https://www.pgadmin.org/) | Browse PostgreSQL directly | ❌ |

---

## Quick Start (Docker — full system)

```bash
# From the project root
docker compose -f docker/docker-compose.yml up --build
```

This starts three services:

| Service | Port | Description |
|---|---|---|
| `db` | `5432` | PostgreSQL 16 |
| `backend` | `8080` | ASP.NET Core 10 API |
| `frontend` | `3000` | Next.js 16 web app |

The API auto-creates tables on first run (`EnsureCreated`).

Once running:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:8080
- **PostgreSQL**: `localhost:5432` (user: `postgres`, password: `postgres`, db: `simulation`)

---

## Running Each Part Locally

### 1. Database only (Docker)

```bash
docker run -d --name sim-db ^
  -e POSTGRES_DB=simulation ^
  -e POSTGRES_USER=postgres ^
  -e POSTGRES_PASSWORD=postgres ^
  -p 5432:5432 ^
  postgres:16
```

### 2. Backend (ASP.NET Core)

```bash
cd src/backend/SimApi
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000`.

> The backend reads the connection string from `appsettings.json`. By default it points to `localhost:5432`. If you changed the DB port, update the connection string.

### 3. Frontend (Next.js 16)

```bash
cd src/frontend/sim-web
npm install
npm run dev
```

Opens at `http://localhost:3000`.

The frontend reads `NEXT_PUBLIC_API_URL` from `.env.local`:

```
NEXT_PUBLIC_API_URL=http://localhost:5000
```

> The `.env.local` file is already created. Update the URL if your backend runs on a different port.

### 4. Godot simulation

```bash
# Open in editor
godot simulation/driving-sim/project.godot

# Run from editor (F5) — requires session-id argument in Godot editor CLI args
# Or export and run standalone:
godot --headless --export-release Windows desktop/SimDriver.exe
./desktop/SimDriver.exe --session-id <guid> --token <jwt> --api-url http://localhost:8080
```

The simulation accepts three CLI arguments:
| Argument | Required | Description |
|---|---|---|
| `--session-id` | Yes | GUID of the simulation session to join |
| `--token` | No | JWT access token for authenticated endpoints |
| `--api-url` | No | Backend URL (default: `http://localhost:8080`) |

---

## Testing

### Frontend (sim-web)

The frontend uses **Vitest** + **Testing Library** for unit tests.

```bash
cd src/frontend/sim-web

# Run all tests once
npm test

# Run in watch mode (re-runs on file changes)
npm run test:watch
```

#### What's tested

| Test file | Coverage | Tests |
|---|---|---|
| `lib/__tests__/api.test.ts` | API client (`api.ts`): auth header injection, 401 refresh flow, concurrent 401 queuing, 204 handling, error responses | 10 |
| `lib/__tests__/auth-context.test.tsx` | `AuthProvider`: loading state, localStorage restore, login/logout, session validation on mount, redirect on failure | 7 |
| `lib/__tests__/proxy.test.ts` | `proxy.ts`: all routes pass through (auth enforced by backend + AuthProvider) | 6 |

**Requirements:** Node.js (see prerequisites above).

### Backend (SimApi)

The backend uses **xUnit** + **WebApplicationFactory** for integration tests.

```bash
dotnet test tests/SimApi.IntegrationTests/SimApi.IntegrationTests.csproj
```

#### What's tested

| Test area | Coverage | Tests |
|---|---|---|
| Health | `/health` endpoint | 1 |
| Auth | register, login, refresh, `/auth/me`, duplicate email, invalid credentials | 7 |
| Admin | organizations and users CRUD/invite flows | 7 |
| Instructor | session creation, retrieval, evaluation flows | 7 |
| Trainee | session list/start/finish flows | 7 |
| Telemetry | ingestion and retrieval | 5 |
| Security | 401/403 auth and role boundaries | 9 |
| Validation | invalid payloads and bad data | 7 |

**Database:** tests use SQLite in-memory for speed and isolation. Add PostgreSQL/Testcontainers coverage if a change depends on provider-specific behavior.

### Godot Simulation

Pure Godot-adjacent tests use **xUnit** and target the simulation project contracts.

```bash
dotnet test tests/GodotSim.Tests/GodotSim.Tests.csproj
```

These tests cover BackendClient payload compatibility and telemetry data structures. Engine-bound behavior such as `VehicleController` physics still requires Godot headless/editor execution or extracting more pure logic.

### E2E Smoke Test

The E2E smoke test runs against a running API and self-seeds the bootstrap organization through Docker Compose PostgreSQL by default.

```bash
# Start services first
docker compose -f docker/docker-compose.yml up -d --build

# Run smoke test
pwsh tests/e2e/smoke-test.ps1
```

Use `-SkipBootstrapSeed` only when the target database is already seeded.

---

## Complete Happy Path (Manual Test)

The admin organization endpoints require an authenticated Admin user. For a fully automated happy path, prefer `tests/e2e/smoke-test.ps1`, which seeds the bootstrap organization before registering the first admin.

If running manually against Docker Compose, seed the first organization once:

```bash
docker compose -f docker/docker-compose.yml exec -T db psql -U postgres -d simulation -c "INSERT INTO \"Organizations\" (\"Id\", \"Name\", \"CreatedAt\") VALUES ('00000000-0000-0000-0000-000000000001', 'Bootstrap Org', NOW()) ON CONFLICT (\"Id\") DO NOTHING;"
```

Then register the first admin against that organization:

```bash
curl -X POST http://localhost:8080/api/auth/register ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"admin@abc.com\",\"password\":\"123456\",\"name\":\"Admin\",\"role\":\"Admin\",\"organizationId\":\"00000000-0000-0000-0000-000000000001\"}"
```

Save the returned `accessToken` as `<admin-token>`.

### Step 1: Create an organization

```bash
curl -X POST http://localhost:8080/api/admin/organizations ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <admin-token>" ^
  -d "{\"name\":\"ABC Driving School\"}"
```

Save the returned `id` — this is your `OrganizationId`.

### Step 2: Register users

```bash
# Register Instructor
curl -X POST http://localhost:8080/api/admin/organizations/<org-id>/users ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <admin-token>" ^
  -d "{\"email\":\"instructor@abc.com\",\"password\":\"123456\",\"name\":\"Mary\",\"role\":\"Instructor\"}"

# Register Trainee
curl -X POST http://localhost:8080/api/admin/organizations/<org-id>/users ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <admin-token>" ^
  -d "{\"email\":\"trainee@abc.com\",\"password\":\"123456\",\"name\":\"John\",\"role\":\"Trainee\"}"
```

Save each user's `userId` from the responses.

### Step 3: Login as Instructor

```bash
curl -X POST http://localhost:8080/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"instructor@abc.com\",\"password\":\"123456\"}"
```

Save the `accessToken`.

### Step 4: Create a session (as Instructor)

```bash
curl -X POST http://localhost:8080/api/instructor/sessions ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <instructor-token>" ^
  -d "{\"traineeId\":\"<trainee-user-id>\",\"scenario\":\"sharp-turn\"}"
```

Save the returned session `id`.

### Step 5: Start the session (as Trainee)

```bash
# Login as trainee
curl -X POST http://localhost:8080/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"trainee@abc.com\",\"password\":\"123456\"}"

# Start session
curl -X POST http://localhost:8080/api/trainee/sessions/<session-id>/start ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <trainee-token>"
```

### Step 6: Send telemetry

```bash
curl -X POST http://localhost:8080/api/telemetry ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <trainee-token>" ^
  -d "{\"sessionId\":\"<session-id>\",\"points\":[{\"timestamp\":\"2025-01-01T00:00:00Z\",\"speed\":10.5,\"steeringAngle\":0.1,\"positionX\":1.0,\"positionY\":0.0,\"positionZ\":2.0,\"collision\":false}]}"
```

### Step 7: Finish session (as Trainee)

```bash
curl -X POST http://localhost:8080/api/trainee/sessions/<session-id>/finish ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <trainee-token>"
```

### Step 8: Evaluate (as Instructor)

```bash
curl -X POST http://localhost:8080/api/instructor/sessions/<session-id>/evaluate ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <instructor-token>" ^
  -d "{\"score\":85,\"comments\":\"Good control, minor lane deviation at turn 3.\"}"
```

### Step 9: View evaluation (as Trainee)

```bash
curl -H "Authorization: Bearer <trainee-token>" ^
  http://localhost:8080/api/trainee/evaluations
```

---

## Debugging

### Backend (VS Code)

1. Open `src/backend/SimApi/` in VS Code
2. Install **C# Dev Kit** extension
3. Press **F5** (select ".NET Core Launch" configuration)
4. Set breakpoints in controllers or services
5. Send HTTP requests to `http://localhost:5000`

### Backend (Rider / Visual Studio)

1. Open `src/backend/SimApi/SimApi.csproj` or the containing folder
2. Press **F5** — debugger attaches automatically
3. Use `Properties/launchSettings.json` to switch between profiles

### Frontend (Chrome DevTools)

1. Run `npm run dev`
2. Open Chrome DevTools (**F12**) → **Sources** tab
3. Set breakpoints in `.tsx` files under `app/` or `components/`
4. React DevTools extension for component state inspection

### Frontend (VS Code — attach browser debugger)

Create `.vscode/launch.json` at the project root:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "type": "chrome",
      "request": "launch",
      "name": "Frontend Debug",
      "url": "http://localhost:3000",
      "webRoot": "${workspaceFolder}/src/frontend/sim-web"
    }
  ]
}
```

### Frontend (Next.js 16 proxy debugging)

The `proxy.ts` file at the project root handles route protection. To debug it:

1. Add `console.log` or use `NextResponse` properties to inspect requests
2. In dev mode, changes to `proxy.ts` require a dev server restart
3. The proxy runs on Node.js runtime (not Edge). Standard `console` APIs work
4. Logs appear in the terminal where `npm run dev` is running

### Godot simulation

1. Open the project in Godot 4 editor
2. Attach the editor to the running game: **Debugger → Attach to Running Game**
3. Set breakpoints in C# scripts via the **Mono** tab in the debugger
4. Use `GD.Print("message")` — output appears in the **Output** panel
5. For network debugging, check the **Output** panel for `GD.PrintErr` messages from `BackendClient.cs`

### Docker logs

```bash
# Follow all services
docker compose -f docker/docker-compose.yml logs -f

# Follow a specific service
docker compose -f docker/docker-compose.yml logs -f backend
docker compose -f docker/docker-compose.yml logs -f frontend
docker compose -f docker/docker-compose.yml logs -f db

# View last N lines
docker compose -f docker/docker-compose.yml logs --tail=100 backend
```

### Database inspection

```bash
# Direct psql through Docker Compose
docker compose -f docker/docker-compose.yml exec db psql -U postgres -d simulation

# List tables
docker compose -f docker/docker-compose.yml exec db psql -U postgres -d simulation -c "\dt"

# Query users
docker compose -f docker/docker-compose.yml exec db psql -U postgres -d simulation -c "SELECT id, email, name, role FROM \"Users\";"

# Query sessions
docker compose -f docker/docker-compose.yml exec db psql -U postgres -d simulation -c "SELECT id, status, score FROM \"SimulationSessions\";"

# Or use pgAdmin
# Host: localhost, Port: 5432, User: postgres, Password: postgres, Database: simulation
```

---

## Docker Compose Reference

### Service matrix

| Service | Image / Build | Port | Depends on | Health check |
|---|---|---|---|---|
| `db` | `postgres:16` | `5432` | — | `pg_isready` |
| `backend` | `docker/Dockerfile.backend` | `8080` | `db` (healthy) | — |
| `frontend` | `docker/Dockerfile.frontend` | `3000` | `backend` | — |

### Useful commands

```bash
# Start everything
docker compose -f docker/docker-compose.yml up

# Start in background
docker compose -f docker/docker-compose.yml up -d

# Rebuild images and start
docker compose -f docker/docker-compose.yml up --build

# Start a single service
docker compose -f docker/docker-compose.yml up backend -d

# Stop all
docker compose -f docker/docker-compose.yml down

# Stop and delete volumes (resets database)
docker compose -f docker/docker-compose.yml down -v

# Rebuild a single service
docker compose -f docker/docker-compose.yml build backend
```

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `docker: command not found` | Docker Desktop not installed | Install from docker.com |
| `Connection refused:5432` | PostgreSQL not running | `docker compose up db -d` |
| `Could not find dotnet` | .NET SDK not installed | Install from dotnet.microsoft.com |
| `JWT validation failed` | Wrong `Jwt:Key` in config | Use the same key in backend and docker-compose |
| `CORS error` | Frontend port not allowed | Check `Program.cs` CORS config includes the frontend origin |
| `Tables not found` | DB not created | `EnsureCreated()` runs on startup; check backend logs |
| `Port already in use` | Another process on same port | Change port in `appsettings.json`, `.env.local`, or docker-compose |
| Frontend shows blank page | API URL misconfigured | Check `NEXT_PUBLIC_API_URL` in `.env.local` |
| Telemetry returns 401 | Missing or invalid JWT | Pass `--token <jwt>` to the Godot executable |
| Godot can't connect to API | Wrong `--api-url` or backend not running | Verify `--api-url` points to the running backend |
| `docker compose` not found | Old Docker version | Use `docker-compose` (with hyphen) instead, or upgrade |
| Vitest tests fail with jsdom errors | Vitest config missing `environment: 'jsdom'` or setup file missing jest-dom import | Ensure `vitest.config.mts` uses `environment: 'jsdom'` and the setup file (`lib/__tests__/setup.ts`) imports `@testing-library/jest-dom/vitest` |

---

## Common dotnet commands

```bash
dotnet build              # Compile (without running)
dotnet run                # Build + run
dotnet watch run          # Hot reload on file changes
dotnet test               # Run tests
dotnet ef migrations add  # Create EF migration
dotnet ef database update # Apply migrations
dotnet ef database drop   # Drop database
```

---

## Project Structure Reference

```
simulation/
├── src/
│   ├── backend/SimApi/         # ASP.NET Core 10 API
│   │   ├── Controllers/        # Auth, Admin, Instructor, Trainee, Telemetry
│   │   ├── Models/             # User, Organization, SimulationSession, etc.
│   │   ├── Services/           # JwtService, PasswordService
│   │   ├── Data/               # AppDbContext
│   │   ├── DTOs/               # Request/response types
│   │   └── Program.cs
│   └── frontend/sim-web/       # Next.js 16 app (App Router)
│       ├── app/                # Pages (login, admin/*, instructor/*, trainee/*)
│       ├── components/         # UI primitives + layout (sidebar, navbar)
│       ├── lib/                # API client, auth context
│       ├── lib/__tests__/      # Vitest test files + setup
│       ├── proxy.ts            # Route protection (Next.js 16 proxy)
│       ├── vitest.config.mts   # Vitest configuration (jsdom, path aliases)
│       └── .env.local          # NEXT_PUBLIC_API_URL
├── simulation/
│   └── driving-sim/            # Godot 4 + C# project
│       ├── Scenes/             # Main.tscn
│       ├── Scripts/            # VehicleController.cs, BackendClient.cs
│       ├── project.godot
│       └── export_presets.cfg
├── docker/
│   ├── docker-compose.yml      # db + backend + frontend
│   ├── Dockerfile.backend
│   └── Dockerfile.frontend
├── .memory/                    # Agent shared memory
├── agents/                     # Agent identity docs
└── docs/
    ├── 00-overview/
    ├── 01-product/             # MVP definition, features, roadmap
    ├── 02-architecture/
    ├── 03-ai-system/
    ├── 04-api/
    ├── 05-ui-ux/
    ├── 06-engineering/         # Run & debug, stack, coding standards
    └── 99-reference/
```
