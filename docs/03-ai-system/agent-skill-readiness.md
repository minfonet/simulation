# Agent/Skill Readiness for Flow Gap Execution

## Scope

Analyze whether the current agent architecture (`lead`, `implementer`, `reviewer`, `qa`) is ready to execute the pending Admin → Evaluator/Instructor → Trainee/Evaluated → Godot → report flow work, and when project knowledge should move into skills. This task documents only the proposal; it does not modify `.opencode` or create skills.

## Readiness summary

Current agents can execute the pending MVP work if the Lead keeps delegating small tasks with explicit boundaries and complete acceptance criteria. The prompts already contain strong guardrails for backend, Godot, HAL, telemetry, contracts, and QA. However, an important part of domain knowledge lives in `.memory/active-context.md`, sessions, scattered documents, and agent prompts; if the pending work becomes recurring or spreads across conversations, that knowledge should be extracted into project-specific skills to reduce repetition and errors.

## Current capabilities

| Agent | Useful capabilities for this pending work | Status |
|---|---|---|
| Lead | Reads context/docs/agents, classifies boundary, delegates implementation/review/QA, enforces guardrails, maintains memory. | Sufficient to coordinate the pending work if it includes flow-specific criteria. |
| Implementer | Reads memory/decisions/bugs/learnings, declares boundary, keeps controllers/pages/Godot scripts thin, uses HAL/telemetry boundaries, avoids duplicated DTOs without tests. | Sufficient to implement small pieces; needs prompts with exact files and expected contracts. |
| Reviewer | Rejects architecture violations, reviews auth, cross-layer contracts, Godot decoupling, HAL, and telemetry pipeline. | Sufficient as a gate if it receives flow context and rejection criteria. |
| QA | Creates/runs frontend/backend/Godot/E2E tests, protects boundaries, requires producer/consumer tests for DTOs, documents gaps. | Sufficient to validate; must receive a test matrix by flow and expected runtime. |

## Missing or over-embedded capabilities

- **Requested flow domain**: base preset, Godot launch, auth/token for Godot, critical events, and final report are documented in memory/sessions but not packaged as reusable guidance.
- **Cross-cutting contracts**: presets, launch handoff, telemetry/event/report DTOs require producer/consumer discipline; today the guidance exists as a general guardrail, not as a domain checklist.
- **Godot + HAL + telemetry**: agents know how to isolate Godot APIs and use HAL, but they do not have a specific recipe for evolving `VehicleController`/`BackendClient` without increasing coupling.
- **Backend reporting/telemetry**: prompts prohibit controller-to-DB logic for new telemetry, but they do not describe a concrete `ITelemetryIngestor`/`ITelemetryStore`/query service/report service blueprint.
- **Next.js role UI**: general UI and testing rules exist, but there is no specific guide for role pages, auth refresh/localStorage/proxy, and session states.
- **Simulation E2E**: QA knows the smoke script, but there is no skill/checklist for executing/verifying the complete flow with live services, Godot handoff, and reports.

## Are the current prompts/guardrails enough?

Yes, for the immediate MVP, as long as each Lead task includes:

- primary boundary and secondary boundaries;
- files to read and contracts that must not be broken;
- role-specific verifiable acceptance criteria;
- requirement for `ITelemetryIngestor`/`ITelemetryStore` or equivalents for telemetry/reporting;
- Godot API isolation and HAL for expanded controls;
- tests for authorization, ownership, producer/consumer contracts, and relevant pages.

It is not advisable to depend only on the current prompts if several preset/handoff/events/reporting implementation tasks are chained, because the context must be repeated manually and may be omitted. At that point, project skills would reduce cognitive load and make execution more consistent.

## When project-specific skills should be created

Create skills when one or more conditions apply:

- The same task type will repeat across several sessions, for example telemetry/reporting or role UI.
- The Lead prompt needs to copy long checklists to avoid errors.
- There is a risk of mixing engine APIs with core, controllers with persistence, or duplicated DTOs without tests.
- QA needs a stable recipe for E2E with backend/frontend/Godot.
- New agents or collaborators must operate without reading the full `.memory` history.

## Suggested skills (proposal, not implementation)

| Skill | When to use it | Recommended content |
|---|---|---|
| `simulation-domain` | Define or change MVP business flows. | Roles, session states, base preset, manual evaluation, no advanced analytics, criteria by flow. |
| `godot-telemetry-hal` | Changes in Godot, controls, simulator telemetry, or handoff. | Isolation of `Node/Input/OS/GD`, args parsing, session client, telemetry collector, HAL `IControlInputProvider`, pure tests. |
| `backend-telemetry-reporting` | Critical events, report, telemetry ingestion/query/storage. | Blueprint for `ITelemetryIngestor`, `ITelemetryStore`, report query service, ownership/auth, schema versioning, integration/contract tests. |
| `nextjs-role-ui` | Admin/Instructor/Trainee pages, auth, and flow states. | Route map, auth localStorage/proxy caveats, UI states, page/component testing with Vitest/Testing Library. |
| `qa-e2e-simulation` | Validate the full flow with live services. | Docker/API prerequisites, smoke script, seeding, Godot launch/handoff checks, telemetry/event/report assertions, failure triage. |

## How to add skills if needed

Without doing it in this task, a future implementation should:

1. Identify the minimum skill and its clear trigger.
2. Move stable knowledge from docs/memory into a reusable skill guide, avoiding secrets or ephemeral state.
3. Keep agents more agnostic: Lead coordinates, Implementer implements, Reviewer reviews, QA tests; the skill provides domain/checklists.
4. Reference source documents (`docs/01-product/user-flows.md`, `docs/01-product/flow-gap-plan.md`, architecture guardrails) instead of duplicating them fully.
5. Define prompt examples and acceptance criteria by boundary.
6. Validate that the skill does not authorize shortcuts that contradict guardrails.

## Recommendation

For the next P0 pending items (base preset, Godot launch handoff, auth/token), current agents are sufficient if the Lead uses `flow-gap-plan.md` as the acceptance criteria source. Before starting P1 (critical events and final report), create at least `backend-telemetry-reporting` and `godot-telemetry-hal`, because those tasks cross sensitive boundaries and have high coupling risk. If UI/E2E coverage expands, `nextjs-role-ui` and `qa-e2e-simulation` would also add value.
