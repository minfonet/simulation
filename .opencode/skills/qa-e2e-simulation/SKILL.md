---
name: qa-e2e-simulation
description: qa-e2e-simulation, Docker Compose, API smoke, seeding, Godot launch handoff, telemetry, events, reports, and full-flow triage; use when validating end-to-end simulation flows.
---

# QA E2E Simulation

## When to use

- Use for QA planning or execution of the full Admin → Instructor/Evaluator → Trainee/Evaluated → Godot → telemetry/events → report/evaluation flow.
- Use when running or updating `tests/e2e/smoke-test.ps1`, Docker Compose prerequisites, seeding, Godot launch checks, or cross-layer contract assertions.
- Use when triaging failures that may involve backend, frontend, Godot, auth, telemetry, or reports.

## When not to use

- Do not use for isolated unit-test-only changes unless they affect E2E coverage.
- Do not use to paper over architecture violations with broader smoke coverage; boundary defects still need implementation fixes.

## Read first

- `.memory/active-context.md`
- `docs/01-product/user-flows.md`
- `docs/01-product/flow-gap-plan.md`
- `docs/06-engineering/run-and-debug.md`
- `.memory/memories/architecture/mem_scalable_architecture_guardrails.md`
- `.memory/memories/bugs/mem_e2e_smoke_bootstrap_auth_header.md`
- `.memory/memories/decisions/mem_docker_compose_approach.md`
- `docs/06-engineering/language-policy.md`

## E2E prerequisites

- Backend API reachable, normally through Docker Compose at `localhost:8080`.
- PostgreSQL available and seeded as required by the smoke script.
- Frontend available when browser/UI verification is in scope.
- Godot executable/build available when launch or argument handoff verification is in scope.
- Test users and organizations should be seeded by the test or explicitly documented as prerequisites; avoid silent skips of core flows.

## Validation flow

1. Admin creates an organization and invites Instructor/Evaluator and Trainee/Evaluated users.
2. Instructor creates a Pending session using the base preset.
3. Trainee sees the session and starts it.
4. Launch/handoff passes verifiable `sessionId`, `apiUrl`, and authorization/token data to Godot or to a documented launch stub.
5. Godot or a controlled producer sends telemetry and at least required critical-event input.
6. Session finishes explicitly with no mandatory product timeout.
7. Instructor opens the basic report, sees summary/events/telemetry references, and saves evaluation.
8. Trainee sees score/comments.

## Mandatory guardrails

- QA must verify boundaries, not only happy paths: backend modules, Godot adapter/core separation, HAL, telemetry ingestion/storage/query, frontend adapter, and shared contracts.
- New telemetry/report tests must prove ingestion and query go through service/store/report boundaries, not controller-to-database logic.
- Godot tests should isolate engine APIs from pure parsing, telemetry, and HAL logic where feasible.
- Shared contract changes need producer/consumer assertions.
- UI checks do not replace backend auth and ownership tests.

## MVP scope limits and anti-overengineering

- Prefer a reliable smoke path plus focused integration/component tests over a brittle giant E2E suite.
- Do not require real hardware, vendor SDKs, or complex device calibration for MVP.
- A launch stub is acceptable only when the task explicitly scopes real Godot launch out and the gap is documented.
- Keep failure triage actionable: identify the failing boundary and the missing assertion.

## Acceptance/test checklist

- Smoke script syntax is validated before execution and fails fast on missing IDs, tokens, statuses, or telemetry counts.
- Docker/API health, seed data, and authentication are verified before flow assertions.
- Assertions cover role authorization, ownership, invalid state, invalid preset/token, telemetry/event ingestion, report retrieval, and evaluation save.
- Godot launch/handoff checks verify received args/config and controlled error handling.
- Failure report includes command run, environment assumptions, observed error, suspected boundary, and next diagnostic step.
