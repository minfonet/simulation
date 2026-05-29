# Session: P2 M4 — Improved Physics Feel (Godot)

**Date:** 2026-05-29

## What was done
Implemented P2 M4 (Improved physics) — complete rewrite of VehicleController._PhysicsProcess with arcade-drift model.

### Changes

**MODIFIED `simulation/driving-sim/Scripts/VehicleController.cs`:**

**Updated exports:** EnginePower 800→15000, SteeringTorque 120→5000, BrakeForce 400→20000

**9 new exported properties:**
- GripFactor (0.8) — lower = more drift
- DriftThreshold (0.5) — speed fraction to trigger drift
- OmegaMax (3.0) — max angular velocity normal
- OmegaMaxDrift (6.0) — max angular velocity in drift
- WeightTransferPitch (5.0), WeightTransferRoll (3.0) — visual torque
- GripRestoreSpeed (2.0) — grip recovery rate
- DriftAngleThreshold (0.3) — min angle (rad) to trigger drift
- LateralGripForce (15.0) — lateral grip resistance multiplier

**New field:** `_driftFactor` — smooth 0-1 drift tracking

**Rewritten _PhysicsProcess** (9 sections):
1. Speed-sensitive steering: halved at 20 m/s
2. Acceleration/braking with higher values
3. Drift state: forward-velocity angle > threshold
4. Lift-off oversteer: throttle release while turning → grip reduction
5. Lateral grip force resists sliding
6. PID-style angular velocity: error torque + clamp
7. Weight transfer: pitch/roll torques
8. Telemetry: unchanged
9. Steering wheel: unchanged

**NOT modified:** BackendClient.cs, HudController.cs, FollowCamera.cs, Main.tscn

### Bugs found
1. **(MEDIUM)** Hardcoded drift angle threshold (0.3) — exported as `DriftAngleThreshold`
2. **(MEDIUM)** Hardcoded lateral grip multiplier (15) — exported as `LateralGripForce`
3. **(LOW)** Variable name `rght` confusing — renamed to `steerRight`, basis direction renamed to `basisRight`

### Decisions made
- Weight transfer not clamped — ground collision provides natural limit; avoiding overengineering
- Physics model uses simplified grip + omega control rather than full wheel-by-wheel simulation
- All tuning values exported for editor tuning without recompilation

### Next steps
- P2 M5: WorldEnvironment (ProceduralSkyMaterial, fog, improved lighting)

### Verification
- Build: 0 errors, 0 warnings
- Godot tests: 12/12 PASS
- All backend (66) + frontend (23) + E2E (19) tests unaffected
- Architecture gate: PASS — BackendClient untouched, telemetry unchanged
- Review: PASS WITH CHANGES (3 fixes applied)
- QA: PASS
