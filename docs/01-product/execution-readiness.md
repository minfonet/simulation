# Execution Readiness — Flow-by-Flow Analysis

> Last updated: 2026-05-28
> Current test status: **120/120 passing** (backend 66, frontend 23, Godot 12, E2E 19)
> Milestones: P0 ✅ | P1 ✅

---

## Purpose

This document analyses each user flow requested for the MVP and determines exactly what is **ready to execute** vs what **remains pending** to run that flow end-to-end in a real environment. Each flow includes a step-by-step execution procedure and a validation checklist.

---

## Flow 1 — Admin Creates Organization

### Status: ✅ **READY TO EXECUTE** (self-service via UI)

### What's implemented
- `POST /api/auth/register` — register as Admin (bootstrap org auto-seeded on first startup)
- Signup/Signin toggle on `/login` page — users can sign up directly from the browser
- `POST /api/admin/organizations` — creates org with 201
- `GET /api/admin/organizations` — lists orgs
- `DELETE /api/admin/organizations/{id}` — deletes org
- Frontend page at `/admin/organizations` with create + delete UI
- Backend integration tests cover success, auth, and validation

### How to execute
1. Start Docker Compose: `cd docker && docker compose up -d`
2. Open browser at `http://localhost:3000/login`
3. Click **"Don't have an account? Sign up"**
4. Enter Name, Email, Password → Click **"Sign up"**
5. Automatically redirected to `/admin` dashboard
6. Navigate to `/admin/organizations`
7. Enter organization name → Click "Create"
8. Verify org appears in list

### Pending to complete this flow

| Item | Impact | Effort |
|------|--------|--------|
| Frontend page component tests | Low (features work) | 1 session |

---

## Flow 2 — Instructor/Evaluator Loads Base Scene and Creates Session

### Status: ✅ **READY TO EXECUTE** (with minor notes)

### What's implemented
- `GET /api/instructor/scenario-presets` — returns `[{id:"default", name:"Default", ...}]`
- `POST /api/instructor/sessions` — validates preset, creates Pending session
- `ScenarioPresetStore` — singleton with `"default"` preset mapping to `res://Scenes/Main.tscn`
- Frontend dropdown selector on `/instructor/sessions` page populated from presets endpoint
- Backend integration tests for preset validation (valid preset → 201, invalid → 400)

### How to execute
1. Login as Instructor
2. Navigate to `/instructor/sessions`
3. Select a trainee from the dropdown
4. Select preset `"Default"` from the dropdown (description shown)
5. Click "Create Session"
6. Verify session appears in list with status `Pending`

### Pending to complete this flow

| Item | Impact | Effort |
|------|--------|--------|
| Integration test for `GET /api/instructor/scenario-presets` | Low (endpoint works, manual test possible) | 15 min |
| Multiple presets (currently only "default") | Low (MVP scope is 1 preset) | Post-MVP |

---

## Flow 3 — Trainee Accesses Simulation Scene

### Status: ✅ **READY TO EXECUTE** (manual Godot launch)

### What's implemented
- `POST /api/trainee/sessions/{id}/start` — transitions Pending → Active, returns `{id, apiUrl, status}`
- Frontend launch card at `/trainee/sessions/{id}` with:
  - CLI command: `godot --session-id {id} --api-url {url} --token {token}`
  - "Copy Command" button (clipboard API)
  - "Download Launch Script" button (PowerShell .ps1)
  - Token security warning
- Godot `BackendClient.ReadSessionId()` parses `--session-id`, `--token`, `--api-url`
- Godot `BackendClient.StartSession()` calls `POST /api/trainee/sessions/{id}/start` with auth
- No mandatory time limit enforced

### How to execute
1. Login as Trainee
2. Navigate to `/trainee/sessions`
3. Click "Start" on a Pending session
4. Session status changes to `Active`
5. Launch card appears with CLI command
6. Copy the command or download the `.ps1` script
7. Run Godot locally with the arguments:
   ```
   godot --session-id {id} --api-url http://localhost:8080 --token {jwt-token}
   ```
8. Godot starts the session via API, collects telemetry, and sends batches

### Pending to complete this flow

| Item | Impact | Effort |
|------|--------|--------|
| Godot must be installed locally on the evaluator's machine | Medium (required for real simulation) | Setup |
| No auto-launch from browser (CLI command only) | Low (by MVP design) | Post-MVP |
| No test for Godot argument parsing in E2E script | Low (unit tests exist: 12/12) | Add to E2E script |
| E2E smoke test doesn't cover launch handoff (would need Godot) | Low (script simulates telemetry) | Documented limitation |

---

## Flow 4 — Collect Session Information (Telemetry + Critical Events)

### Status: ✅ **READY TO EXECUTE**

### What's implemented
- `POST /api/telemetry` — ingests telemetry batch, auto-generates CriticalEvent for each `collision=true`
  - Response now includes `{ ingested, criticalEvents }` counts
- `GET /api/telemetry/session/{sessionId}` — returns telemetry records ordered by timestamp
- `GET /api/telemetry/session/{sessionId}/events` — returns critical events ordered by timestamp
- `CriticalEvent` model: SessionId, Timestamp, EventType ("collision"), Severity ("medium"), Metadata (JSON with speed/steeringAngle/position)
- `ITelemetryIngestor` / `ITelemetryStore` boundaries (architecture compliant)
- 7 integration tests covering: collision counting, event type/severity, metadata shape, empty events, 404, timestamp ordering

### How to execute
1. Trainee starts session (Flow 3)
2. Send telemetry with `collision: true` (from Godot or direct API call)
3. Verify `POST /api/telemetry` response includes `criticalEvents` count
4. Query `GET /api/telemetry/session/{id}/events` — returns collision events
5. Query `GET /api/telemetry/session/{id}` — returns raw telemetry points

### Pending to complete this flow

| Item | Impact | Effort |
|------|--------|--------|
| No Godot → Docker Compose E2E telemetry test | Medium (real integration not validated) | 1 session |
| Collision deduplication not implemented | Low (acceptable for MVP) | Post-MVP |

---

## Flow 5 — Evaluator Gets Session Report and Grades

### Status: ✅ **READY TO EXECUTE**

### What's implemented
- `GET /api/instructor/sessions/{id}/report` — returns `SessionReportResponse` with:
  - Telemetry summary (total points, avg/max/min speed)
  - Collision count
  - Critical events list (with speed extracted from metadata)
  - Evaluation info (score, notes, instructor name)
  - Validates: ownership (instructor must own session), status (must be Completed)
- `GET /api/trainee/sessions/{id}/report` — same report for trainee view
- Frontend report card on both Instructor and Trainee session detail pages:
  - 5-metric grid (telemetry count, avg/max/min speed, collisions)
  - Critical events list with severity coloring
  - Evaluation section (when evaluated)
- 9 integration tests covering: report data accuracy, active session → 400, ownership isolation, evaluation integration

### How to execute
1. After trainee finishes session → status is `Completed`
2. Login as Instructor → navigate to `/instructor/sessions/{id}`
3. Report card appears automatically with telemetry summary
4. Critical events shown if collisions occurred
5. Fill score + comments → Click "Submit Evaluation"
6. Report updates to show `isEvaluated: true`, score, notes
7. Login as Trainee → navigate to `/trainee/sessions/{id}`
8. Report card shows score and instructor notes

### Pending to complete this flow

| Item | Impact | Effort |
|------|--------|--------|
| No E2E test for report endpoint in smoke script | Medium (no automated validation) | Extend script |

---

## Consolidated Pending Items

### ✅ Completed

| # | Item | Status |
|---|------|--------|
| 1 | **Fix smoke-test.ps1** — change `scenario = "smoke-test"` to `"default"` | ✅ Done |
| 2 | **Extend smoke-test.ps1** — add critical events + report validation (6 new steps) | ✅ Done |
| – | **Execute E2E smoke test against live Docker Compose** | ✅ 19/19 PASS |

### Should fix for completeness

| # | Item | Why | File | Effort |
|---|------|-----|------|--------|
| 3 | Integration test for `GET /api/instructor/scenario-presets` | QA coverage gap | `InstructorControllerTests.cs` | 15 min |
| 4 | Fix 4 pre-existing TypeScript errors in `api.test.ts` | Clean compilation | `api.test.ts` | 15 min |
| 5 | Frontend page component tests | No UI regression safety | 6 new test files | 1-2 sessions |

### Infrastructure required

| # | Item | Why |
|---|------|-----|
| 6 | Docker Compose running (`docker compose up -d`) | Required for services (✅ already running) |
| 7 | Godot 4.6.3 mono installed locally | Required for real simulation (manual step) |
| 8 | .NET SDK 10 and Node.js installed | Development prerequisites (already ✅) |

---

## Quick-Start: Full E2E Execution Procedure

To run the **complete end-to-end flow** (all 5 flows) in one session:

```powershell
# 1. Prerequisites
#    - Docker Desktop installed and running
#    - Godot 4.6.3 mono installed (for real simulation)

# 2. Start services
cd docker
docker compose up -d
# Wait for: postgres healthy + backend responding on :8080

# 3. Run smoke test
cd ..
.\tests\e2e\smoke-test.ps1

# 4. Manual UI validation (optional, via browser at http://localhost:3000)
#    - Login as Admin → create org → invite users
#    - Login as Instructor → create session with "default" preset
#    - Login as Trainee → start session → copy launch command
#    - Run Godot locally with the command
#    - Finish session → Instructor views report → evaluates
```

### Expected smoke test output:
```
=== 1. Health Check ===
  [PASS] Health endpoint returns healthy
=== 2. Bootstrap Organization ===
  [PASS] Bootstrap organization exists
=== 3. Register Bootstrap Admin ===
  [PASS] Bootstrap admin registered
=== 4. Create Organization ===
  [PASS] Organization created
=== 5. Invite Instructor and Trainee ===
  [PASS] Instructor created
  [PASS] Trainee created
=== 6. Login as Instructor and Trainee ===
  [PASS] Instructor logged in
  [PASS] Trainee logged in
=== 7. Create Session ===
  [PASS] Session created (status: Pending)
=== 8. Start Session ===
  [PASS] Session started
=== 9. Send Telemetry ===
  [PASS] Telemetry ingested: 2 points (0 critical events)
=== 9b. Send Telemetry with Collision ===
  [PASS] Telemetry ingested: 1 points (1 critical events)
=== 9c. Get Critical Events ===
  [PASS] Retrieved 1 critical events
=== 10. Finish Session ===
  [PASS] Session finished
=== 11. Instructor Report ===
  [PASS] Report has sessionId, totalTelemetryPoints=3, collisionCount=1, criticalEvents=1
=== 12. Evaluate Session ===
  [PASS] Session evaluated
=== 13. Retrieve Telemetry ===
  [PASS] Retrieved 3 telemetry records
=== 14. Trainee Report ===
  [PASS] Report shows score, isEvaluated=true
=== 15. Verify Auth/Me ===
  [PASS] Auth/me returns instructor identity
```

---

## Per-Flow Validation Checklist

Use this checklist when executing each flow manually or via automation.

### Flow 1: Admin creates organization
- [ ] `POST /api/admin/organizations` → 201 with org `id`
- [ ] Org appears in `GET /api/admin/organizations` list
- [ ] Non-admin gets 403
- [ ] UI shows org in table after creation

### Flow 2: Instructor creates session with default preset
- [ ] `GET /api/instructor/scenario-presets` → array with at least `{"id":"default","name":"Default"}`
- [ ] `POST /api/instructor/sessions` with `scenario:"default"` → 201, status `Pending`
- [ ] `POST /api/instructor/sessions` with `scenario:"invalid"` → 400
- [ ] Frontend dropdown shows presets, default is selectable

### Flow 3: Trainee accesses and runs simulation
- [ ] `GET /api/trainee/sessions` shows assigned Pending sessions
- [ ] `POST /api/trainee/sessions/{id}/start` → 200, status `Active`, includes `apiUrl`
- [ ] Frontend shows launch card with CLI command
- [ ] Godot parses `--session-id`, `--api-url`, `--token` correctly (unit tested)
- [ ] Session has no mandatory time limit

### Flow 4: Collect telemetry and critical events
- [ ] `POST /api/telemetry` with collision=true → `criticalEvents` count > 0
- [ ] `POST /api/telemetry` with all collision=false → `criticalEvents` count = 0
- [ ] `GET /api/telemetry/session/{id}/events` → events with `eventType:"collision"`, `severity:"medium"`, `metadata` containing speed
- [ ] `GET /api/telemetry/session/{id}/events` for nonexistent session → 404
- [ ] `GET /api/telemetry/session/{id}` → telemetry points ordered by timestamp

### Flow 5: Evaluator reviews report and grades
- [ ] `GET /api/instructor/sessions/{id}/report` for completed session → 200 with `totalTelemetryPoints`, `collisionCount`, `criticalEvents`
- [ ] Active session report → 400 "Session must be completed"
- [ ] Other instructor's session → 404
- [ ] After evaluation: `isEvaluated: true`, `score`, `instructorNotes` populated
- [ ] `GET /api/trainee/sessions/{id}/report` for completed session → 200 with score
- [ ] Frontend shows report card with summary stats + events + evaluation
