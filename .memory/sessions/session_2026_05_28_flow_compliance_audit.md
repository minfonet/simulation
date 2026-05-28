# Session: Flow compliance audit

**Date:** 2026-05-28

## What changed

- Audited compliance with the requested flows: Admin creates organization, Evaluator/Instructor defines session/scene, and Evaluated/Trainee starts Godot simulation and generates data for evaluation.
- Reviewed active context, product/flow/UI/telemetry documents, and architecture guardrails.
- Inspected backend, frontend, Godot, integration tests, Godot tests, and E2E smoke test.
- Updated `.memory/active-context.md` with findings, risks, and open tasks derived from the analysis.

## Key files reviewed

- `docs/01-product/mdvp.md`
- `docs/01-product/user-flows.md`
- `docs/05-ui-ux/role-based-ui.md`
- `docs/02-architecture/telemetry-system.md`
- `.memory/memories/architecture/mem_scalable_architecture_guardrails.md`
- `src/backend/SimApi/Controllers/AdminController.cs`
- `src/backend/SimApi/Controllers/InstructorController.cs`
- `src/backend/SimApi/Controllers/TraineeController.cs`
- `src/backend/SimApi/Controllers/TelemetryController.cs`
- `src/backend/SimApi/DTOs/SessionDtos.cs`
- `src/backend/SimApi/DTOs/TelemetryDtos.cs`
- `src/frontend/sim-web/app/admin/organizations/page.tsx`
- `src/frontend/sim-web/app/instructor/sessions/page.tsx`
- `src/frontend/sim-web/app/instructor/sessions/[id]/page.tsx`
- `src/frontend/sim-web/app/trainee/sessions/page.tsx`
- `src/frontend/sim-web/app/trainee/sessions/[id]/page.tsx`
- `simulation/driving-sim/Scripts/VehicleController.cs`
- `simulation/driving-sim/Scripts/BackendClient.cs`
- `simulation/driving-sim/Scenes/Main.tscn`
- `tests/e2e/smoke-test.ps1`

## Files changed

- `.memory/active-context.md`
- `.memory/sessions/session_2026_05_28_flow_compliance_audit.md`

## Bugs or gaps found

- The Admin flow is mostly covered: organization creation exists in backend/frontend and API/E2E tests.
- The Evaluator/Instructor flow is basic: it creates sessions with free-text/default `scenario`, but there is no versioned scenario preset, real selector, scenario endpoint, or mapping to Godot.
- The Evaluated/Trainee flow does not launch Godot from frontend: it only calls `/api/trainee/sessions/{id}/start`.
- Godot expects `--session-id`, `--token`, and `--api-url` arguments, but there is no handoff from frontend.
- `BackendClient.StartSession()` and `FinishSession()` do not attach an Authorization header, while trainee endpoints require the `Trainee` role.
- Telemetry captures speed, steering, position, and collision; there is no critical event model or aggregated final report.
- Tests cover API/lib/basic JSON contract, but not Admin/Instructor/Trainee pages, real Godot launch, or critical report.

## Decisions made

- The flows are considered to exist as a partial MVP vertical slice, not as a complete operating flow.
- The next implementation should prioritize functional minimums: explicit base preset, Godot launch handoff, critical events/basic report.
- Any telemetry/reporting expansion must comply with guardrails: `ITelemetryIngestor`/`ITelemetryStore` or equivalent, explicit contracts, and producer/consumer tests.

## Next steps

1. Implement basic scenario preset contract/listing (`default/base`) and selector in the Instructor UI.
2. Implement an MVP Godot launch mechanism from Trainee with `sessionId`, API URL, and clear token/authorization.
3. Fix authorization for `BackendClient.StartSession()`/`FinishSession()` or define that frontend controls state and Godot only sends telemetry.
4. Add a basic report model/endpoint/UI with critical events derived from telemetry, for example collisions, speeding if applicable, or leaving the zone if one exists.
5. Add page tests, telemetry/event contract tests, and E2E smoke test with live services.
