# MVP — Minimum Viable Product

## 1. MVP Philosophy

Build the minimum functional system that crosses all architecture layers
(backend, frontend, simulation) to validate the complete flow:

```
Trainee → Simulates → Generates telemetry → Backend persists → Instructor evaluates → Trainee reviews
```

Without this vertical integration there is no validatable product.

---

## 2. MVP Scope (Minimum Vertical Complete)

| Layer | Includes | Excludes |
|---|---|---|
| **Backend** (ASP.NET Core) | JWT auth + roles (Admin, Instructor, Trainee), CRUD Organizations, CRUD Users, CRUD Sessions, telemetry ingestion, basic evaluation endpoint | TimescaleDB, real-time SignalR, replay, advanced analytics |
| **Frontend** (Next.js + TS) | Login, Admin Dashboard (manage orgs/users), Instructor Dashboard (create sessions, view trainees, evaluate), Trainee Dashboard (join session, view performance), Profile settings | Hardware config, calibration UI, complex presets |
| **Simulation** (Godot 4 + C#) | Basic 3D scene with a controllable vehicle (keyboard), telemetry emission (speed, steering, position, collisions), REST backend connection. P2: enhanced multi-piece vehicle, drift camera, HUD, improved physics feel | Physical hardware, AI traffic, weather, multiple scenarios, replay |
| **Infrastructure** | Docker Compose with backend + frontend + postgres + godot (headless or build export) | TimescaleDB, advanced orchestration, CDN |

---

## 3. Roles and Features per Role

### Admin
- Log in
- Create/edit/delete organizations
- Invite users (Instructors, Trainees) to an organization
- View session list for their organization
- Basic dashboard with aggregate metrics

### Instructor
- Log in
- View assigned trainees
- Create a simulation session (select scenario, assign trainee)
- View live session (status, basic telemetry)
- Finish session and issue evaluation (score, comments)
- View evaluation history

### Trainee
- Log in
- View assigned sessions
- Start simulation session (launches Godot)
- Drive in the 3D scenario
- Upon completion, view results and instructor evaluation

---

## 4. End-to-End Flow (Happy Path)

```
1. Admin creates organization "ABC Driving School"
2. Admin creates user "John" (Trainee) and "Instructor Mary" (Instructor)
3. Mary logs in, creates session "Practice 1 - Sharp Turn" and assigns John
4. John logs in, sees the assigned session, presses "Start simulation"
5. Godot opens (or loads in web), John drives until explicit/manual/controlled completion (no mandatory time limit)
6. Godot sends telemetry to backend POST /api/telemetry every 100ms
7. John finishes the simulation
8. Mary views recorded telemetry, assigns score and comments
9. John reviews his evaluation on the dashboard
```

---

## 5. Technology Stack (MVP)

| Component | Technology | Version |
|---|---|---|
| Backend API | ASP.NET Core | 8+ |
| ORM / DB | Entity Framework Core + PostgreSQL | 16+ |
| Authentication | JWT (access + refresh tokens) | — |
| Frontend | Next.js + TypeScript | 14+ |
| UI Framework | Tailwind CSS + shadcn/ui | — |
| Simulation | Godot 4 + C# | 4.x |
| Containers | Docker + Docker Compose | — |

---

## 6. MVP Folder Structure

```
simulation/
├── agents/                    # AI agent identities
│   ├── lead.md
│   ├── implementer.md
│   └── reviewer.md
├── .memory/                   # Organizational persistent memory
│   ├── active-context.md
│   ├── memories/
│   └── sessions/
├── src/
│   ├── backend/
│   │   └── SimApi/            # ASP.NET Core project
│   │       ├── Controllers/
│   │       ├── Models/
│   │       ├── Services/
│   │       ├── Data/
│   │       └── Program.cs
│   └── frontend/
│       └── sim-web/           # Next.js project
│           ├── app/
│           ├── components/
│           ├── lib/
│           └── package.json
├── simulation/                # Godot 4 project
│   └── driving-sim/
│       ├── Scenes/
│       ├── Scripts/
│       ├── project.godot
│       └── export_presets.cfg
├── docker/
│   ├── Dockerfile.backend
│   ├── Dockerfile.frontend
│   └── docker-compose.yml
├── docs/                      # Documentation
└── README.md
```

---

## 7. API Endpoints (MVP)

### Auth
| Method | Route | Description |
|---|---|---|
| POST | /api/auth/register | Register user |
| POST | /api/auth/login | Login, returns JWT |
| POST | /api/auth/refresh | Refresh token |

### Admin
| Method | Route | Description |
|---|---|---|
| GET | /api/admin/organizations | List organizations |
| POST | /api/admin/organizations | Create organization |
| PUT | /api/admin/organizations/{id} | Edit organization |
| DELETE | /api/admin/organizations/{id} | Delete organization |
| GET | /api/admin/organizations/{id}/users | List users in an org |
| POST | /api/admin/organizations/{id}/users | Invite user to an org |

### Instructor
| Method | Route | Description |
|---|---|---|
| GET | /api/instructor/sessions | List instructor's sessions |
| POST | /api/instructor/sessions | Create session |
| GET | /api/instructor/sessions/{id} | View session details |
| POST | /api/instructor/sessions/{id}/evaluate | Evaluate session |
| GET | /api/instructor/trainees | List assigned trainees |
| GET | /api/instructor/evaluations | View issued evaluations |

### Trainee
| Method | Route | Description |
|---|---|---|
| GET | /api/trainee/sessions | List assigned sessions |
| GET | /api/trainee/sessions/{id} | View session details |
| POST | /api/trainee/sessions/{id}/start | Start simulation |
| POST | /api/trainee/sessions/{id}/finish | Finish simulation |
| GET | /api/trainee/evaluations | View received evaluations |

### Telemetry
| Method | Route | Description |
|---|---|---|
| POST | /api/telemetry | Ingest telemetry batch |
| GET | /api/telemetry/session/{sessionId} | Get session telemetry |

---

## 8. Data Models (MVP)

### Organization
```
Id (Guid), Name, CreatedAt
```

### User
```
Id (Guid), Email, PasswordHash, Name, Role (Admin|Instructor|Trainee),
OrganizationId (FK), CreatedAt
```

### SimulationSession
```
Id (Guid), OrganizationId (FK), InstructorId (FK), TraineeId (FK),
Scenario, Status (Pending|Active|Completed|Failed),
Score (nullable), InstructorNotes, CreatedAt, CompletedAt
```

### TelemetryRecord
```
Id (Guid), SessionId (FK), Timestamp, Speed, SteeringAngle,
PositionX, PositionY, PositionZ, Collision (bool), RawData (jsonb)
```

### Evaluation
```
Id (Guid), SessionId (FK), InstructorId (FK), Score,
Comments, CreatedAt
```

---

## 9. Godot MVP Simulation (driving-sim)

### Minimum scene (P0 — basic)
- Base plane with asphalt texture
- A vehicle (cube or simple model) with physics
- Controls: WASD + space (brake)
- Fixed or following third-person camera
- Cones or walls as simple obstacles

### Enhanced scene (P2 — driving experience, in priority order)
1. **Cockpit interior** — driver's-eye camera (fixed at ~0.6, 0.5, 0.3), steering wheel mesh that rotates with input, dashboard mesh, driver seat mesh. **This is the default view.**
2. **CanvasLayer HUD** — speedometer, steering indicator, controls hint, "Finish Simulation" button
3. **Third-person drift camera** — smooth follow with drift lean, look-ahead, toggleable with C key
4. **Improved physics** — lift-off oversteer, PID-style regulation, weight transfer, tunable exported properties
5. **WorldEnvironment** — sky with ProceduralSkyMaterial, fog, improved lighting
6. Post-MVP: rear-view mirrors (Viewport-based), approach/walk animation

### Emitted telemetry
Every frame (or every N frames):
- Current speed
- Steering angle
- Position (x, y, z)
- Collisions (on_body_entered event)

### Backend connection
- On start, receives sessionId as command line argument
- Sends POST /api/telemetry with batch every ~100-500ms
- On finish, sends POST /api/trainee/sessions/{id}/finish

---

## 10. MVP Success Criteria

1. An Admin can create an organization and add Instructor + Trainee
2. An Instructor can create a session and assign a Trainee
3. A Trainee can start the Godot simulation from the frontend
4. The simulation sends telemetry to the backend in real time (no SignalR, via REST batch)
5. The Instructor can view telemetry and evaluate the Trainee
6. The Trainee can view their evaluation
7. Everything runs with `docker compose up`

---

## 11. What is NOT in the MVP (post-MVP)

- Physical hardware (steering wheels, pedals, joysticks)
- Real-time SignalR / WebSockets
- TimescaleDB (vanilla PostgreSQL for MVP)
- AI traffic / autonomous vehicles
- Replay system
- Multiple scenarios / scenario editor
- Multiplayer / shared sessions
- Advanced analytics / reports
- Custom hardware SDK
- Industry-specific support (aviation, maritime, etc.)
- AI guardrails
- Hardware calibration interface
- Versioned presets


# Basic Prompts
## Run 1:
Lead, start MVP phase 1: backend scaffolding with ASP.NET Core and Docker Compose

## Run 2:
Lead, continue with the MVP. Next task: Next.js frontend scaffolding

## Run 3:
Lead, continue with the MVP. Next task: Godot 4 simulator scaffolding

## Run 4
Set up frontend test infrastructure (Vitest) in sim-web, then write and run tests for api.ts, auth-context.tsx, and proxy.ts. Follow the workflow: delegate test infra setup to QA, then delegate test writing to QA, then review. Report coverage results.
