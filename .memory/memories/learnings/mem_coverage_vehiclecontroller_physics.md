---
id: mem_coverage_vehiclecontroller_physics
type: learning
tags:
  - coverage
  - godot
  - vehicle-controller
  - physics
  - unit-testing
---

# Coverage Blind Spot: VehicleController._PhysicsProcess

## What

`VehicleController._PhysicsProcess()` in `simulation/driving-sim/Scripts/VehicleController.cs` contains significant physics simulation logic (drift detection, grip calculation, lateral force computation, angular velocity PID regulation, weight transfer) that is **not covered by unit tests**.

## Why

The method depends on Godot engine types (`RigidBody3D`, `Input`, `Mathf`, `GlobalTransform`, `LinearVelocity`, `AngularVelocity`) and cannot run outside the Godot runtime. The 12 existing GodotSim.Tests cover only `BackendClient` and `TelemetryPoint` data structures — pure C# classes extracted from VehicleController.

## Impact

- Physics changes for M4 (Improved Physics Feel) were validated by file inspection and build only — no automated physics regression tests exist.
- Any future physics tweaks risk regressions that can only be caught by manual Godot play-testing.
- The `_driftFactor`, `currentGrip`, `isDrifting`, and `liftOff` state machine logic is mixed with Godot API calls and cannot be verified independently.

## Recommended follow-up (P2 M4.5 or P3)

Extract the following pure computation blocks into testable static methods or pure classes:

1. **Drift state detection** — inputs: `speed`, `forward`, `velDir`, `DriftAngleThreshold`, `DriftThreshold` → output: `(driftAngle, isDrifting)`
2. **Grip calculation** — inputs: `GripFactor`, `isDrifting`, `liftOff`, `_driftFactor`, `GripRestoreSpeed`, `delta` → output: `currentGrip`
3. **Lateral grip force** — inputs: `LinearVelocity`, `GlobalTransform.Basis`, `currentGrip`, `LateralGripForce` → output: `lateralForce` (Vector3)
4. **Angular velocity PID** — inputs: `_steeringAngle`, `AngularVelocity.Y`, `maxOmega`, `SteeringTorque`, `delta` → output: `omegaTorque` + clamped Y

Each pure function can be tested with xUnit + in-memory values, no Godot engine required.

## Related

- Architecture guardrail: `.memory/memories/architecture/mem_scalable_architecture_guardrails.md` §Godot Rules: "Vehicle physics orchestration may stay in Godot nodes, but control interpretation, telemetry point creation, session client behavior, and retry/buffering policy should be extracted to pure C# where feasible."
- Active context note: `VehicleController engine physics requires full Godot engine → extracted pure logic tested separately`
