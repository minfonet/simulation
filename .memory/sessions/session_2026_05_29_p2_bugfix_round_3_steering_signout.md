# Session: P2 Bugfix Round 3 — Steering Direction, Engine Power, Signout

**Date:** 2026-05-29

## Objective
Fix three remaining issues: (1) steering reversed (A turns right), (2) car still flies on acceleration, (3) signout doesn't redirect to login.

## Diagnosis

| Issue | Root Cause | Fix |
|---|---|---|
| **A turns right, D turns left (reversed)** | `targetOmega = _steeringAngle * maxOmega * 2f` — In Godot Y-up, positive Y angular velocity = CCW = LEFT turn. But `_steeringAngle` for left key = -0.5, giving negative targetOmega = RIGHT turn. Sign error. | `targetOmega = -_steeringAngle * maxOmega * 2f` |
| **Car still flies** | EnginePower=15000 with mass=1 creates 15000 m/s² acceleration. Even with XZ projection preventing direct upward force, the car bounces off obstacles/ground at 50 m/s due to extreme momentum. | Reduced EnginePower from 15000 → 800 (~20× reduction). With mass=1, acceleration ≈ 800 m/s², 0-50 m/s in ~0.06s (4 frames). |
| **Signout stays on dashboard** | `logout()` cleared localStorage and setUser(null) but didn't navigate. The proxy middleware (proxy.ts) is a pass-through with no auth check. | Added `window.location.href = "/login"` after clearing in `logout()`. |

## Files Changed

1. **`simulation/driving-sim/Scripts/VehicleController.cs`**
   - EnginePower: 15000 → 800
   - targetOmega: `_steeringAngle * ...` → `-_steeringAngle * ...`

2. **`src/frontend/sim-web/lib/auth-context.tsx`**
   - logout(): added `window.location.href = "/login"` after `setUser(null)`

## Tests

- **GodotSim.Tests**: 12/12 passing
- **Frontend (sim-web)**: 23/23 passing (api 10, proxy 6, auth-context 7)
- **Architecture gate**: No telemetry/session lifecycle changes

## Key Decisions
- EnginePower 800 chosen as a balance between playability and the arcade feel — car reaches max speed in ~4 frames, still very responsive but no longer explosive.
- `window.location.href` used instead of `useRouter` from next/navigation because AuthProvider is a context wrapper that doesn't need router imports; hard navigation is simpler and ensures complete state reset.

## Next Steps
- User should re-test:
  1. **W** to accelerate → car stays on ground, accelerates smoothly
  2. **A** → car turns LEFT (steering wheel rotates counter-clockwise)
  3. **D** → car turns RIGHT (steering wheel rotates clockwise)
  4. **Space** → brakes work (car is at manageable speed)
  5. **S** → reverse works
  6. **Click "Sign out"** → redirects to /login page
