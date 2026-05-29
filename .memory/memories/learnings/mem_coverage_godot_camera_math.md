---
id: mem_coverage_godot_camera_math
type: learning
tags:
  - testing
  - godot
  - camera
  - coverage-gap
  - p2-m3
---

# P2 M3 — Third-Person Drift Camera: Coverage Gap for Pure Camera Math

## What is missing

FollowCamera.cs embeds all third-person camera math (smooth follow, drift lean, look-ahead, position calculation) inside a Godot `Node3D` script with direct dependencies on Godot types (`Vector3`, `Transform3D`, `Camera3D`, `Input`, etc.). This makes it impossible to unit test the position/lean/look-ahead logic without spinning up the Godot engine.

## Why it matters

- Drift lean logic computes `desiredPosition += leanDir * driftAngle * LeanAmount` — this is pure geometry that could be validated with deterministic tests.
- Look-ahead target computation is a simple vector operation: `carPos + velDir * LookAheadDistance`.
- Edge detection for C key toggle (`_cWasPressed`) is a finite-state pattern that could be unit tested.
- Without tests, regressions in camera behavior (e.g., broken lean direction, incorrect look-ahead, toggle misfire) would only be caught by visual inspection in the Godot editor.

## Recommended fix

Extract the camera target calculation into a pure C# static helper class (no Godot dependencies) that takes `Vector3` parameters and returns a target position + look-at point. Write unit tests for:

1. **Base position**: correct `carPos + carBack * Distance + Vector3.Up * Height`
2. **Drift lean**: lean offset direction matches drift side (sign of `forward.Cross(velDir).Y`)
3. **Drift lean zero-speed guard**: no lean when speed ≤ 1.0
4. **Look-ahead**: uses `velDir * LookAheadDistance` when speed > 0.5, falls back to `carPos` when stationary
5. **Edge detection state machine**: single toggle per press, no false triggers on hold

This follows the existing pattern in the project: `VehicleController` physics stays engine-bound but its telemetry collection is tested via pure data contract tests in `GodotSim.Tests`.

## When to address

Post-P2 M3, as a quality improvement. The current camera implementation is correct per static analysis and builds without errors, but lacks automated regression protection.
