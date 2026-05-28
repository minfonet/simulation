---
id: mem_coverage_scenario_presets_endpoint
type: learning
tags:
  - coverage
  - backend
  - integration-test
  - scenario-presets
---

# Coverage Blind Spot: `/api/instructor/scenario-presets` Endpoint

## What is missing

The `GET /api/instructor/scenario-presets` endpoint was added in `InstructorController.cs` as part of the P0 milestone (preset dropdown for instructor sessions page). No integration test covers this endpoint.

## Why it matters

- The endpoint returns `List<ScenarioPresetResponse>` with `Id`, `Name`, `Description`, `GodotScenePath`.
- It requires `Instructor` role via `[Authorize(Roles = "Instructor")]`.
- Currently only the `"default"` preset is registered in `ScenarioPresetStore`.
- Missing tests mean regressions in auth enforcement or response shape won't be caught.

## What should be tested

1. **Happy path**: Authenticated instructor gets 200 with at least the `"default"` preset.
2. **Auth enforcement**: Non-instructor (trainee, admin, unauthenticated) gets 401/403.
3. **Response shape**: Returned items have `id`, `name`, `description`, `godotScenePath` properties matching `ScenarioPresetResponse`.

## Detection context

Found during P0 milestone QA verification (2026-05-28). Backend integration tests pass 50/50 but none cover this new endpoint. The other 7 test files (Auth, Admin, Instructor, Trainee, Telemetry, Security, Validation) were also reviewed — none reference "scenario-presets" or "ScenarioPreset".

## Action

Add integration test coverage in `InstructorControllerTests.cs` before closing P0, or document as deferred gap for P1.
