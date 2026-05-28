---
name: godot-telemetry-hal
description: godot-telemetry-hal, Godot, VehicleController, BackendClient, controls, HAL, telemetry, launch handoff, and simulator arguments; use when changing Godot simulation adapter code.
---

# Godot Telemetry HAL

## When to use

- Use for Godot C# changes involving `VehicleController`, `BackendClient`, startup arguments, session handoff, telemetry collection, controls, collisions, or simulator tests.
- Use when adding or refactoring control input through a HAL-style interface such as `IControlInputProvider`.
- Use when converting Godot events or physics state into telemetry or critical events.

## When not to use

- Do not use for backend-only telemetry storage/reporting or Next.js role UI unless the simulator contract changes.
- Do not use to add hardware/vendor/serial/HID code directly to simulation logic; that belongs behind a HAL adapter.

## Read first

- `.memory/active-context.md`
- `docs/01-product/flow-gap-plan.md`
- `docs/01-product/user-flows.md`
- `.memory/memories/architecture/mem_scalable_architecture_guardrails.md`
- `.memory/memories/bugs/mem_godot_telemetry_json_contract.md`
- `.memory/memories/learnings/mem_godot4_csharp.md`
- `docs/06-engineering/language-policy.md`

## Implementation guidance

- Keep `Node`, `RigidBody3D`, `Input`, `OS`, and `GD` usage inside adapter/node code.
- Extract pure classes for argument parsing, normalized control state, telemetry point creation, event derivation, backend session client behavior, and retry/buffering policy when feasible.
- Route control input through `IControlInputProvider` or an equivalent HAL abstraction before it reaches vehicle simulation logic.
- Normalize controls into a stable shape such as throttle, brake, steering, clutch, gear, buttons, and device metadata.
- Keep JSON names explicit for telemetry contracts; the backend expects camelCase fields such as `timestamp`, `speed`, `steeringAngle`, `positionX`, `positionY`, `positionZ`, and `collision`.
- Parse launch handoff values such as `sessionId`, `apiUrl`, and `token` in an adapter/pure parser and fail with controlled errors for missing or invalid values.

## Mandatory guardrails

- `VehicleController` must not keep accumulating responsibilities; prefer collaborators for input, telemetry, and backend communication.
- Godot-specific APIs must not leak into shared contracts, core simulation logic, backend clients, or tests that can be pure.
- Hardware integrations and vendor/device-specific mappings must be isolated behind HAL adapters.
- Telemetry producers emit DTO batches/events to backend contracts; they do not know database models.
- Shared telemetry, launch, and event contracts must be explicit, versionable, and tested from producer and consumer sides.

## MVP scope limits and anti-overengineering

- Keep keyboard controls acceptable for MVP, but do not block future HAL extraction.
- Do not implement complex device calibration, multi-device management, or vendor SDK support unless explicitly requested.
- Keep retry/buffering simple and testable; avoid building a full offline sync framework for MVP.
- Preserve the single base scene/config unless a task explicitly expands presets.

## Acceptance/test checklist

- Pure tests cover argument parsing, missing/invalid handoff values, telemetry JSON shape, and event derivation.
- HAL tests cover normalized control values independently of Godot `Input`.
- Backend client tests cover authorization header, API URL/session ID use, batch telemetry payload, and controlled error handling.
- Godot adapter code remains thin and is the only place using engine APIs.
- Contract changes are paired with backend/frontend compatibility tests or documented follow-up if explicitly out of scope.
