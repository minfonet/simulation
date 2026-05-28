# User Flows — MVP

---

## 1. Authentication

### 1.1 Login

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

### 1.2 Logout

```
[User] → Clicks "Sign out" → localStorage.clear()
                              Redirect to /login
```

**Frontend**: `components/layout/sidebar.tsx` → `useAuth().logout()`

### 1.3 Token refresh (automatic)

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
          Enter scenario name
                  │
          Click "Create Session"
                  │
          POST /api/instructor/sessions
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
        (MVP: manual status change — Godot integration TBD)
```

**Frontend**: `app/trainee/sessions/page.tsx`  
**Backend**: `POST /api/trainee/sessions/{id}/start` → `TraineeController.StartSession()`  
**Persistence**: `SimulationSession.Status` → `Active`

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

### 4.4 View Session Detail (Telemetry + Evaluation)

```
[Trainee] → /trainee/sessions/{id}
               │
        GET /api/trainee/sessions/{id}
        GET /api/telemetry/session/{sessionId}
               │
        View: status, instructor, scenario, score
              telemetry table (last 50 points)
              instructor notes (if evaluated)
```

**Frontend**: `app/trainee/sessions/[id]/page.tsx`  
**Backend**: `GET /api/trainee/sessions/{id}`, `GET /api/telemetry/session/{sessionId}`

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
│    Godot sends telemetry every ~10 frames                          │
│    POST /api/telemetry (batch)                                     │
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

### 6.2 Current state vs target/gap

| Flow | Current state | MVP target | Minimum gap |
|---|---|---|---|
| Admin creates organization | Backend/API tests cover the main path. The E2E smoke test includes this path, but it has not yet been run against live Docker Compose services. The frontend exists. | Admin can create an organization and see UI confirmation. | Direct Admin component/page tests and E2E execution with live services are missing. |
| Evaluator defines evaluation | Instructor creates `SimulationSession` with `traineeId` and free-text/default `scenario`. | Instructor chooses an explicit, versionable base preset, then creates a pending session. | Preset contract/listing, UI selector, backend validation, and preset mapping to the base Godot scene/config are missing. |
| Evaluated launches Godot | Frontend marks the session active with `POST /start`; Godot is not launched from the UI. | Start button initiates a real handoff to Godot with `sessionId`, `apiUrl`, and authorization. | Launch/handoff mechanism, packaging/URI/protocol definition, and auth/token strategy for Godot are missing. |
| Test with no time limit | No time limit is documented/enforced; the session ends by action/API. | There is no product time limit for the MVP; finishing is an explicit user/simulation action. | Acceptance must ensure that no mandatory timeout is added accidentally. |
| Critical events | Current telemetry: speed, steering, position, and collision boolean. | Record minimal critical events, at least collision, with timestamp/severity/type for the report. | Critical event model/contract, derivation from telemetry or explicit emission, storage, and visualization are missing. |
| Final report | Instructor sees a telemetry table and grades manually. | Instructor sees a basic report with session summary, critical events, and relevant telemetry before grading. | Report endpoint/query, report UI, content criteria, and tests are missing. |
| Tests | Backend 50/50, frontend lib 23/23, Godot client 12/12; E2E smoke syntax validated. | Tests verify the real Admin → Instructor → Trainee → Godot/handoff → telemetry/events → report/evaluation flow. | Page tests, preset/event/report contracts, Godot auth, E2E execution with live services, and boundary coverage are missing. |

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
