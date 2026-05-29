---
id: mem_decision_p2_cockpit_first
type: decision
tags:
  - p2
  - godot
  - cockpit
  - priority
  - driving-experience
---

# Decision: P2 Cockpit Interior Is the First Priority

## Context
The P2 milestone was initially defined with the following order: multi-piece car model → drift camera → HUD → physics → environment. After showing the user the skill, they clarified the real goal: **"Trainee dentro del auto"** (Trainee inside the car). Later steps could include approaching the car, opening the door, and sitting down.

## Decision
Restructure the P2 implementation order so that **cockpit interior (M1) is the absolute first priority**. The new order is:

1. **M1 — Cockpit interior:** Driver's-eye camera fixed at ~(0.6, 0.5, 0.3) relative to car center, steering wheel mesh that rotates with input, dashboard mesh, driver seat mesh. This is the default (and initially only) view.
2. **M2 — CanvasLayer HUD:** Speed, steering, controls hint, Finish button.
3. **M3 — Third-person drift camera:** Smooth follow, look-ahead, drift lean, toggleable with C key.
4. **M4 — Improved physics:** Lift-off oversteer, PID-style regulation, weight transfer, tunable exports.
5. **M5 — WorldEnvironment:** Sky, fog, lighting.

## Rationale
- The user's explicit requirement is to see the steering wheel and dashboard while driving (cockpit view).
- Third-person camera is secondary — the user wants to "be inside the car" first.
- The existing car model (primitives) is already functional; we just need to add interior meshes and position the camera.
- This matches the approach in `DriftCarG4` (GitHub) which has a toggleable first-person camera mode.

## External references added to skill
- **Rembot Games** — "How to make a Drift Cam in Godot | Car Game Part 1" (third-person drift camera)
- **Rembot Games** — "How Add 'Driver Physics' in Godot | Car Game Part 2" (physics)
- **tomaga-dev/DriftCarG4** — Full Godot 4.6 drift car project with camera modes, PID controllers, exported properties

## Consequences
- The skill `godot-driving-experience` was rewritten: cockpit moved from "deferred" to "first priority" section.
- All documentation updated (user-flows, flow-gap-plan, execution-readiness, mdvp, active-context).
- Steering wheel rotation animation is a new requirement (not in the original P0/P1 scope).
- Existing Main.tscn's camera structure needs to be changed (add CameraCockpit child + Interior nodes).
- All 120/120 existing tests must continue to pass unchanged.
- Viewport-based mirrors remain post-P2 (too expensive for MVP).

## Status
Accepted 2026-05-29.
