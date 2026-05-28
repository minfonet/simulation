---
name: backend-telemetry-reporting
description: backend-telemetry-reporting, ITelemetryIngestor, ITelemetryStore, critical events, reports, telemetry ingestion, storage, query, ownership, and schema versioning; use for backend telemetry/report work.
---

# Backend Telemetry Reporting

## When to use

- Use for backend changes involving telemetry ingestion, telemetry retrieval, critical events, final reports, report queries, session ownership, or telemetry/report DTOs.
- Use when adding `ITelemetryIngestor`, `ITelemetryStore`, report query services, or equivalent application boundaries.
- Use when changing API contracts consumed by Godot or Next.js.

## When not to use

- Do not use for Godot-only input/physics work unless telemetry contracts change.
- Do not use for UI-only rendering unless report/telemetry backend contracts change.
- Do not use to add real-time transport directly into controllers or domain logic.

## Read first

- `.memory/active-context.md`
- `docs/01-product/flow-gap-plan.md`
- `docs/01-product/user-flows.md`
- `.memory/memories/architecture/mem_scalable_architecture_guardrails.md`
- `.memory/memories/bugs/mem_godot_telemetry_json_contract.md`
- `.memory/memories/learnings/mem_xunit_webapplicationfactory.md`
- `docs/06-engineering/language-policy.md`

## Boundary blueprint

- Controllers: authenticate, validate request shape, extract route/user context, delegate to application services, map responses.
- `ITelemetryIngestor`: validates batches/events, enforces session state and ownership, normalizes schema versions, and delegates persistence.
- `ITelemetryStore`: persists and retrieves telemetry/event records behind a storage boundary.
- Report query service: assembles completed-session report data from session, evaluation, telemetry, and critical-event sources without putting query rules in controllers.
- DTO/contracts: include explicit schema/version semantics for telemetry, critical events, and reports when new contracts are introduced.

## Mandatory guardrails

- Do not persist telemetry directly from controller logic for new telemetry work.
- Keep telemetry ingestion, validation, buffering, persistence, retrieval, and streaming as separate responsibilities.
- Keep telemetry, sessions, evaluation, auth, and organizations separable modules even inside one API deployable.
- Auth and ownership checks are acceptance criteria for ingestion and retrieval.
- Shared contracts must be explicit, versionable, and covered by producer/consumer tests.
- Real-time transport, if later added, must wrap stable query/stream contracts and not leak into UI or simulation domain logic.

## MVP scope limits and anti-overengineering

- Start with collision or explicitly requested critical events only.
- Build a basic report with summary, events, and relevant telemetry references for manual grading; do not implement advanced analytics or automatic scoring.
- Vanilla EF Core/PostgreSQL is sufficient for MVP; do not introduce TimescaleDB or a separate event platform without explicit approval.
- Prefer small service/store extraction around touched code over a broad backend rewrite.

## Acceptance/test checklist

- Integration tests cover authenticated ingestion, retrieval/report authorization, ownership isolation, invalid session state, invalid schema/version, and incomplete-session report behavior.
- Contract tests cover Godot-produced telemetry/event payloads and backend-accepted JSON names.
- Report tests verify completed-session summary, critical events, and evaluator access before grading.
- Controller tests or integration tests show controllers delegate behavior rather than embedding new persistence rules.
- API changes do not break existing frontend or Godot expectations unless corresponding consumers and tests are updated.
