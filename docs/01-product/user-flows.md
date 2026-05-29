# User Flows — MVP

---

## 1. Authentication

### 1.1 Sign Up (Admin self-registration)

```
[User] → Opens /login → Clicks "Don't have an account? Sign up"
                          │
                  Enters name, email, password
                          │
                  POST /api/auth/register { name, email, password, role: "Admin", organizationId: bootstrapOrgId }
                          │
                     ┌────┴────┐
                     │ 200 OK  │ 409 (email exists)
                     └────┬────┘ └──→ Show error
                          │
           Store accessToken + refreshToken + user in localStorage
                          │
              Redirect to /admin (auto-logged in)
```

**Frontend**: `app/login/page.tsx` (signup toggle)  
**Backend**: `POST /api/auth/register` → `AuthController.Register()`  
**Persistence**: `User` table, JWT in localStorage  
**Note**: Bootstrap org (`00000000-0000-0000-0000-000000000001`) is auto-seeded in `Program.cs` on first startup

### 1.2 Login

```
[User] → Opens /login → Enters email + password → POST /api/auth/login
                                                          │
                                                     ┌────┴────┐
                                                     │ 200 OK  │ 401
                                                     └────┬────┘ └──→ Show error
                                                          │
                                           Store accessToken + refreshToken
                                           in localStorage
                                                          │
                                               Redirect to role dashboard:
                                               Admin → /admin
                                               Instructor → /instructor
                                               Trainee → /trainee
```

**Frontend**: `app/login/page.tsx`  
**Backend**: `POST /api/auth/login` → `AuthController.Login()`  
**Persistence**: `User` table, JWT in localStorage

### 1.3 Logout

```
[User] → Clicks "Sign out" → localStorage.clear()
                              Redirect to /login
```

**Frontend**: `components/layout/sidebar.tsx` → `useAuth().logout()`

### 1.4 Token refresh (automatic)

```
[API call returns 401] → Retry with refreshToken
                          POST /api/auth/refresh
                              │
                         ┌────┴────┐
                         │ 200 OK  │ 401 → Force logout
                         └────┬────┘
                              │
                    Store new tokens → Retry original request
```

**Frontend**: `lib/api.ts` (interceptor not yet implemented — direct call for MVP)  
**Backend**: `POST /api/auth/refresh` → `AuthController.Refresh()`

---

## 2. Admin Flows

### 2.1 Create Organization

```
[Admin] → /admin/organizations → Enters org name → Clicks "Create"
                                                      │
                                          POST /api/admin/organizations
                                                      │
                                                    201
                                                      │
                                              Org appears in list
```

**Frontend**: `app/admin/organizations/page.tsx`  
**Backend**: `POST /api/admin/organizations` → `AdminController.CreateOrganization()`  
**Persistence**: `Organization` table

### 2.2 Delete Organization

```
[Admin] → /admin/organizations → Clicks "Delete" on an org
                                    │
                          DELETE /api/admin/organizations/{id}
                                    │
                                  204
                                    │
                          Org removed from list
```

**Frontend**: `app/admin/organizations/page.tsx`  
**Backend**: `DELETE /api/admin/organizations/{id}` → `AdminController.DeleteOrganization()`

### 2.3 Invite User to Organization

```
[Admin] → /admin/users
            │
      Select organization from dropdown
            │
      Fill: email, password, name, role
            │
      Click "Invite User"
            │
      POST /api/admin/organizations/{id}/users
            │
          200
            │
      User appears in list below the form
```

**Frontend**: `app/admin/users/page.tsx`  
**Backend**: `POST /api/admin/organizations/{id}/users` → `AdminController.InviteUser()`  
**Persistence**: `User` table

### 2.4 View Admin Dashboard

```
[Admin] → /admin
            │
      GET /api/admin/organizations (list)
            │
      Display: total orgs count + total users count
```

**Frontend**: `app/admin/page.tsx`  
**Backend**: `GET /api/admin/organizations` → `AdminController.GetOrganizations()`

---

## 3. Instructor Flows

### 3.1 Create Session

```
[Instructor] → /instructor/sessions
                  │
          Select trainee from dropdown
          Select base preset from dropdown (e.g. "Default")
                  │
          Click "Create Session"
                  │
          POST /api/instructor/sessions { traineeId, scenario: "default" }
                  │
                201
                  │
          Session appears in list (status: Pending)
```

**Frontend**: `app/instructor/sessions/page.tsx`  
**Backend**: `POST /api/instructor/sessions` → `InstructorController.CreateSession()`  
**Persistence**: `SimulationSession` table

### 3.2 View Sessions

```
[Instructor] → /instructor/sessions
                  │
          GET /api/instructor/sessions
                  │
          List: trainee name, scenario, status badge, score
                  │
          Click a session → /instructor/sessions/{id}
```

**Frontend**: `app/instructor/sessions/page.tsx`  
**Backend**: `GET /api/instructor/sessions` → `InstructorController.GetSessions()`

### 3.3 Monitor Session + Evaluate

```
[Instructor] → /instructor/sessions/{id}
                  │
          GET /api/instructor/sessions/{id}
          GET /api/telemetry/session/{sessionId}
                  │
          View: status badge, scenario, telemetry table
                  │
          ┌── If session is "Completed": ──────────────┐
          │  Fill: score (0-100) + comments             │
          │  Click "Submit Evaluation"                  │
          │  POST /api/instructor/sessions/{id}/evaluate│
          │  Score + notes saved to session             │
          └─────────────────────────────────────────────┘
```

**Frontend**: `app/instructor/sessions/[id]/page.tsx`  
**Backend**: `POST /api/instructor/sessions/{id}/evaluate` → `InstructorController.EvaluateSession()`  
**Persistence**: `Evaluation` table + `SimulationSession.Score/InstructorNotes`

### 3.4 View Evaluation History

```
[Instructor] → /instructor/evaluations
                  │
          GET /api/instructor/evaluations
                  │
          List: score, comments, date
```

**Frontend**: `app/instructor/evaluations/page.tsx`  
**Backend**: `GET /api/instructor/evaluations` → `InstructorController.GetEvaluations()`

### 3.5 View Assigned Trainees

```
[Instructor] → /instructor (dashboard)
                  │
          GET /api/instructor/trainees
                  │
          Display: list of trainees in same organization
```

**Frontend**: `app/instructor/page.tsx`  
**Backend**: `GET /api/instructor/trainees` → `InstructorController.GetTrainees()`

---

## 4. Trainee Flows

### 4.1 View Assigned Sessions

```
[Trainee] → /trainee/sessions
               │
        GET /api/trainee/sessions
               │
        List: scenario, instructor name, status badge, score
               │
        If Pending  → "Start" button
        If Completed → "View" button (links to detail)
```

**Frontend**: `app/trainee/sessions/page.tsx`  
**Backend**: `GET /api/trainee/sessions` → `TraineeController.GetSessions()`

### 4.2 Start Simulation

```
[Trainee] → /trainee/sessions
               │
        Click "Start" on a Pending session
               │
        POST /api/trainee/sessions/{id}/start
               │
              200
               │
          Status changes to "Active"
                  │
          Launch card appears with:
          • CLI command: godot --session-id {id} --api-url {url} --token {token}
          • "Copy Command" button
          • "Download Launch Script" (.ps1) button
                  │
          Trainee copies command or runs script locally
                  │
          Godot parses arguments via BackendClient.ReadSessionId()
                  │
          Godot calls POST /api/trainee/sessions/{id}/start (re-entrant)
          to activate session from within the engine
```

**Frontend**: `app/trainee/sessions/[id]/page.tsx` (launch card)  
**Backend**: `POST /api/trainee/sessions/{id}/start` → `TraineeController.StartSession()`  
**Persistence**: `SimulationSession.Status` → `Active`  
**Godot**: `BackendClient.ReadSessionId()` parses `--session-id`, `--api-url`, `--token`

### 4.3 Finish Simulation

```
[Trainee] → /trainee/sessions (or Godot calls API directly)
               │
        POST /api/trainee/sessions/{id}/finish
               │
              200
               │
        Status changes to "Completed"
        CompletedAt timestamp set
```

**Frontend**: `app/trainee/sessions/[id]/page.tsx` or `BackendClient.FinishSession()`  
**Backend**: `POST /api/trainee/sessions/{id}/finish` → `TraineeController.FinishSession()`  
**Persistence**: `SimulationSession.Status` → `Completed`, `CompletedAt`

### 4.4 View Session Detail (Telemetry + Evaluation + Report)

```
[Trainee] → /trainee/sessions/{id}
               │
        GET /api/trainee/sessions/{id}
        GET /api/telemetry/session/{sessionId}
        GET /api/trainee/sessions/{id}/report (if Completed)
               │
        View: status, instructor, scenario, score
              telemetry table (last 50 points)
              report card with summary metrics (if Completed):
              • total telemetry points
              • avg/max/min speed
              • collision count
              • critical events list
              instructor notes (if evaluated)
```

**Frontend**: `app/trainee/sessions/[id]/page.tsx` (includes report card component)  
**Backend**: `GET /api/trainee/sessions/{id}`, `GET /api/telemetry/session/{sessionId}`, `GET /api/trainee/sessions/{id}/report`

### 4.5 View Evaluations

```
[Trainee] → /trainee/evaluations
               │
        GET /api/trainee/evaluations
               │
        List: instructor name, score, comments, date
        Color badge: green (≥70%), yellow (≥40%), red (<40%)
```

**Frontend**: `app/trainee/evaluations/page.tsx`  
**Backend**: `GET /api/trainee/evaluations` → `TraineeController.GetEvaluations()`

---

## 5. Complete E2E Happy Path

```
┌────────────────────────────────────────────────────────────────────┐
│ 1. Admin creates organization "ABC Driving School"                 │
│    POST /api/admin/organizations                                   │
├────────────────────────────────────────────────────────────────────┤
│ 2. Admin invites Instructor (Mary) and Trainee (John)              │
│    POST /api/admin/organizations/{id}/users  (×2)                  │
├────────────────────────────────────────────────────────────────────┤
│ 3. Mary logs in → /instructor/sessions                             │
│    Creates session "Practice 1 - Sharp Turn" → assigns John        │
│    POST /api/instructor/sessions                                   │
├────────────────────────────────────────────────────────────────────┤
│ 4. John logs in → /trainee/sessions                                │
│    Sees the session → clicks "Start"                               │
│    POST /api/trainee/sessions/{id}/start                           │
├────────────────────────────────────────────────────────────────────┤
│ 5. John drives in Godot simulation                                 │
│    Godot sends telemetry every ~10 frames (100ms)                  │
│    POST /api/telemetry (batch)                                     │
│    Collisions auto-record critical events                          │
├────────────────────────────────────────────────────────────────────┤
│ 6. John finishes simulation                                        │
│    POST /api/trainee/sessions/{id}/finish                          │
├────────────────────────────────────────────────────────────────────┤
│ 7. Mary views session → /instructor/sessions/{id}                  │
│    Reviews telemetry, fills score=85, comments="Good control"      │
│    POST /api/instructor/sessions/{id}/evaluate                     │
├────────────────────────────────────────────────────────────────────┤
│ 8. John views evaluation → /trainee/evaluations                    │
│    Sees score 85% + instructor notes                               │
└────────────────────────────────────────────────────────────────────┘
```

---

## 6. Target Evaluation Flow / Requested Target Flows

This section documents the requested target operating flow for the MVP and compares it with the current state. Scope remains minimal: one functional base preset, basic launch/handoff to Godot, a test with no time limit, minimal critical events, and a basic report for manual grading by Evaluator/Instructor.

### 6.1 Target flow summary

```
1. Admin creates an organization.
2. Admin invites users with Evaluator/Instructor and Evaluated/Trainee roles.
3. Evaluator/Instructor defines an evaluation:
   - selects the Evaluated/Trainee;
   - selects a functional base scenario preset, for example `default`/`base`;
   - creates a Pending session associated with the preset.
4. Evaluated/Trainee enters the portal, opens a Pending session, and launches Godot.
5. The handoff to Godot passes `sessionId`, `apiUrl`, and an authorization/token strategy.
6. Godot runs the test with no product time limit; the user/session decides when it ends.
7. During the simulation, telemetry is ingested and minimal critical events are recorded.
8. After finishing, the Evaluator/Instructor opens the basic report, reviews events/telemetry, and grades the session.
9. Evaluated/Trainee views the score, comments, and final status.
```

### 6.2 Current state vs target/gap (post-P0/P1)

| Flow | Current state (P0+P1 ✅) | MVP target | Remaining gap |
|---|---|---|---|
| Admin creates organization | Backend 201/validation/auth tested. Frontend page exists with create/delete. E2E smoke covers path (not yet run against live Docker). | Admin can create an organization and see UI confirmation. | Frontend page component tests; E2E smoke execution with live services. |
| Evaluator defines evaluation | `ScenarioPresetStore` with `"default"` preset. `POST /api/instructor/sessions` validates preset → 201 or 400. Frontend dropdown selector populated from `GET /api/instructor/scenario-presets`. | Instructor chooses an explicit, versionable base preset, then creates a pending session. | Integration test for presets endpoint; multiple presets (post-MVP). |
| Evaluated launches Godot | Frontend launch card shows CLI command + download script. Godot `BackendClient.ReadSessionId()` parses `--session-id`, `--api-url`, `--token`. Start is re-entrant (API + Godot can both call it). | Start button initiates a real handoff to Godot with `sessionId`, `apiUrl`, and authorization. | No auto-launch (by MVP design); Godot must be installed locally. |
| Test with no time limit | No time limit enforced in code. Session ends by explicit finish action. | There is no product time limit for the MVP; finishing is an explicit user/simulation action. | Acceptance must ensure no mandatory timeout is added accidentally. ✅ Verified. |
| Critical events | `POST /api/telemetry` auto-derives `CriticalEvent` for each `collision=true`. Response returns `{ ingested, criticalEvents }`. `GET /api/telemetry/session/{id}/events` returns events ordered by timestamp. 7 integration tests. | Record minimal critical events, at least collision, with timestamp/severity/type for the report. | Collision deduplication (post-MVP). |
| Final report | `GET /api/*/sessions/{id}/report` returns telemetry summary, collision count, critical events, evaluation. Report card component on both Instructor/Trainee session detail pages. 9 integration tests. | Instructor sees a basic report with session summary, critical events, and relevant telemetry before grading. | E2E smoke script does not include report validation step. |
| Tests | Backend **66/66**, frontend lib **23/23**, Godot client **12/12**; E2E smoke syntax validated. P0/P1 boundary tests exist (telemetry ingestor/store, events, reports). | Tests verify the real Admin → Instructor → Trainee → Godot/handoff → telemetry/events → report/evaluation flow. | Page component tests; scenario-presets endpoint test; smoke script fix + extend; E2E execution with live services. |

### 6.3 Verifiable acceptance criteria by flow

#### Admin creates organization

- Given an authenticated Admin user, when they create an organization with a valid name, the backend responds with `201` and the organization appears in the UI list.
- The flow rejects non-Admin users with `401/403` as appropriate.
- Backend authorization/creation tests and page or component tests cover success and visible error states.

#### Evaluator/Instructor defines evaluation with base preset

- Given an authenticated Instructor, the UI shows at least one functional base preset (`default`/`base`) from an explicit contract, not unvalidated free text.
- When creating the session, the backend persists a stable preset reference and the session remains `Pending`.
- The selected preset has documented mapping to the existing MVP Godot scene/configuration.
- The preset contract is covered by backend/frontend tests; if Godot consumes the value, a producer/consumer compatibility test must exist.

#### Evaluated/Trainee launches Godot and runs an untimed test

- Given an authenticated Trainee with a `Pending` session, pressing Start activates the session and performs a verifiable handoff to Godot.
- The handoff includes `sessionId`, `apiUrl`, and an explicit authorization/token strategy for Godot; it must not depend on embedded credentials.
- Godot can start the session and send authorized telemetry for that session.
- There is no mandatory product timeout for finishing; the session ends through an explicit finish/controlled-close action.
- Launch errors or invalid tokens are shown to the user and do not leave the session in an ambiguous unmanaged state.

#### Critical events and final report

- During or after the simulation, minimal critical events are recorded, at least collisions, with `sessionId`, timestamp, type, and basic severity.
- The Evaluator can open a basic report for a completed session with summary, critical events, and references to relevant telemetry.
- The Evaluator can save score/comments after reviewing the report.
- The Trainee can view final score/comments.
- Contract and authorization tests exist for telemetry, critical event, and report ingestion/query.

### 6.4 Required boundaries for the missing work

- **Telemetry pipeline**: new telemetry, critical event, and reporting implementations must go through ingestion/storage/query boundaries such as `ITelemetryIngestor`, `ITelemetryStore`, and an equivalent query/report service. Do not add new persistence directly from controllers.
- **Session/evaluation backend module**: session creation, presets, and evaluation should live behind application services/stores when they grow beyond the current CRUD behavior; controllers should stay thin.
- **Frontend adapter**: Next.js pages should orchestrate UI and call contracts; they must not contain evaluation/reporting rules or deep scenario mapping.
- **Godot adapter/core**: Godot APIs (`Node`, `RigidBody3D`, `Input`, `OS`, `GD`) must stay in adapters/node scripts. Argument parsing, session client, telemetry creation, and event rules should be extracted to pure classes when feasible.
- **HAL**: when expanding controls beyond the keyboard MVP, input must go through an abstraction such as `IControlInputProvider`; do not add HID/serial/vendor SDK code outside a HAL adapter.
- **Shared contracts**: presets, launch arguments, telemetry, critical events, and reports must be explicit, versionable, and covered by producer/consumer tests.

---

## 9. In-Simulation Driving Experience (P2)

This section describes the visual/physical experience **inside Godot** while the Trainee is driving. It is not a separate user flow but a qualitative enhancement to the driving step (step 5 in the E2E flow).

### 9.1 Current state (P0/P1)

- Car is a single red box (MeshInstance3D with red material)
- Camera is fixed behind the car with basic follow
- No HUD (speed, steering, or controls display)
- Ground is a dark green plane with a few colored obstacle boxes
- Physics: basic RigidBody3D with forward force + steering torque

### 9.2 P2 milestone priority order

The P2 driving experience must be implemented in this exact order:

| Order | Milestone | Rationale |
|-------|-----------|-----------|
| 1 | **Cockpit interior** (camera inside car + steering wheel + dashboard) | User's explicit priority |
| 2 | **CanvasLayer HUD** (speed, steering, controls hint, finish button) | Needed for driver feedback |
| 3 | **Third-person drift camera** (toggleable with C key) | Secondary camera mode |
| 4 | **Improved physics feel** (drift, weight transfer, tunable exports) | Makes driving fun |
| 5 | **WorldEnvironment** (sky, fog, lighting) | Final visual polish |

### 9.3 Milestone details

**M1 — Cockpit interior (FIRST):**
- Camera positioned at driver's eye level: `~(0.6, 0.5, 0.3)` relative to car center
- Steering wheel mesh (TorusMesh or CylinderMesh) visible from cockpit view
- Steering wheel rotates with input: `Rotation.X = steeringAngle * 2`
- Dashboard mesh visible in front of driver (placeholder for future gauges)
- Driver seat mesh visible in peripheral view
- Single collision shape covers the car exterior (unchanged from P0)
- Third-person camera and toggle are NOT implemented yet (M3)

**M2 — HUD:**
- Speed label (top-right): "Speed: 45 km/h"
- Steering bar (bottom-center): shows current steering angle
- Controls hint (bottom): "WASD: drive | SPACE: brake | F: finish"
- Finish Simulation button (top-right): POSTs `/api/trainee/sessions/{id}/finish` then quits
- Session info (top-left): session ID + status

**M3 — Third-person drift camera:**
- Smooth follow with lerp (configurable: `DistanceBehind`, `HeightAbove`, `Smoothing`)
- Look-ahead: camera rotates toward velocity direction
- Drift lean: lateral offset when the car slides
- Toggle with `C` key between cockpit/third-person
- Spring arm collision detection (RayCast-based, basic)

**M4 — Physics feel (tunable exported properties):**
- Lift-off oversteer: release accelerator while turning → drift initiates
- PID-style regulation: angular velocity + drift angle + lateral forces
- Weight transfer pitch on accel/brake, roll on steering
- Speed-sensitive steering reduction
- Key exports: `EnginePower`, `SteeringTorque`, `GripFactor`, `OmegaMax`, `OmegaMaxDrift`

**M5 — Environment:**
- WorldEnvironment with ProceduralSkyMaterial (gradient sky)
- Fog for atmosphere
- More/improved obstacles
- Improved lighting

### 9.5 Architecture boundaries

| Concern | Boundary | Must NOT leak into |
|---------|----------|--------------------|
| Cockpit camera | Camera3D child of Car, fixed transform | BackendClient, telemetry contracts |
| Third-person camera math | Pure class or CameraPivot script | BackendClient, telemetry contracts |
| HUD | CanvasLayer + script reading VehicleController properties | BackendClient, telemetry pipeline |
| Physics tuning | Exported properties on VehicleController | Hardcoded magic numbers in code |
| Vehicle model | TSCN scene with MeshInstance3D children | Code logic |
| Telemetry | Unchanged (see `godot-telemetry-hal` skill) | Camera, HUD, physics feel |

### 9.6 Acceptance criteria (P2)

**M1 — Cockpit interior:**
- [x] Cockpit camera is the default view when the scene loads
- [x] Camera is at driver's eye level ~(0.6, 0.5, 0.3) from car center
- [x] Steering wheel mesh visible and rotates with steering input
- [x] Dashboard mesh visible in front of driver
- [x] All exterior meshes still present; telemetry/session unchanged

**M2 — HUD:**
- [ ] HUD shows speed (km/h), steering indicator, controls, and Finish button
- [ ] Finish button ends the session via API and closes Godot

**M3 — Third-person camera:**
- [ ] Camera follows smoothly with no jitter and drifts/leans on slides
- [ ] `C` key toggles between cockpit and third-person view

**M4 — Physics:**
- [ ] Physics feels more arcade-like; drift is achievable and recoverable
- [ ] All tunable values are exported properties

**M5 — Environment:**
- [ ] WorldEnvironment with sky and fog present

**Global:**
- [ ] All 120/120 existing tests still pass
- [ ] Existing telemetry, session lifecycle, and BackendClient unchanged

---

## 7. Error / Edge Case Flows

### 7.1 Invalid credentials
```
Login → 401 → Show "Invalid email or password" → Stay on /login
```

### 7.2 Session not in correct state
```
Start a non-Pending session → 400 → Show "Session is not in pending status"
Evaluate a non-Completed session → 400 → Evaluation form disabled
```

### 7.3 Email already registered
```
Register with existing email → 409 → Show "Email already registered"
Signup form shows error message below the form
```

### 7.4 Unauthorized access
```
Access /admin without Admin role → 403 → Backend rejects
Frontend proxy redirects to /login if no token
```

### 7.5 Network error / backend down
```
API call fails → Frontend shows error message
Godot: GD.PrintErr, keeps driving (telemetry lost)
```

---

## 8. Route Map (Frontend)

| Path | Role | Page |
|---|---|---|
| `/login` | Public | Login form |
| `/admin` | Admin | Dashboard (orgs + users count) |
| `/admin/organizations` | Admin | CRUD organizations |
| `/admin/users` | Admin | Invite users per org |
| `/instructor` | Instructor | Dashboard (sessions count + trainees) |
| `/instructor/sessions` | Instructor | List + create sessions |
| `/instructor/sessions/[id]` | Instructor | Telemetry + evaluation form |
| `/instructor/evaluations` | Instructor | Evaluation history |
| `/trainee` | Trainee | Dashboard (sessions + evaluations count) |
| `/trainee/sessions` | Trainee | List sessions (start/view) |
| `/trainee/sessions/[id]` | Trainee | Telemetry + instructor notes |
| `/trainee/evaluations` | Trainee | Evaluation history |
