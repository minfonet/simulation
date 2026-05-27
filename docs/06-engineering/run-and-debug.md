# Run & Debug Guide — MVP

## Prerequisites

### Required
| Dependency | Version | Check |
|---|---|---|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Latest | `docker --version && docker compose version` |
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ | `dotnet --version` |
| [Node.js](https://nodejs.org/) | 20+ | `node --version` |
| [Git](https://git-scm.com/) | Any | `git --version` |

### Optional
| Tool |用途 |
|---|---|
| [Godot 4 .NET](https://godotengine.org/download) | Open/simulation project (`simulation/driving-sim/`) |
| [VS Code](https://code.visualstudio.com/) + C# extension | Edit/debug backend |
| [Rider](https://www.jetbrains.com/rider/) | .NET IDE (alternative to VS) |
| [pgAdmin](https://www.pgadmin.org/) | Browse PostgreSQL directly |

---

## Quick Start (Docker — full system)

```bash
# From the project root
docker compose -f docker/docker-compose.yml up --build
```

This starts:
- **PostgreSQL 16** on `localhost:5432`
- **Backend API** on `http://localhost:8080`

The API auto-creates tables on first run (`EnsureCreated`).

---

## Running Backend Locally (without Docker)

### 1. Start PostgreSQL

Using Docker for just the database:

```bash
docker run -d --name sim-db ^
  -e POSTGRES_DB=simulation ^
  -e POSTGRES_USER=postgres ^
  -e POSTGRES_PASSWORD=postgres ^
  -p 5432:5432 ^
  postgres:16
```

### 2. Restore & Run

```bash
cd src/backend/SimApi
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000` (or `https://localhost:5001`).

### 3. Verify

```bash
# Health check — should return 200
curl http://localhost:5000/api/auth/login -X POST ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"test\",\"password\":\"test\"}"
```

---

## Running Frontend Locally

### 1. Install dependencies

```bash
cd src/frontend/sim-web
npm install
```

### 2. Start dev server

```bash
npm run dev
```

Opens at `http://localhost:3000`.

### 3. Configure API URL

Set `NEXT_PUBLIC_API_URL` in `.env.local`:

```
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## Running the Godot Simulation

### 1. Open the project

Open Godot 4, click **Import**, navigate to `simulation/driving-sim/` and select `project.godot`.

### 2. Run from editor

Press **F5** (or click Play). The simulation expects a `sessionId` argument:

```
godot --session-id <guid>
```

### 3. Export standalone

```bash
godot --headless --export-release Windows desktop/SimDriver.exe
```

### 4. Launch with session

```bash
SimDriver.exe --session-id 550e8400-e29b-41d4-a716-446655440000
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
3. Use `launchSettings.json` to switch between IIS Express and Kestrel profiles

### Frontend (VS Code / Chrome)

1. Run `npm run dev` (or use VS Code's built-in terminal)
2. Open Chrome DevTools (**F12**) → **Sources** tab
3. Set breakpoints in `.tsx` files under `src/`
4. React DevTools extension for component state inspection

### Frontend (VS Code — attach debugger)

1. Install **Debugger for Microsoft Edge** or **Debugger for Chrome**
2. Use this `.vscode/launch.json`:

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

### Docker logs

```bash
# Follow all services
docker compose -f docker/docker-compose.yml logs -f

# Follow a specific service
docker compose -f docker/docker-compose.yml logs -f backend

# Follow database
docker compose -f docker/docker-compose.yml logs -f db
```

### Database inspection

```bash
# Direct psql
docker exec -it sim-db psql -U postgres -d simulation

# Or using pgAdmin
# Host: localhost, Port: 5432, User: postgres, Password: postgres
```

### Common dotnet commands

```bash
dotnet build              # Compile (without running)
dotnet run                # Build + run
dotnet watch run          # Hot reload on file changes
dotnet test               # Run tests (when added)
dotnet ef migrations add  # Create EF migration
dotnet ef database update # Apply migrations
```

---

## Docker Compose Reference

| Service | Image | Port | Health check |
|---|---|---|---|
| `db` | `postgres:16` | `5432` | `pg_isready` |
| `backend` | Build from `Dockerfile.backend` | `8080` | — |
| `frontend` | (future) Build from `Dockerfile.frontend` | `3000` | — |

### Useful compose commands

```bash
# Start all services
docker compose -f docker/docker-compose.yml up

# Start in background
docker compose -f docker/docker-compose.yml up -d

# Rebuild and start
docker compose -f docker/docker-compose.yml up --build

# Stop all
docker compose -f docker/docker-compose.yml down

# Stop and delete volumes (resets database)
docker compose -f docker/docker-compose.yml down -v

# View logs
docker compose -f docker/docker-compose.yml logs -f backend
```

---

## Testing the API Manually

### 1. Create an organization

```bash
curl -X POST http://localhost:8080/api/admin/organizations ^
  -H "Content-Type: application/json" ^
  -d "{\"name\":\"ABC Driving School\"}"
```

### 2. Register as Admin

```bash
curl -X POST http://localhost:8080/api/auth/register ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"admin@abc.com\",\"password\":\"123456\",\"name\":\"Admin\",\"role\":\"Admin\",\"organizationId\":\"<org-id-from-step-1>\"}"
```

### 3. Login

```bash
curl -X POST http://localhost:8080/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"admin@abc.com\",\"password\":\"123456\"}"
```

Save the `accessToken` from the response. Use it in subsequent requests:

```bash
curl -H "Authorization: Bearer <token>" http://localhost:8080/api/admin/organizations
```

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `docker: command not found` | Docker Desktop not installed | Install from docker.com |
| `Connection refused:5432` | PostgreSQL not running | `docker compose up db` |
| `Could not find dotnet` | .NET SDK not installed | Install from dotnet.microsoft.com |
| `JWT validation failed` | Wrong `Jwt:Key` in config | Use the same key in backend and docker-compose |
| `CORS error` | Frontend port not allowed | Check CORS in `Program.cs` |
| `Tables not found` | DB not created | `EnsureCreated()` runs on startup, check logs |
| `Port already in use` | Another process on same port | Change port in `appsettings.json` or kill process |

---

## Project Structure Reference

```
simulation/
├── src/
│   ├── backend/SimApi/     # ASP.NET Core 8 API
│   ├── frontend/sim-web/   # Next.js 14 app
│   └── simulation/         # (future docs)
│       └── driving-sim/    # Godot 4 project
├── docker/
│   ├── docker-compose.yml
│   └── Dockerfile.backend
├── .memory/                # Agent shared memory
├── .opencode/agents/       # Agent definitions
├── agents/                 # Agent identity docs
└── docs/                   # All documentation
    ├── 00-overview/
    ├── 01-product/         # Features, SRD, roadmap, MVP definition
    ├── 02-architecture/
    ├── 03-ai-system/
    ├── 04-api/
    ├── 05-ui-ux/
    ├── 06-engineering/     # Stack, coding standards, run & debug
    └── 99-reference/
```
