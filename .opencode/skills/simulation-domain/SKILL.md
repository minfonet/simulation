---
name: simulation-domain
description: simulation-domain, MVP flows, roles, session states, base preset, manual evaluation, and report scope; use when defining or changing simulation business flows or acceptance criteria.
---

# Simulation Domain

## When to use

- Use for Admin, Evaluator/Instructor, Evaluated/Trainee, session, preset, evaluation, and basic report flow changes.
- Use when translating product flow gaps into implementation or QA acceptance criteria.
- Use when deciding MVP scope for base presets, untimed tests, critical events, and manual grading.

## When not to use

- Do not use for low-level Godot input, telemetry transport, backend storage, or UI-only styling unless flow rules are changing.
- Do not use to justify broad scenario editors, advanced analytics, hardware support, or real-time dashboards in the MVP.

## Read first

- `.memory/active-context.md`
- `docs/01-product/user-flows.md`
- `docs/01-product/flow-gap-plan.md`
- `docs/01-product/mdvp.md`
- `.memory/memories/architecture/mem_scalable_architecture_guardrails.md`
- `docs/06-engineering/language-policy.md`

## Domain baseline

- Roles: Admin manages organizations and users; Evaluator/Instructor creates sessions and grades; Evaluated/Trainee runs sessions and views results.
- Session states: Pending before launch, Active during the test, Completed after explicit finish, Evaluated after score/comments are saved if represented separately.
- Base preset: MVP needs one explicit functional preset such as `default`/`base`, with a stable contract and mapping to the existing Godot base scene/config.
- Evaluation: MVP grading is manual by Evaluator/Instructor after reviewing a basic report; do not add advanced analytics or automated scoring unless explicitly requested.
- Time limit: the simulation test has no mandatory product time limit; finishing is explicit.

## Mandatory guardrails

- Keep backend session and evaluation behavior behind application/service/store boundaries when adding non-trivial logic; controllers stay thin.
- Keep Next.js pages as frontend adapters that orchestrate UI and API calls; do not place evaluation/reporting rules in pages.
- Keep Godot engine details out of domain contracts.
- Make preset, launch handoff, telemetry, critical-event, and report contracts explicit, versionable, and tested by producer and consumer sides.
- Do not authorize MVP shortcuts that add controller-to-database telemetry logic, Godot API leakage into core/contracts, or untested duplicated DTOs.

## MVP scope limits and anti-overengineering

- Implement one base preset before adding scenario libraries or editors.
- Report content should be basic: summary, minimal critical events, and relevant telemetry references for manual grading.
- Critical events should start with collision or another explicitly required event only.
- Avoid generic workflow engines, analytics pipelines, or complex rules engines for the MVP.

## Acceptance/test checklist

- Admin can create an organization; non-Admin cannot.
- Instructor can create a Pending session using a valid base preset; unknown presets are rejected.
- Trainee can start only their own Pending session and finish explicitly, without a required timeout.
- Handoff criteria include `sessionId`, `apiUrl`, and an authorization strategy when launch work is in scope.
- Critical events and reports, if changed, respect ownership and session state.
- Tests cover success, invalid role/ownership, invalid state, invalid preset, and contract compatibility where applicable.
