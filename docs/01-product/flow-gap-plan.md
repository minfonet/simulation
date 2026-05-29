# Flow Gap Plan — Admin → Evaluator → Trainee → Godot → Report

## Purpose

Document the minimum pending work needed to turn the current vertical slice into the requested operating flow while keeping MVP scope: one functional base preset, basic handoff to Godot, a test with no time limit, minimal critical events, and a basic report for manual grading.

## Current baseline (post-P0/P1)

- Admin can create organizations and invite users. ✅
- Instructor can create sessions with validated base preset (`default`) and manually evaluate completed sessions. ✅
- Trainee can start/finish sessions through API/UI and view results. Launch handoff card includes CLI command and download script; Godot receives `--session-id`, `--api-url`, `--token`. ✅
- Godot has a base scene and HTTP client; `BackendClient.ReadSessionId()` parses launch arguments; `BackendClient.StartSession()` activates session from Godot. ✅
- Telemetry persists speed, steering, position, and collision; critical events auto-derived from `collision=true`; aggregated report available. ✅ (P1)
- Existing tests: backend **66/66**, frontend lib **23/23**, Godot client **12/12**. P0/P1 boundary tests for telemetry ingestor/store, events, reports. ✅

### Remaining baseline gaps (post-P1)

- E2E smoke script has known issue: line 227 uses `scenario = "smoke-test"` which should be `"default"`. ⚠️
- Critical event deduplication not implemented (post-MVP scope).
- No page component tests for frontend.
- E2E smoke script not yet executed against live Docker Compose services.

## Prioritized MVP gaps

| Priority | Gap | Status | Minimum deliverable | Acceptance criteria | Boundaries/tests |
|---|---|---|---|---|---|---|
| P0 | Base scenario preset | ✅ DELIVERED | Explicit contract with at least `default`/`base`, endpoint/listing or validated backend source, Instructor UI selector, mapping to Godot base scene/config. | Instructor creates a session by selecting a valid preset; backend rejects unknown presets; session keeps a stable reference; UI does not depend on free text for the main path. | Backend session/preset service or store when adding non-trivial logic; frontend page thin; contract tests backend/frontend and Godot compatibility if consumed by simulator. |
| P0 | Godot handoff/launch | ✅ DELIVERED | Start from Trainee produces launch/handoff with `sessionId`, `apiUrl`, and defined authorization. | Clicking Start activates or prepares the session, runs the documented launch mechanism, and Godot receives verifiable arguments/config; errors do not leave ambiguous state. | Frontend adapter handles launch only; session use case behind backend boundary; Godot parses args in adapter/pure config parser tested without engine. |
| P0 | Auth/token for Godot | ✅ DELIVERED | Secure MVP strategy for Godot to call authorized APIs without hardcoded credentials. | Godot can start/send telemetry/finish only for an authorized session; invalid/expired token fails with a controlled error; ownership is validated in backend. | Auth/session boundary; producer/consumer tests for launch auth contract; backend integration tests for telemetry/session ownership. |
| P1 | Critical events | ✅ DELIVERED | Minimum critical event model/contract: `sessionId`, timestamp, type, severity, optional metadata; at least collision. | Collisions record a queryable critical event; duplicates/derivation are defined; events belong to the correct session. | Telemetry ingestion/storage/query boundaries (`ITelemetryIngestor`/`ITelemetryStore` or equivalents); no new controller-to-DB telemetry logic; contract tests. |
| P1 | Basic final report | ✅ DELIVERED | Query/endpoint/UI for completed-session report with summary, critical events, and relevant telemetry. | Evaluator sees report before grading; report respects org/role ownership; evaluation saves score/comments; Trainee sees final result. | Reporting query service over telemetry/session/evaluation stores; controllers/pages thin; integration and UI tests. |
| P1 | Missing flow tests | ⚠️ PARTIAL | Admin/Instructor/Trainee page tests, preset/launch/events/report contracts, and E2E smoke with live services. | CI/local reports passing tests or documents missing runtime; E2E covers create org → create preset session → launch/handoff stub or real → telemetry/events → report/evaluate. | QA must protect boundaries, not only happy path: auth, ownership, invalid state, invalid preset/token. |
| P2 | Driving Experience (M1 cockpit interior, M2 HUD, M3 drift camera, M4 physics, M5 environment) | ✅ **ALL 5 COMPLETE** | **M1**: Cockpit interior with driver's-eye camera, steering wheel mesh, dashboard, driver seat. **M2**: CanvasLayer HUD (speed, steering, controls, finish button). **M3**: Third-person drift camera with smooth follow, look-ahead, lean, toggleable with C key. **M4**: Improved arcade-style physics (lift-off oversteer, PID regulation, weight transfer, tunable exports). **M5**: WorldEnvironment with sky and fog. | M1: Cockpit camera is default POV; steering wheel visible and rotates; dashboard visible. M2: HUD shows speed, steering, controls, finish button. M3: Camera follows smoothly and drifts/leans; C toggles view. M4: Physics feels arcade-like; drift achievable; all tunables exported. M5: Sky + fog present. Global: 120/120 tests pass; telemetry/session unchanged. | Godot engine APIs stay in adapter/node scripts; camera/HUD logic extracted from BackendClient and VehicleController; no telemetry contract changes; no hardware/input changes; use `godot-driving-experience` skill. Cockpit is THE priority: implement before third-person camera. |
| P2 | Incremental refactor of current boundaries | ❌ NOT STARTED | Extract services/stores when touching sessions, evaluation, and telemetry. | New capabilities do not increase logic in controllers/pages/Godot node scripts; existing debt does not get worse and follow-ups are documented. | Architecture guardrails; reviewer rejects new direct persistence/business logic in controllers for telemetry/reporting. |

### Legend
- ✅ DELIVERED = implementation complete, tests passing
- ⚠️ PARTIAL = some tests done but gaps remain (page tests, E2E execution)
- ❌ NOT STARTED = not yet addressed

## Flow-specific acceptance checklist

### 1. Admin creates organization

- [x] Authenticated Admin creates an organization and receives `201`.
- [x] Organization appears in the list after data refresh.
- [x] Non-Admin user cannot create an organization.
- [x] Backend tests cover success/auth.
- [ ] Frontend page component tests cover UI success/error. ⚠️ PENDING

### 2. Evaluator/Instructor defines evaluation using base preset

- [x] UI shows at least one functional base preset (dropdown with "Default").
- [x] Backend validates the preset and creates a `Pending` session for a trainee in the allowed organization.
- [x] Preset has documented mapping to the MVP Godot scene/config (`res://Scenes/Main.tscn`).
- [x] Multiple complex scenarios or a scenario editor are not implemented in the MVP.
- [x] Tests verify valid preset (201) and invalid preset (400).
- [ ] Integration test for `GET /api/instructor/scenario-presets` endpoint. ⚠️ PENDING

### 3. Evaluated/Trainee launches Godot and runs untimed test

- [x] Trainee sees assigned sessions and can start only their own `Pending` sessions.
- [x] Handoff includes `sessionId`, `apiUrl`, and token/authorization strategy.
- [x] Godot does not depend on embedded credentials and does not expose tokens unnecessarily.
- [x] There is no mandatory time limit; finish is explicit.
- [x] Tests cover invalid state, invalid auth, and Godot argument parsing/receipt (12/12).

### 4. Critical events and final report

- [x] Telemetry/critical events enter through an ingestion boundary (`ITelemetryIngestor`), stored through a store (`ITelemetryStore`), and queried through a query/report boundary.
- [x] At least collision generates a visible critical event with timestamp and basic severity.
- [x] Basic report shows summary and events for a completed session.
- [x] Evaluator grades from the report or detail screen, saving score/comments.
- [x] Tests cover ownership, events from another user's session, incomplete-session report, and response contract (7 events + 9 report = 16 tests).

## Remaining gaps (post-P1)

These are the open items required to execute all 5 flows end-to-end:

| # | Item | Flow affected | Type | Effort | Status |
|---|------|--------------|------|--------|--------|
| 1 | Fix smoke-test.ps1 line 227: `"smoke-test"` → `"default"` | All (E2E validation) | Bug | 1 min | ✅ Done |
| 2 | Extend smoke-test.ps1: add critical events + report validation | 4, 5 | Test gap | 30 min | ✅ Done (6 new steps) |
| 3 | Run smoke test against live Docker Compose | All | Validation | 30 min | ✅ Done (19/19 PASS) |
| 4 | Integration test for `GET /api/instructor/scenario-presets` | 2 | Test gap | 15 min | ⬜ Pending |
| 5 | Frontend page component tests (~6 files) | 1–5 | Test gap | 1-2 sessions | ⬜ Pending |
| 6 | Fix 4 pre-existing TypeScript compilation errors in `api.test.ts` | All | Build | 15 min | ⬜ Pending |
| 7 | **Cockpit interior** (driver's-eye camera, steering wheel, dashboard, seat) | 6 (Driving Experience) | Visual | 1 session | ✅ Complete |
| 8 | **CanvasLayer HUD** (speed, steering, controls, finish button) | 6 | HUD | 1 session | ✅ Complete |
| 9 | **Third-person drift camera** (smooth follow, look-ahead, lean, C toggle) | 6 | Camera | 1 session | ✅ Complete |
| 10 | **Improved physics feel** (lift-off oversteer, PID regulation, tunable exports) | 6 | Physics | 1 session | ✅ Complete |
| 11 | **WorldEnvironment** (sky, fog, improved lighting) | 6 | Environment | 30 min | ✅ Complete |

### Priority for next milestone

1. Cockpit interior (item 7) — ✅ **DONE**
2. CanvasLayer HUD (item 8) — ✅ **DONE**
3. Third-person drift camera (item 9) — ✅ **DONE**
4. Improved physics feel (item 10) — ✅ **DONE**
5. WorldEnvironment (item 11) — ✅ **DONE**
6. Integration test for scenario-presets endpoint (item 4)
7. Frontend page component tests (item 5)
8. Fix TypeScript errors in api.test.ts (item 6)

## Architecture constraints for all pending work

- Telemetry/reporting must use explicit ingestion/storage/query contracts such as `ITelemetryIngestor`, `ITelemetryStore`, and a report query service equivalent.
- Session/evaluation changes should introduce application services/stores before adding non-trivial behavior to controllers.
- Godot engine APIs must stay inside adapter/node code; pure parsing, telemetry mapping, event derivation and retry/buffering logic should be tested outside the engine when possible.
- Control expansion beyond keyboard MVP must go through HAL (`IControlInputProvider`-style normalized controls) before reaching simulation logic.
- Cross-layer contracts for presets, launch handoff, telemetry, critical events and reports must be explicit, versionable and covered by producer/consumer tests.

---

> Last updated: 2026-05-29 | Milestones: P0 ✅ | P1 ✅ | P2 ✅ **ALL 5 COMPLETE**
