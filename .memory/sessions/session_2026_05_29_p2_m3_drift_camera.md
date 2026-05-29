# Session: P2 M3 — Third-Person Drift Camera (Godot)

**Date:** 2026-05-29

## What was done
Implemented P2 M3 (Third-person drift camera) for the Godot driving simulation.

### Changes

**NEW `simulation/driving-sim/Scripts/FollowCamera.cs`** (87 lines):
- Exported properties: Distance=6, Height=3, SmoothSpeed=5, LookAheadDistance=2, LeanAmount=1.5
- `_Ready()`: Gets VehicleController, camera, cockpitCamera references. Starts in cockpit view.
- `_Process()`:
  - C key toggle with rising-edge detection (`_cWasPressed`)
  - Third-person: computes desired position (behind+above car + drift lean lateral offset)
  - Smooth lerp via `GlobalPosition.Lerp(desired, delta * SmoothSpeed)`
  - `_camera.LookAt(lookTarget)` — looks ahead in velocity direction, falls back to carPos when stopped
- Drift lean uses `_car.GlobalTransform.Basis.X * Mathf.Sign(cross.Y)` for lateral (left/right) offset

**MODIFIED `simulation/driving-sim/Scenes/Main.tscn`:**
- `load_steps` 21→22
- Added `ext_resource id=3` for FollowCamera.cs
- CameraPivot parent changed from `"Car"` → `"."` (World-level sibling)
- CameraPivot now has `script = ExtResource("3")`
- Camera parent changed from `"Car/CameraPivot"` → `"CameraPivot"`
- ControlsHint text updated to include `"C: Toggle Camera"`

**NOT modified:** BackendClient.cs, VehicleController.cs, HudController.cs remain untouched.

### Bugs found
1. **(MEDIUM) Drift lean produced vertical offset instead of lateral** — `cross.Normalized()` gives the perpendicular axis (up/down) for horizontal drift. Fixed to use `_car.GlobalTransform.Basis.X * Mathf.Sign(cross.Y)` for lateral (left/right) offset.

### Decisions made
- CameraPivot moved to World level (not child of Car) so position lerp operates in world space independently of car movement
- Manual C key edge detection via `_cWasPressed` flag — simpler than InputMap action registration for MVP
- All tuning values exported and editable in inspector (Distance, Height, SmoothSpeed, etc.)
- Drift lean uses driftAngle * LeanAmount for natural-feeling offset proportional to slide intensity

### Next steps
- P2 M4: Improved physics feel (lift-off oversteer, PID-style regulation, weight transfer, tunable exports)

### Verification
- Build: 0 errors, 0 warnings
- Godot tests: 12/12 PASS
- All backend (66) + frontend (23) + E2E (19) tests unaffected
- Architecture gate: PASS — BackendClient untouched, telemetry unchanged, camera logic self-contained
- Review: PASS WITH CHANGES (1 drift lean fix applied)
- QA: PASS
