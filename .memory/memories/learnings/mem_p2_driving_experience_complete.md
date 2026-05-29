# P2 Driving Experience — Complete (including 4 Bugfix Rounds)

**Date:** 2026-05-29

## Summary
All 5 milestones of the P2 Driving Experience enhancement plus 4 post-delivery bugfix rounds have been delivered, reviewed, and QA-validated. User confirmed "mucho mejor" after Round 4.

## Milestones delivered

| Milestone | What was built | Key files |
|-----------|---------------|-----------|
| **M1 — Cockpit Interior** | Driver's-eye camera at (0.6, 0.5, 0.3), TorusMesh steering wheel (Y-axis rotation), BoxMesh dashboard/seat/column | Main.tscn, VehicleController.cs |
| **M2 — CanvasLayer HUD** | SpeedLabel (km/h), SteeringLabel (visual bar), ControlsHint, FinishButton → FinishAndQuit() with _sessionFinished guard | HudController.cs (NEW), Main.tscn, VehicleController.cs |
| **M3 — Third-person drift camera** | Smooth lerp follow, look-ahead in velocity direction, lateral drift lean via Basis.X * Sign(cross.Y), C key toggle with edge detection | FollowCamera.cs (NEW), Main.tscn |
| **M4 — Improved physics** | Speed-sensitive steering, drift detection (forward-velocity angle), lift-off oversteer, lateral grip force, PID-style angular velocity control, weight transfer. 9 exported properties. | VehicleController.cs |
| **M5 — WorldEnvironment** | ProceduralSkyMaterial (blue gradient), Sky resource, Environment with height fog | Main.tscn |

## Post-delivery bugfix rounds

After initial P2 delivery, 4 iterative bugfix rounds were required:

| Round | Issues fixed | Root cause |
|-------|-------------|------------|
| **R1** | Car flips skyward, cockpit invisible, sky flat, camera disoriented, "BadRequest" session start | Weight transfer too aggressive (5.0), body mesh occluded interior, sky had no explicit sun, camera gimbal-lock |
| **R2** | Car flies up on acceleration, steering wheel too small, brake/reverse unresponsive | Engine/reverse force had Y component, speed uncapped (50+ m/s), wheel inner/outer radii too small |
| **R3** | Steering reversed (A→right), car bounces uncontrollably, signout stays on dashboard | Wrong steering sign in targetOmega, EnginePower=15000 too high, no hard navigation on logout |
| **R4** | Steering still wrong (final fix), car lifts on lateral/brake forces, "axis must be normalized" crash | targetSteer values inverted, central forces not clamped to Y=0, LookAt zero-direction edge case |

**Key lesson**: All 4 rounds preserved architecture boundaries (BackendClient untouched, telemetry/session unchanged).

## Architecture guardrails respected
- BackendClient.cs: **untouched** across all 5 milestones
- Telemetry contracts: **unchanged** (TelemetryPoint struct, batch format, collection frequency)
- Session lifecycle: **unchanged** (Pending→Active→Completed→Evaluated)
- All camera/HUD/physics logic stays in Godot node scripts (adapter layer)
- No Godot engine APIs leaked into pure classes or backend contracts

## Test results
- Godot simulation: 12/12 PASS
- Backend integration: 66/66 PASS
- Frontend unit: 23/23 PASS
- E2E smoke: 19/19 PASS
- **Total: 120/120 all passing**

## Bugs fixed during implementation (M1-M5)
1. Steering wheel rotation overwriting base orientation (M1) — fixed rotation axis
2. Double FinishSession() call (M2) — added _sessionFinished guard
3. SpeedLabel/FinishButton overlap (M2) — moved SpeedLabel to top-left
4. Drift lean producing vertical offset (M3) — switched to Basis.X * Sign(cross.Y)
5. Hardcoded drift angle threshold and lateral grip multiplier (M4) — exported as properties
6. Confusing variable name `rght` (M4) — renamed to steerRight

## Bugs fixed post-delivery (R1-R4)
7. Car flips on acceleration — WeightTransferPitch 5→0.3, angular damping ×0.92
8. Cockpit interior occluded — BodyMesh changed from full enclosure (2×0.8×4) to thin chassis floor (1.8×0.2×3.8 at Y=-0.15)
9. Sky flat — explicit sun parameters (sun_angle_max=30, sun_latitude=35, sun_longitude=150)
10. Camera gimbal-lock crash — tiny offset when look direction parallels up vector
11. Car flies upward — engine/reverse force projected to XZ plane
12. Speed cap missing — capped at 50 m/s
13. Steering wheel too small — inner 0.08→0.12, outer 0.15→0.22, rotation ±115°
14. EnginePower extreme — 15000→800 (~20× reduction)
15. Steering reversed — targetSteer swapped (left=0.5, right=-0.5), targetOmega sign reverted
16. Anti-lift incomplete — all central forces Y=0 (lateral grip, brake)
17. LookAt crash — zero-length direction check before LookAt()
18. Signout no redirect — window.location.href = "/login"

## Key decisions
- CameraPivot moved to World level (not child of Car) for independent lerp movement
- HUD reads VehicleController properties directly (no GameState singleton for MVP)
- Physics uses simplified grip + omega control (not full wheel simulation)
- All tuning values exported for editor tuning without recompilation
- C key toggle uses manual edge detection (not InputMap actions)

## Remaining pre-existing gaps (not P2 scope)
- Integration test for scenario-presets endpoint
- Frontend page component tests (admin, instructor, trainee)
- TypeScript errors in api.test.ts
- REST polling architecture, no SignalR
- HAL abstraction not yet extracted from VehicleController
- Session/evaluation controllers still use direct EF access
