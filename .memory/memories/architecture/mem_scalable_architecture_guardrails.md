---
id: mem_architecture_scalable_guardrails
type: architecture
tags:
  - architecture
  - modularity
  - godot
  - hal
  - telemetry
---

# Scalable Architecture Guardrails

These guardrails are mandatory for implementation, review, and QA. They turn the target scalable architecture into acceptance criteria.

## Target Direction

The system must evolve from the current MVP vertical slice into a modular architecture with explicit boundaries:

- modular backend/application boundaries
- Godot engine code isolated as an adapter
- hardware abstraction layer for all physical or simulated controls
- independent telemetry pipeline with clear contracts and storage boundaries

## Non-Negotiable Rules

- Do not add new business logic directly to ASP.NET controllers, Next.js pages, or Godot node scripts when it can live behind an application/core service.
- Do not let Godot-specific APIs leak into simulation domain logic, telemetry contracts, input mapping, or backend clients.
- Do not read hardware or controls directly from domain logic. All control input must go through an abstraction such as `IControlInputProvider` that returns normalized control state.
- Do not persist telemetry directly from controller logic for new telemetry work. Use an ingestion boundary such as `ITelemetryIngestor` and storage boundary such as `ITelemetryStore`.
- Do not duplicate cross-layer contracts casually. Shared contracts must be explicit, versionable, and tested from both producer and consumer sides.
- Do not add a vendor SDK, serial/HID integration, or device-specific mapping without isolating it behind a HAL adapter.
- Do not introduce real-time transport details into UI or simulation domain logic. SignalR, SSE, WebSocket, or REST polling must be adapters around a stable telemetry query/stream contract.

## Backend Rules

- Controllers should be thin: auth/context extraction, request validation, delegation to application services, response mapping.
- New use cases should live in feature/application services, not in controllers.
- Data access should be behind a repository/store/service boundary when adding or refactoring non-trivial behavior.
- Telemetry, sessions, evaluation, auth, and organizations must remain separable modules even if they live in one deployable API.

## Godot Rules

- Godot `Node`, `RigidBody3D`, `Input`, `OS`, and `GD` usage belongs only in adapter classes.
- Vehicle physics orchestration may stay in Godot nodes, but control interpretation, telemetry point creation, session client behavior, and retry/buffering policy should be extracted to pure C# where feasible.
- `VehicleController` must not keep accumulating responsibilities. Prefer extracted collaborators for input, telemetry collection, and backend communication.

## HAL Rules

- All controls must normalize into a stable shape such as throttle, brake, steering, clutch, gear, buttons, and device metadata.
- Keyboard, gamepad, HID, serial, and vendor SDK integrations are adapters implementing the same input provider contract.
- Calibration and mapping must be data-driven and persistable, not hard-coded into vehicle behavior.

## Telemetry Rules

- Telemetry ingestion, validation, buffering, persistence, retrieval, and streaming are separate responsibilities.
- Producers should emit batches/events to a telemetry boundary, not write directly to database-specific models.
- The telemetry schema must be explicit and versionable.
- Retrieval for historical data and streaming for live data must share contract semantics but not necessarily transport.
- Auth and ownership checks are part of telemetry ingestion and retrieval acceptance criteria.

## Review Gate

Any implementation that crosses these boundaries must be rejected unless the prompt explicitly authorizes a temporary MVP shortcut and documents the reason, follow-up, and affected files.
