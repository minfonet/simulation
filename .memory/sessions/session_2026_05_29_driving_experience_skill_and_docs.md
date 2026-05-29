---
date: 2026-05-29
type: session
tags:
  - p2
  - driving-experience
  - skill
  - documentation
  - godot
---

# Session: Driving Experience Skill + Documentation Update

## Objective
Create the `godot-driving-experience` skill and update all project documentation with the new P2 goal: drift camera, HUD, multi-piece vehicle model, improved physics feel, and WorldEnvironment.

## What was done

### Created
- `.opencode/skills/godot-driving-experience/SKILL.md` — new project skill covering:
  - Multi-piece vehicle model from primitives (body, cabin, wheels, bumpers, lights)
  - Drift camera system (smooth follow, look-ahead, drift lean, exported properties)
  - CanvasLayer HUD (speed, steering, controls, finish button)
  - Improved physics feel (drift friction, weight transfer, tunable exported properties)
  - WorldEnvironment (sky, fog, lighting)
  - Acceptance checklist (10 items)
  - Mandatory guardrails (no telemetry changes, no BackendClient coupling, no engine API leaks)

### Updated documentation

| File | Changes |
|------|---------|
| `docs/01-product/user-flows.md` | Added §9 — In-Simulation Driving Experience (P2): target state, vehicle model diagram, camera pseudocode, HUD layout, physics goals, environment improvements, architecture boundaries table, acceptance criteria checklist |
| `docs/01-product/flow-gap-plan.md` | Added P2 row to prioritized gaps table (Driving Experience). Added 5 new remaining gaps (items 7-11: multi-piece car, drift camera, HUD, physics, WorldEnvironment). Reordered priority for next milestone |
| `docs/01-product/execution-readiness.md` | Added Flow 6 — Driving Experience (❌ NOT STARTED status). Listed 5 components needed, integration with existing flows, acceptance criteria, and validation procedure. Added P2 section to consolidated pending items |
| `docs/01-product/mdvp.md` | Updated Simulation layer scope row to mention P2. Split §9 Godot MVP into "Minimum scene (P0)" and "Enhanced scene (P2)" with specific details |
| `.opencode/agents/lead.md` | Added "Godot driving experience (camera, HUD, vehicle model, physics feel)" to the boundary classification list |
| `.memory/active-context.md` | Full update: P2 milestone line, 6th skill in all references, updated Current Focus, new Open Tasks (6 P2 items), updated risks, new Implementation Plan Phase 2, updated scaffolding table, updated flow gap documentation section, updated memory state session count |

### Files changed
- `.opencode/skills/godot-driving-experience/SKILL.md` (new - 193 lines)
- `.opencode/agents/lead.md` (1 line)
- `docs/01-product/mdvp.md` (8 lines)
- `docs/01-product/user-flows.md` (71 lines)
- `docs/01-product/flow-gap-plan.md` (16 lines)
- `docs/01-product/execution-readiness.md` (34 lines)
- `.memory/active-context.md` (40+ lines across 12 edits)

## Decisions

### New skill boundary
The `godot-driving-experience` skill covers visual/driving experience only. It explicitly does NOT overlap with `godot-telemetry-hal` (backend client, telemetry, HAL) or `backend-telemetry-reporting` (telemetry storage/query). The boundary is:
- Camera, HUD, vehicle model, and physics feel → `godot-driving-experience`
- Telemetry contracts, BackendClient, launch handoff, HAL → `godot-telemetry-hal`
- Backend telemetry/reporting → `backend-telemetry-reporting`

### P2 prioritization
First milestone: car model + drift camera (highest visual impact). Second: HUD. Third: physics feel. Fourth: WorldEnvironment polish. HUD Finish button is critical for the session lifecycle to work end-to-end from Godot.

### No scope changes to existing flows
P2 is purely cosmetic/physical within Godot. No new API endpoints, no telemetry contract changes, no session lifecycle changes. All existing 120 tests must continue to pass unchanged.

## Bugs found
None — documentation-only milestone.

## Next steps
1. **Implement P2 Milestone 1**: Multi-piece car model + drift camera in Main.tscn + new CameraController script
   - Delegate to IMPLEMENTER using `godot-driving-experience` skill
2. Implement P2 Milestone 2: CanvasLayer HUD
3. Implement P2 Milestone 3: Improved physics feel
4. Implement P2 Milestone 4: WorldEnvironment
5. Smoke test: verify 120/120 tests + visual validation in Godot
6. (Parallel) Fix pre-existing TS errors + page component tests
