---
id: mem_bug_godot_contact_monitor
type: bug
tags:
  - godot
  - collision
  - physics
  - high
---

# Issue

VehicleController.cs connects the `BodyEntered` event, but `RigidBody3D` requires `ContactMonitor = true` and `MaxContactsReported > 0` for the event to fire. Without this, `_collidedThisFrame` is always `false` and telemetry never reports collisions.

# Root cause

Godot 4 C# API knowledge gap. In Godot 3, RigidBody detected collisions without additional configuration. In Godot 4, contact flags are explicit.

# Impact

Medium — collisions are not recorded in telemetry, but the simulator works correctly. Instructor evaluation will lose collision data.

# Fix

Add in `_Ready()` before connecting the event:
```
ContactMonitor = true;
MaxContactsReported = 1;
```

# Status

✅ Fixed 2026-05-27 — lines added to `_Ready()` before `BodyEntered += OnBodyEntered;`.

# Reference

docs/99-reference/architecture-review.md — item 3
