---
date: 2026-05-29
type: session
tags:
  - p2
  - driving-experience
  - cockpit
  - priority
  - documentation
  - godot
  - skill
---

# Session: Cockpit-First Priority Restructure

## Objective
After analyzing the Rembot Games drift camera video and the DriftCarG4 GitHub repo, the user clarified the real goal: "Trainee dentro del auto" (inside the car). This required restructuring the entire P2 milestone to prioritize cockpit interior over third-person camera.

## What was done

### External references analyzed
- **"How to make a Drift Cam in Godot | Car Game Part 1"** (Rembot Games) — smooth third-person camera with drift lean
- **"How Add 'Driver Physics' in Godot | Car Game Part 2"** (Rembot Games) — car physics and control feel
- **tomaga-dev/DriftCarG4** (GitHub) — Godot 4.6 project with:
  - Toggleable first-person/third-person camera modes
  - PID controllers for drift physics (Omega, Drift, Steering)
  - Lift-off oversteer mechanic
  - All tuning values as exported properties

### Files changed

| File | Changes |
|------|---------|
| `.opencode/skills/godot-driving-experience/SKILL.md` | **Complete rewrite.** Restructured from "drift camera first" to "cockpit first". Added: external references section, milestone priority table, M1 cockpit interior with scene tree and camera positioning, steering wheel animation code, dashboard setup, rear-view mirror guidance (static, no Viewports). Moved third-person camera to M3. Updated anti-overengineering to reflect cockpit priority. Updated acceptance checklist per milestone. |
| `docs/01-product/user-flows.md` §9 | Restructured §9.2 to priority table with 5 milestones. Replaced old target state with milestone details (M1–M5). Renumbered architecture boundaries (9.5) and acceptance criteria (9.6) by milestone. |
| `docs/01-product/flow-gap-plan.md` | Updated P2 row in prioritized gaps table to list M1–M5 in order. Reordered remaining gaps items 7–11. Updated priority for next milestone: cockpit first. |
| `docs/01-product/execution-readiness.md` | Updated Flow 6: new priority table, restructured acceptance criteria by milestone, updated validation procedure per milestone. Updated P2 consolidated pending items. |
| `docs/01-product/mdvp.md` | Rewrote enhanced scene section as numbered priority list: M1 cockpit → M5 environment. |
| `.memory/active-context.md` | Updated P2 milestone line (M1 priority + external refs). Restructured P2 Implementation Plan as M1–M5. Updated Current Focus. Reordered Open Tasks P2 items to M1→M5. Updated risks with cockpit-specific details. |

## Decisions

### Cockpit-first priority
The entire P2 execution order was inverted. Previously: car model → drift camera → HUD → physics → environment. Now: **cockpit interior (M1) → HUD (M2) → third-person camera (M3) → physics (M4) → environment (M5)**.

### Steering wheel animation
The steering wheel mesh must rotate visually with input. This is implemented by accessing the SteeringWheel node from VehicleController and setting `Rotation.X = _steeringAngle * 2f` in `_PhysicsProcess`.

### No Viewport mirrors in MVP
Rear-view mirrors using Viewport + second Camera3D are deferred to post-P2 due to the rendering performance cost. Use static meshes with reflective material instead.

### External references in skill
The skill now has an "External references" section at the top with links to Rembot Games videos and DriftCarG4 GitHub. IMPLEMENTER should study these before writing code.

## Bugs found
None — documentation/memory restructure only.

## Next steps
1. **Delegate P2 M1 (Cockpit interior) to IMPLEMENTER** with the skill as guidance
2. After implementation → REVIEWER → QA → milestone complete
3. Continue with M2 (HUD), M3 (third-person camera), M4 (physics), M5 (environment)
