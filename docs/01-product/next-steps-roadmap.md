# Next Steps Roadmap — Post-P1

Last updated: 2026-05-28
Baseline: 101/101 tests passing (backend 66, frontend 23, Godot 12)

---

## Overview

P0 (scenario presets + Godot launch handoff) and P1 (critical events + basic final report) are complete. The following documents all remaining work, organized by priority and dependency.

---

## Prerequisites

### ✅ Restart opencode to load skills

**Why:** Five project skills exist at `.opencode/skills/` but opencode must be restarted to make them available. Without this, the skills listed below cannot be loaded.

**Action:** Restart opencode IDE/editor.

**Skills to load per task:**
- E2E smoke test → `qa-e2e-simulation`
- Frontend page tests → `nextjs-role-ui`
- Backend boundary refactors → `backend-telemetry-reporting`, `simulation-domain`
- Godot HAL extraction → `godot-telemetry-hal`
- Scenario expansion → `simulation-domain`

---

## Priority Matrix

| Priority | Task | Est. Effort | Depends On | Risk if deferred |
|----------|------|-------------|------------|------------------|
| P0 (quick) | Add scenario-presets integration test | 15 min | None | Coverage gap perpetuates |
| P0 (quick) | Fix 4 TS errors in `api.test.ts` | 15 min | None | False-positive error signals |
| P1 | Run E2E smoke test vs Docker Compose | 1 session | Docker setup | No full-flow validation |
| P1 | Frontend page component tests | 1-2 sessions | Restart opencode | No UI regression safety net |
| P2 | Backend boundary refactor (sessions/evaluations) | 2-3 sessions | P1 complete | Controller debt grows |
| P2 | Godot HAL extraction | 2-3 sessions | P1 complete | VehicleController becomes harder to test |
| P3 | Scenario preset expansion | 1 session | P2 boundaries | Acceptable for MVP |

---

## Quick Wins (P0)

### 1. Add integration test for `GET /api/instructor/scenario-presets`

**Objective:** Add a single integration test covering the scenario-presets endpoint.

**Files to touch:**
- `tests/SimApi.IntegrationTests/InstructorControllerTests.cs` (add test)

**Acceptance criteria:**
- One test verifying the endpoint returns 200 with at least one preset ("default")
- One test verifying preset shape: `id`, `name`, `description`, `godotScenePath`

**Architecture boundaries:**
- None required — `ScenarioPresetStore` is a singleton and the endpoint is read-only

**Skill to load:** `simulation-domain`

---

### 2. Fix pre-existing TypeScript errors in `api.test.ts`

**Objective:** Fix the 4 TypeScript compilation errors in `src/frontend/sim-web/lib/__tests__/api.test.ts`.

**Errors (4):**
1. Line 27: Unused `@ts-expect-error` directive
2. Line 29: Unused `@ts-expect-error` directive
3. Line 105: `fetch` mock type incompatibility (`url: string` vs `input: string | URL | Request`)
4. Line 144: Same `fetch` mock type incompatibility

**Files to touch:**
- `src/frontend/sim-web/lib/__tests__/api.test.ts`

**Acceptance criteria:**
- `npx tsc --noEmit` returns 0 errors
- All 23 frontend tests still pass

**Skill to load:** `nextjs-role-ui`

---

## P1 Gaps — Completing Test Coverage

### 3. Run E2E smoke test against live Docker Compose

**Objective:** Execute `tests/e2e/smoke-test.ps1` against the live Docker Compose stack to validate the full end-to-end flow.

**Prerequisite:** Docker Compose services running (`docker compose up` from `docker/`).

**Current script limitations (to fix/update):**
- Line 227: `scenario = "smoke-test"` uses invalid preset → change to `"default"`
- Script has no test for critical events (POST telemetry with `collision: true`, then GET `/api/telemetry/session/{id}/events`)
- Script has no test for report endpoint (GET `/api/instructor/sessions/{id}/report`)
- Script has no test for report endpoint (GET `/api/trainee/sessions/{id}/report`)

**Files to touch:**
- `tests/e2e/smoke-test.ps1` (fix scenario name, add event/report tests)

**Suggested new steps:**
- Step 9b: Send telemetry with collision → verify `criticalEvents` field
- Step 9c: GET `/api/telemetry/session/{id}/events` → verify events exist
- Step 12b: GET instructor report → verify report structure (averageSpeed, maxSpeed, collisionCount, criticalEvents)
- Step 12c: GET trainee report → verify report structure

**Acceptance criteria:**
- Script exits with code 0
- All steps pass (health → bootstrap → create → start → telemetry → events → finish → evaluate → report)
- Tests cover: happy path, critical events (collision), report data shape

**Skill to load:** `qa-e2e-simulation`

---

### 4. Frontend page component tests

**Objective:** Add Vitest + Testing Library tests for Admin, Instructor, and Trainee page components. Currently only `lib/` tests exist (23 tests).

**Files to create:**
- `src/frontend/sim-web/app/admin/__tests__/page.test.tsx`
- `src/frontend/sim-web/app/instructor/__tests__/page.test.tsx`
- `src/frontend/sim-web/app/instructor/sessions/__tests__/page.test.tsx`
- `src/frontend/sim-web/app/trainee/__tests__/page.test.tsx`
- `src/frontend/sim-web/app/trainee/sessions/__tests__/page.test.tsx`

**What to test per page:**

| Page | States to cover |
|------|----------------|
| Admin dashboard | Loading, org list, empty orgs, error |
| Admin orgs | Create org, validation, error |
| Admin users | Invite user, role selector, error |
| Instructor dashboard | Loading, sessions list, empty, error |
| Instructor sessions create | Preset dropdown, trainee selector, validation, submit |
| Instructor session detail | Loading, session data, report card (completed), telemetry table, evaluation form, empty report, error report |
| Instructor evaluations | Evaluations list, empty, error |
| Trainee dashboard | Loading, sessions list, empty, error |
| Trainee session detail | Loading, Pending state (start button), Active state (finish button), Completed state (report), launch handoff card, error states |

**Key patterns:**
- Mock `api.get<T>()` and `api.post<T>()` at the boundary
- Use `vi.mock()` for `@/lib/api`
- Test rendering outcomes (not internal state)
- Cover loading, empty, success, error states

**Acceptance criteria:**
- All page tests pass
- No regression on existing 23 lib tests
- Each page has at least 3 test cases (success, loading, error)

**Skill to load:** `nextjs-role-ui`

---

## P2.1 — Backend Boundary Refactor

### 5. Extract ISessionStore / ISessionService boundaries

**Objective:** Extract session CRUD and state management from `InstructorController` and `TraineeController` into service/store boundaries. Follows the same pattern as `ITelemetryIngestor`/`ITelemetryStore`.

**Current debt:**
- `InstructorController.CreateSession()` — validates preset, creates session, saves directly to `_db`
- `InstructorController.EvaluateSession()` — finds session, validates state, creates Evaluation, saves
- `TraineeController.StartSession()` — validates Pending → Active, saves
- `TraineeController.FinishSession()` — validates Active → Completed, saves

**Files to create:**
- `src/backend/SimApi/Services/ISessionStore.cs`
- `src/backend/SimApi/Services/SessionStore.cs`
- `src/backend/SimApi/Services/ISessionService.cs` (or per-use-case services)
- `src/backend/SimApi/Services/SessionService.cs`
- `src/backend/SimApi/Services/IEvaluationStore.cs`
- `src/backend/SimApi/Services/EvaluationStore.cs`

**Files to modify:**
- `src/backend/SimApi/Controllers/InstructorController.cs` — delegate to services
- `src/backend/SimApi/Controllers/TraineeController.cs` — delegate to services
- `src/backend/SimApi/Program.cs` — register new services
- `tests/SimApi.IntegrationTests/InstructorControllerTests.cs` — update if behavior changes
- `tests/SimApi.IntegrationTests/TraineeControllerTests.cs` — update if behavior changes

**Acceptance criteria:**
- `InstructorController.CreateSession()` delegates to `ISessionService` — no direct `_db` access
- `InstructorController.EvaluateSession()` delegates to `IEvaluationService` — no direct `_db` access
- `TraineeController.StartSession()` and `FinishSession()` delegate to session service — no direct `_db` access
- All 66 existing backend tests still pass
- Controllers remain thin (auth/context extraction, validation, delegation, response mapping)

**Architecture guardrails:**
- ✅ Use service/store boundaries (same pattern as ITelemetryIngestor/ITelemetryStore)
- ❌ Do NOT add new business logic to controllers
- ✅ Keep ownership checks in controllers or delegate to services that validate ownership

**Skill to load:** `simulation-domain`

---

### 6. Extract IUserStore / IOrganizationStore (optional)

**Objective:** Apply the same boundary extraction to AdminController for org/user management.

**Current debt:**
- `AdminController` creates orgs, invites users, lists users — all via direct `_db` access
- `AuthController` registers users via direct `_db` access

**Files to create:**
- `src/backend/SimApi/Services/IUserStore.cs`
- `src/backend/SimApi/Services/UserStore.cs`
- `src/backend/SimApi/Services/IOrganizationStore.cs`
- `src/backend/SimApi/Services/OrganizationStore.cs`

**Files to modify:**
- `src/backend/SimApi/Controllers/AdminController.cs`
- `src/backend/SimApi/Controllers/AuthController.cs`
- `src/backend/SimApi/Program.cs`

**Note:** This is lower priority than session/evaluation refactors. Auth and org CRUD is simpler and less likely to change.

**Skill to load:** `simulation-domain`

---

## P2.2 — Godot HAL + Adapter Extraction

### 7. Extract IControlInputProvider (HAL)

**Objective:** Create a hardware abstraction layer for control input so that VehicleController does not read Godot `Input` directly. This enables:
- Unit-testable input logic
- Future gamepad/steering wheel support
- Input recording/replay

**Current debt:**
`VehicleController._PhysicsProcess()` reads `Input.IsActionPressed("move_forward")`, `move_backward`, `move_left`, `move_right`, `brake` directly.

**Files to create:**
- `simulation/driving-sim/Scripts/HAL/IControlInputProvider.cs`
- `simulation/driving-sim/Scripts/HAL/ControlState.cs` (normalized state: throttle, brake, steering, discrete actions)
- `simulation/driving-sim/Scripts/HAL/KeyboardInputProvider.cs` (Godot `Input` wrapper)

**Files to modify:**
- `simulation/driving-sim/Scripts/VehicleController.cs` — use `IControlInputProvider` instead of `Input.IsActionPressed()`
- `tests/GodotSim.Tests/` — add HAL tests

**Acceptance criteria:**
- `VehicleController` no longer calls `Input.IsActionPressed()` directly
- `KeyboardInputProvider` implements `IControlInputProvider` and wraps Godot Input
- `ControlState` has normalized fields: `Throttle` (0..1), `Brake` (0..1), `Steering` (-1..+1), `Actions` (discrete flags)
- Existing 12 Godot tests still pass
- New unit tests verify `KeyboardInputProvider` maps inputs correctly (can be tested without Godot engine if pure logic is extracted)

**Architecture guardrails:**
- ✅ All control input goes through `IControlInputProvider`
- ✅ Godot-specific APIs stay inside adapter (`KeyboardInputProvider`)
- ❌ `VehicleController` must NOT read Godot `Input` directly

**Skill to load:** `godot-telemetry-hal`

---

### 8. Extract TelemetryCollector and ISessionClient from VehicleController

**Objective:** Split `VehicleController` responsibilities into focused collaborators:
- `TelemetryCollector` — collects telemetry points, manages batching
- `ISessionClient` / `BackendSessionClient` — handles backend HTTP communication
- `VehicleController` — orchestrates physics + collaborators (thin adapter)

**Current debt:**
`VehicleController` has 5 responsibilities: physics, input, telemetry collection, backend communication, and scene lifecycle.

**Files to create:**
- `simulation/driving-sim/Scripts/Telemetry/TelemetryPoint.cs` (shared struct)
- `simulation/driving-sim/Scripts/Telemetry/TelemetryCollector.cs`
- `simulation/driving-sim/Scripts/Backend/ISessionClient.cs`
- `simulation/driving-sim/Scripts/Backend/SessionClient.cs` (extracted from BackendClient)

**Files to modify:**
- `simulation/driving-sim/Scripts/VehicleController.cs` — become thin adapter
- `simulation/driving-sim/Scripts/BackendClient.cs` — split into ISessionClient
- `tests/GodotSim.Tests/` — update tests for new boundaries

**Acceptance criteria:**
- `TelemetryCollector` can be unit-tested without Godot engine
- `ISessionClient` can be mocked in tests
- `VehicleController` only orchestrates: creates collaborators, connects signals, calls `_PhysicsProcess`
- Existing 12 Godot tests still pass (updated for new structure)
- New tests for `TelemetryCollector` batching logic

**Architecture guardrails:**
- ✅ Godot engine APIs stay inside adapter code
- ✅ `VehicleController` does not accumulate more responsibilities
- ✅ Telemetry collection tested outside engine

**Skill to load:** `godot-telemetry-hal`

---

## P3 — Future / Post-MVP

### 9. Scenario preset expansion

**Objective:** Add more scenario presets (e.g., "night-driving", "highway", "offroad") with Godot scene/config mappings.

**Files to modify:**
- `src/backend/SimApi/Services/ScenarioPresetStore.cs` — add more presets
- `simulation/driving-sim/Scenes/` — new Godot scenes per preset

**Acceptance criteria:**
- Instructor can select from multiple presets
- Backend validates all presets
- Each preset maps to a valid Godot scene path
- Integration tests cover all presets

---

### 10. Collision deduplication

**Objective:** Group consecutive collision-true telemetry frames into a single CriticalEvent instead of one per frame.

**Files to modify:**
- `src/backend/SimApi/Services/TelemetryIngestor.cs` — deduplication logic

**Acceptance criteria:**
- 10 consecutive collision frames → 1 CriticalEvent
- Non-consecutive collisions → multiple CriticalEvents

---

## Summary of Files to Create/Modify

### Quick Wins

| File | Action | Task |
|------|--------|------|
| `tests/SimApi.IntegrationTests/InstructorControllerTests.cs` | Modify | Add scenario-presets test |
| `src/frontend/sim-web/lib/__tests__/api.test.ts` | Modify | Fix TS errors |

### P1 Test Coverage

| File | Action | Task |
|------|--------|------|
| `tests/e2e/smoke-test.ps1` | Modify | Fix scenario name + add events/report steps |
| `src/frontend/sim-web/app/*/__tests__/*.test.tsx` | Create (5-6 files) | Page component tests |

### P2 Backend Boundaries

| File | Action | Task |
|------|--------|------|
| `src/backend/SimApi/Services/ISessionStore.cs` | Create | Session storage boundary |
| `src/backend/SimApi/Services/SessionStore.cs` | Create | Session store implementation |
| `src/backend/SimApi/Services/ISessionService.cs` | Create | Session use-case service |
| `src/backend/SimApi/Services/SessionService.cs` | Create | Session service implementation |
| `src/backend/SimApi/Services/IEvaluationStore.cs` | Create | Evaluation storage boundary |
| `src/backend/SimApi/Services/EvaluationStore.cs` | Create | Evaluation store implementation |
| `src/backend/SimApi/Services/IEvaluationService.cs` | Create | Evaluation use-case service |
| `src/backend/SimApi/Services/EvaluationService.cs` | Create | Evaluation service implementation |
| `src/backend/SimApi/Controllers/InstructorController.cs` | Modify | Delegate to services |
| `src/backend/SimApi/Controllers/TraineeController.cs` | Modify | Delegate to services |
| `src/backend/SimApi/Program.cs` | Modify | Register new services |

### P2 Godot HAL

| File | Action | Task |
|------|--------|------|
| `simulation/driving-sim/Scripts/HAL/IControlInputProvider.cs` | Create | Input abstraction |
| `simulation/driving-sim/Scripts/HAL/ControlState.cs` | Create | Normalized control state |
| `simulation/driving-sim/Scripts/HAL/KeyboardInputProvider.cs` | Create | Godot Input adapter |
| `simulation/driving-sim/Scripts/Telemetry/TelemetryCollector.cs` | Create | Telemetry collection logic |
| `simulation/driving-sim/Scripts/Backend/ISessionClient.cs` | Create | Backend communication abstraction |
| `simulation/driving-sim/Scripts/Backend/SessionClient.cs` | Create | Backend HTTP client |
| `simulation/driving-sim/Scripts/VehicleController.cs` | Modify | Thin adapter using collaborators |
| `simulation/driving-sim/Scripts/BackendClient.cs` | Modify/Deprecate | Split into SessionClient |
| `tests/GodotSim.Tests/` | Modify | Update + add tests |

---

## Test Count Projections

| Phase | Backend | Frontend | Godot | E2E | Total |
|-------|---------|----------|-------|-----|-------|
| Current (P1) | 66 | 23 | 12 | 0 (script) | 101 |
| + Quick wins | 67 | 23 | 12 | 0 | 102 |
| + E2E smoke | 67 | 23 | 12 | 10+ | 112+ |
| + Frontend tests | 67 | 35+ | 12 | 10+ | 124+ |
| + P2 Backend | 75+ | 35+ | 12 | 10+ | 132+ |
| + P2 Godot | 75+ | 35+ | 25+ | 10+ | 145+ |

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| E2E script finds real bug in Docker Compose env | Medium | High | Run early, fix bugs before further development |
| Frontend page tests are brittle due to DOM structure | Medium | Low | Test behavior (rendered text, presence of elements), not structure |
| Backend boundary refactor breaks existing tests | Low | High | Do incrementally, ensure 66 tests pass after each change |
| Godot HAL extraction requires engine for tests | Medium | Medium | Extract pure C# logic fully outside Godot; test without engine |
| Control input mapping changes driving feel | Low | Medium | Keep normalization logic identical to current behavior; verify manually |
