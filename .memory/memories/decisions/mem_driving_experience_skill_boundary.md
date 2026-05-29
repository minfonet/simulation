---
id: mem_decision_driving_experience_skill
type: decision
tags:
  - skill
  - godot
  - driving-experience
  - p2
  - architecture
---

# Decision: Create `godot-driving-experience` Skill for P2

## Context
The Godot simulation scene (`Main.tscn`) currently has a minimal visual appearance: car is a single red box, camera is basic, no HUD, physics are rigid. The user requested a "drift camera experience" with visible steering wheel, panel, and mirrors.

## Decision
Create a new opencode project skill `godot-driving-experience` that documents patterns for:
1. Multi-piece vehicle model from primitives
2. Drift camera system (smooth follow, look-ahead, lean)
3. CanvasLayer HUD (speed, steering, controls, finish button)
4. Improved physics feel (drift, weight transfer)
5. WorldEnvironment (sky, fog, lighting)

## Rationale
- The existing `godot-telemetry-hal` skill covers backend client, telemetry, HAL, and launch handoff. Mixing visual/camera/HUD guidance into it would violate single-responsibility and confuse implementers.
- A dedicated skill keeps boundaries clean: camera/HUD/physics → `godot-driving-experience`; telemetry/backend → `godot-telemetry-hal`; backend reports → `backend-telemetry-reporting`.
- The skill provides reusable patterns (pseudocode for camera math, HUD layout, physics formulas) that the IMPLEMENTER agent can reference directly.

## Consequences
- 6 skills total now: simulation-domain, godot-telemetry-hal, backend-telemetry-reporting, nextjs-role-ui, qa-e2e-simulation, godot-driving-experience
- opencode must be restarted to load the new skill
- All documentation updated to reference the new P2 scope
- Existing telemetry, session lifecycle, and BackendClient must remain unchanged by P2 work

## Status
Accepted 2026-05-29.
