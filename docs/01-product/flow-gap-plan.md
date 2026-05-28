# Flow Gap Plan — Admin → Evaluator → Trainee → Godot → Report

## Purpose

Document the minimum pending work needed to turn the current vertical slice into the requested operating flow while keeping MVP scope: one functional base preset, basic handoff to Godot, a test with no time limit, minimal critical events, and a basic report for manual grading.

## Current baseline

- Admin can create organizations and invite users.
- Instructor can create sessions with free-text/default `scenario` and manually evaluate completed sessions.
- Trainee can start/finish sessions through API/UI and view results.
- Godot has a base scene and HTTP client, but there is no real launch from the frontend.
- Telemetry persists speed, steering, position, and collision; there are no critical events or aggregated report.
- Existing tests cover backend API, frontend libs, the Godot client, and E2E smoke syntax; pages, real handoff, and new contracts are missing.

## Prioritized MVP gaps

| Priority | Gap | Minimum deliverable | Acceptance criteria | Boundaries/tests |
|---|---|---|---|---|
| P0 | Base scenario preset | Explicit contract with at least `default`/`base`, endpoint/listing or validated backend source, Instructor UI selector, mapping to Godot base scene/config. | Instructor creates a session by selecting a valid preset; backend rejects unknown presets; session keeps a stable reference; UI does not depend on free text for the main path. | Backend session/preset service or store when adding non-trivial logic; frontend page thin; contract tests backend/frontend and Godot compatibility if consumed by simulator. |
| P0 | Godot handoff/launch | Start from Trainee produces launch/handoff with `sessionId`, `apiUrl`, and defined authorization. | Clicking Start activates or prepares the session, runs the documented launch mechanism, and Godot receives verifiable arguments/config; errors do not leave ambiguous state. | Frontend adapter handles launch only; session use case behind backend boundary; Godot parses args in adapter/pure config parser tested without engine. |
| P0 | Auth/token for Godot | Secure MVP strategy for Godot to call authorized APIs without hardcoded credentials. | Godot can start/send telemetry/finish only for an authorized session; invalid/expired token fails with a controlled error; ownership is validated in backend. | Auth/session boundary; producer/consumer tests for launch auth contract; backend integration tests for telemetry/session ownership. |
| P1 | Critical events | Minimum critical event model/contract: `sessionId`, timestamp, type, severity, optional metadata; at least collision. | Collisions record a queryable critical event; duplicates/derivation are defined; events belong to the correct session. | Telemetry ingestion/storage/query boundaries (`ITelemetryIngestor`/`ITelemetryStore` or equivalents); no new controller-to-DB telemetry logic; contract tests. |
| P1 | Basic final report | Query/endpoint/UI for completed-session report with summary, critical events, and relevant telemetry. | Evaluator sees report before grading; report respects org/role ownership; evaluation saves score/comments; Trainee sees final result. | Reporting query service over telemetry/session/evaluation stores; controllers/pages thin; integration and UI tests. |
| P1 | Missing flow tests | Admin/Instructor/Trainee page tests, preset/launch/events/report contracts, and E2E smoke with live services. | CI/local reports passing tests or documents missing runtime; E2E covers create org → create preset session → launch/handoff stub or real → telemetry/events → report/evaluate. | QA must protect boundaries, not only happy path: auth, ownership, invalid state, invalid preset/token. |
| P2 | Incremental refactor of current boundaries | Extract services/stores when touching sessions, evaluation, and telemetry. | New capabilities do not increase logic in controllers/pages/Godot node scripts; existing debt does not get worse and follow-ups are documented. | Architecture guardrails; reviewer rejects new direct persistence/business logic in controllers for telemetry/reporting. |

## Flow-specific acceptance checklist

### 1. Admin creates organization

- [ ] Authenticated Admin creates an organization and receives `201`.
- [ ] Organization appears in the list after data refresh.
- [ ] Non-Admin user cannot create an organization.
- [ ] Backend tests cover success/auth; frontend tests cover UI success/error.

### 2. Evaluator/Instructor defines evaluation using base preset

- [ ] UI shows at least one functional base preset.
- [ ] Backend validates the preset and creates a `Pending` session for a trainee in the allowed organization.
- [ ] Preset has documented mapping to the MVP Godot scene/config.
- [ ] Multiple complex scenarios or a scenario editor are not implemented in the MVP.
- [ ] Tests verify valid preset, invalid preset, and contract compatibility.

### 3. Evaluated/Trainee launches Godot and runs untimed test

- [ ] Trainee sees assigned sessions and can start only their own `Pending` sessions.
- [ ] Handoff includes `sessionId`, `apiUrl`, and token/authorization strategy.
- [ ] Godot does not depend on embedded credentials and does not expose tokens unnecessarily.
- [ ] There is no mandatory time limit; finish is explicit.
- [ ] Tests cover invalid state, invalid auth, and Godot argument parsing/receipt.

### 4. Critical events and final report

- [ ] Telemetry/critical events enter through an ingestion boundary, are stored through a store, and are queried through a query/report boundary.
- [ ] At least collision generates a visible critical event with timestamp and basic severity.
- [ ] Basic report shows summary and events for a completed session.
- [ ] Evaluator grades from the report or detail screen, saving score/comments.
- [ ] Tests cover ownership, events from another user's session, incomplete-session report, and response contract.

## Architecture constraints for all pending work

- Telemetry/reporting must use explicit ingestion/storage/query contracts such as `ITelemetryIngestor`, `ITelemetryStore`, and a report query service equivalent.
- Session/evaluation changes should introduce application services/stores before adding non-trivial behavior to controllers.
- Godot engine APIs must stay inside adapter/node code; pure parsing, telemetry mapping, event derivation and retry/buffering logic should be tested outside the engine when possible.
- Control expansion beyond keyboard MVP must go through HAL (`IControlInputProvider`-style normalized controls) before reaching simulation logic.
- Cross-layer contracts for presets, launch handoff, telemetry, critical events and reports must be explicit, versionable and covered by producer/consumer tests.
