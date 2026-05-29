# Session: P2 M1 — Cockpit Interior (Godot)

**Date:** 2026-05-29

## What was done
Implemented P2 M1 (Cockpit Interior) for the Godot driving simulation.

### Changes

**`simulation/driving-sim/Scenes/Main.tscn`:**
- Changed `load_steps` from 14 → 20
- Added 6 new sub_resources:
  - `id=14`: StandardMaterial3D (dark gray 0.2,0.2,0.2) for steering wheel, column, seat
  - `id=15`: StandardMaterial3D (darker 0.15,0.15,0.15) for dashboard
  - `id=16`: TorusMesh (inner_r=0.08, outer_r=0.15) for steering wheel geometry
  - `id=17`: BoxMesh (0.06×0.06×0.2) for steering column
  - `id=18`: BoxMesh (0.6×0.08×0.25) for dashboard
  - `id=19`: BoxMesh (0.5×0.12×0.5) for driver seat
- Added 7 new nodes under `Car`:
  - `Interior` (Node3D) — grouping node
  - `Interior/SteeringWheel` (MeshInstance3D) — TorusMesh, pos=(0.5,0.15,0.25), rot=(1.5708,0,0)
  - `Interior/SteeringColumn` (MeshInstance3D) — BoxMesh, pos=(0.5,0.1,0.25)
  - `Interior/Dashboard` (MeshInstance3D) — BoxMesh, pos=(0.5,0.1,0.15)
  - `Interior/DriverSeat` (MeshInstance3D) — BoxMesh, pos=(0.4,-0.1,0)
  - `CameraCockpit` (Camera3D) — current=true, pos=(0.6,0.5,0.3), near=0.05, fov=75
- Changed `Car/CameraPivot/Camera`: current=true → false

**`simulation/driving-sim/Scripts/VehicleController.cs`:**
- Added `CurrentSpeed` property (→ `LinearVelocity.Length()`)
- Added `CurrentSteering` property (→ `_steeringAngle`)
- Added steering wheel rotation in `_PhysicsProcess`:
  ```csharp
  steerWheel.Rotation = new Vector3(1.5708f, _steeringAngle * 2f, 0);
  ```
  Preserves base X rotation (1.5708 = 90°), applies steering animation on Y axis.

**`simulation/driving-sim/Scripts/BackendClient.cs`:** NOT modified.

**`.opencode/skills/godot-driving-experience/SKILL.md`:**
- Fixed steering rotation formula in documentation to preserve base X orientation

### Bugs found
1. **(CRITICAL) Steering wheel rotation overrides base orientation** — Initial implementation used `new Vector3(_steeringAngle * 2f, 0, 0)`, which overwrote the scene's X=1.5708 rotation and used the wrong Euler axis. Fixed to `new Vector3(1.5708f, _steeringAngle * 2f, 0)` which preserves the vertical wheel orientation and applies steering as Y rotation (yaw).

### Decisions made
- Cockpit camera at Y=0.3 is below roof height (Y=0.4) — acceptable for primitive box car; will be resolved with M4 body reshaping or separate Cabin mesh
- Steering wheel animation kept in VehicleController (not extracted to separate class) because it's a 3-line physics-linked visual feedback; will be extracted if it grows
- Camera far=500 for cockpit is generous; acceptable for MVP, may reduce to 100-200 later for depth precision
- No new tests added because changes are visual/physics (Godot engine-dependent) — existing 12 tests cover BackendClient + telemetry contracts (both unchanged)

### Next steps
- P2 M2: CanvasLayer HUD (speed label, steering bar, controls hint, Finish button)
- P2 M3: Third-person drift camera toggle
- P2 M4: Improved physics feel
- P2 M5: WorldEnvironment

### Verification
- Build: 0 errors, 0 warnings
- Godot tests: 12/12 PASS
- All backend (66) + frontend (23) + E2E (19) tests unaffected — 120/120 total
- Architecture gate: PASS — BackendClient untouched, telemetry unchanged, session lifecycle unchanged
- Review: PASS WITH CHANGES (1 critical fix applied)
- QA: PASS
