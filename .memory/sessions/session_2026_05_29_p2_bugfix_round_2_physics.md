# Session: P2 Bugfix Round 2 — Physics & Steering Wheel

**Date:** 2026-05-29

## Objective
Fix remaining issues after the first bugfix round: car flying to sky on acceleration, steering wheel not visible/not rotating properly, and unresponsive brake/reverse.

## Diagnosis

| Issue | Root Cause |
|---|---|
| **Car flies to sky** | `ApplyCentralForce(forward * EnginePower)` where `forward = -GlobalTransform.Basis.Z`. Even 1° pitch gives the 15000 N force a ~262 N upward component (~26× gravity). Positive feedback: pitch → upward force → more pitch → car takes off. |
| **Steering wheel not visible / bad rotation** | TorusMesh was small (inner=0.08, outer=0.15). Rotation range of ±57° (`_steeringAngle * 2f`) was too subtle to see from cockpit camera. |
| **Brake/reverse feel unresponsive** | Car accelerates to absurd speeds (15000 m/s² with mass=1), making braking/reverse forces negligible by comparison. |

## Files Changed

1. **`simulation/driving-sim/Scripts/VehicleController.cs`**
   - **XZ-plane projection**: Engine/reverse force now applied on `flatForward` (forward with Y=0, normalized) instead of raw `forward`. This completely prevents the car from using engine power to lift off.
   - **Speed cap**: Max velocity clamped to 50 m/s (~180 km/h) after angular velocity clamping. Prevents absurd speeds.
   - **Steering wheel rotation**: Amplitude doubled from `_steeringAngle * 2f` to `_steeringAngle * 4f` (±115° range, similar to real steering wheel lock-to-lock).

2. **`simulation/driving-sim/Scenes/Main.tscn`**
   - **Steering wheel mesh enlarged**: TorusMesh inner_radius 0.08→0.12, outer_radius 0.15→0.22 (~50% larger).

## Tests
- **GodotSim.Tests (12/12)**: All passing, no regressions.
- **Architecture gate**: No changes to telemetry, BackendClient, or session lifecycle.

## Key Decisions
- **Ground-plane projection** chosen over reducing EnginePower or increasing mass because:
  - Preserves the intended drift physics feel (high forces for agile drifting)
  - Prevents flying regardless of car orientation
  - Common technique in arcade driving games
- **Speed cap** at 50 m/s (180 km/h) provides a reasonable top speed for the MVP track.
- **±115° steering wheel rotation** matches typical car steering range (~540° total = ±270° is real, but ±115° is more visible without being excessive).

## Next Steps
- User should re-test Godot simulation: car should stay on ground, steering wheel should visibly rotate left/right, brake (Space) and reverse (S) should be responsive.
- For the backend 400 error: ensure `--session-id`, `--token`, `--api-url` are passed when running with Docker Compose.
