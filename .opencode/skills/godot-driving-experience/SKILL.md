---
name: godot-driving-experience
description: godot-driving-experience, cockpit interior, vehicle model, drift camera, HUD, physics, and visual driving experience in Godot; use when improving the in-game driving look and feel.
---

# Godot Driving Experience

## When to use

- Use for Godot C# or scene (TSCN) changes that improve the visual driving experience: **cockpit interior view**, vehicle model, camera system, HUD, physics feel, environment, and lighting.
- Use when adding a **cockpit/first-person camera** inside the car with visible steering wheel, dashboard, and driver seat.
- Use when building a car model from primitives (body, cabin, wheels, bumpers, lights).
- Use when creating a CanvasLayer HUD (speedometer, steering angle, controls hint, finish button).
- Use when adding a third-person drift camera with smooth follow, look-ahead, and speed/drift lean (secondary — after cockpit).
- Use when improving vehicle physics for drift behavior (PID controllers, weight transfer, steering response).
- Use when improving the environment (sky, fog, ground detail, obstacles, lighting).

## When not to use

- Do not use for backend telemetry ingestion/storage/query (use `backend-telemetry-reporting`).
- Do not use for Godot backend client, launch handoff arguments, or session API calls (use `godot-telemetry-hal`).
- Do not use for telemetry data contracts or collision-to-critical-event derivation (use `godot-telemetry-hal`).
- Do not use for Next.js role pages or frontend UI (use `nextjs-role-ui`).
- Do not use for E2E smoke tests or Docker Compose (use `qa-e2e-simulation`).

## Read first

- `.memory/active-context.md`
- `docs/01-product/user-flows.md` (especially §9 — In-Simulation Driving Experience)
- `docs/01-product/flow-gap-plan.md`
- `docs/01-product/mdvp.md`
- `.memory/memories/architecture/mem_scalable_architecture_guardrails.md`
- `.memory/memories/learnings/mem_godot4_csharp.md`
- `docs/06-engineering/language-policy.md`
- `.opencode/skills/godot-telemetry-hal/SKILL.md` (telemetry/backend boundaries must be preserved)

## External references

These references are recommended for the IMPLEMENTER to study before writing code:

- **Video: "How to make a Drift Cam in Godot | Car Game Part 1"** — Rembot Games. Covers smooth third-person camera with drift offset. URL: `https://www.youtube.com/watch?v=90lCtfiZ6Hs`
- **Video: "How Add 'Driver Physics' in Godot | Car Game Part 2"** — Rembot Games. Covers car physics and control feel. URL: `https://www.youtube.com/watch?v=mazcG6ry0kE`
- **GitHub: tomaga-dev/DriftCarG4** — Full Godot 4.6 project with drift car, camera modes (first/third person), PID controllers, exported properties. URL: `https://github.com/tomaga-dev/DriftCarG4`
  - Has first-person camera mode that positions the camera at the driver's seat
  - Uses three PID controllers: Omega (angular velocity), Drift (drift angle), Steering (lateral force)
  - Lift-off oversteer mechanic: release accelerator while turning → drift
  - All tuning values are exported (`max_force`, `omega_max`, `omega_max_drift`, etc.)

## P2 priority order

The P2 driving experience milestones MUST be implemented in this order:

| Order | Milestone | Rationale |
|-------|-----------|-----------|
| 1 | **Cockpit interior** (camera inside car + steering wheel + dashboard) | User's explicit request: "Trainee dentro del auto" |
| 2 | **CanvasLayer HUD** (speed, steering, finish button) | Required for the user to get feedback while driving |
| 3 | **Third-person drift camera** (toggleable, smooth follow + drift lean) | Needed for external view; toggle with key (e.g. C) |
| 4 | **Improved physics feel** (drift, weight transfer, tunable exports) | Makes driving fun; referenced from DriftCarG4 |
| 5 | **WorldEnvironment** (sky, fog, lighting) | Final polish |

---

## Implementation guidance

### Milestone 1 — Cockpit interior (FIRST PRIORITY)

The Trainee starts already sitting inside the car. The camera is positioned at the driver's eye level, looking forward through the windshield. The steering wheel and dashboard are visible as 3D meshes.

**Scene tree:**

```
Car (RigidBody3D)
├── [all exterior meshes: body, cabin, wheels, bumpers, lights...]
├── Interior (Node3D)
│   ├── DriverSeat (BoxMesh, ~0.6 x 0.15 x 0.6, dark gray)     — visible in cockpit view
│   ├── Dashboard (BoxMesh, ~0.8 x 0.1 x 0.3, dark gray)       — in front of driver
│   ├── SteeringWheel (CylinderMesh or TorusMesh, r=0.15)       — in front of driver
│   ├── SteeringColumn (BoxMesh, small, dark)                   — connects wheel to dashboard
│   └── PedestriansSeat (BoxMesh, ~0.6 x 0.15 x 0.6)           — passenger seat (optional)
├── CameraCockpit (Camera3D)
│   └── Position: ~(0.6, 0.5, 0.3) relative to Car center     — driver's eye level
├── CameraPivot (Node3D)                                         — for third-person (future)
│   └── Camera3D (current=false)                                — third-person cam (future)
└── CollisionShape3D (BoxShape3D)                                — simplified collision
```

**Camera setup for cockpit:**
- `CameraCockpit` has `current = true` (active by default).
- Position: `(0.6, 0.5, 0.3)` — slightly right of center, at eye height, forward in the cabin.
- Rotation: `(0, 0, 0)` — looking forward along the car's -Z axis.
- No smoothing needed (it's fixed to the car).
- Field of view: `75-80` for a natural driving feel.
- Near clip: `0.05` to see the steering wheel clearly.
- Far clip: `500`.

**Steering wheel animation:**
- The SteeringWheel mesh rotates around its local X axis based on the current steering angle.
- In `VehicleController._PhysicsProcess()`:
  ```csharp
  var steerWheel = GetNode<Node3D>("Interior/SteeringWheel");
  steerWheel.Rotation = new Vector3(1.5708f, _steeringAngle * 2f, 0); // preserve base X rotation, steer on Y axis
  ```
- This gives visual feedback: the wheel turns left/right as the player steers.

**Dashboard instruments (3D meshes — minimal):**
- Two small BoxMesh children of Dashboard:
  - `SpeedGauge` (BoxMesh, ~0.08 x 0.08 x 0.01, white) — centered on dashboard, could be textured later.
  - `RPMGauge` (BoxMesh, ~0.08 x 0.08 x 0.01, white) — to the right of speed gauge.
- For MVP, the HUD (CanvasLayer) will show the actual numeric speed and RPM. The 3D gauges are visual placeholders.

**Rear-view mirrors (basic approach — no Viewport):**
- For MVP, do NOT implement Viewport-based mirrors (expensive).
- Instead, place two small box meshes (MirrorL, MirrorR, MirrorCenter) at mirror positions.
- They can have a reflective-ish material (high metalness, low roughness) for visual effect.
- Post-MVP: use a Viewport with a second Camera3D facing backward for real reflections.

**Toggle key (future):**
- After third-person camera is implemented (Milestone 3), pressing `C` toggles between `CameraCockpit.current` and `Camera3D.current`.

---

### Milestone 2 — HUD (CanvasLayer)

Create a CanvasLayer scene with:

```
CanvasLayer
├── SpeedLabel (Label)            — top-right: "Speed: 45 km/h"
├── SteeringBar (ProgressBar/TextureRect)  — bottom-center: steering wheel angle
├── GearLabel (Label)             — near speed: "Gear: 1" (optional MVP)
├── ControlsHint (Label)          — bottom: "WASD: drive | SPACE: brake | F: finish"
├── FinishButton (Button)         — top-right or bottom-right: "Finish Simulation"
└── SessionInfo (Label)           — top-left: "Session: {id} | Status: Active"
```

**Data flow:**
- Read speed from `VehicleController.LinearVelocity.Length()`
- Read steering from `VehicleController._steeringAngle` (expose via property)
- Session info from `BackendClient` (session ID, api URL)
- Finish button calls `BackendClient.FinishSession()` and then `GetTree().Quit()`

**Implementation pattern:**
- Add a public property to VehicleController: `public float CurrentSpeed => LinearVelocity.Length();`
- Add a public property: `public float CurrentSteering => _steeringAngle;`
- HUD script reads these via `GetNode<VehicleController>("..")` or a signal.
- Better: use a singleton/autoload GameState that VehicleController updates, and HUD reads from.

---

### Milestone 3 — Third-person drift camera

Implement a third-person camera that follows the car smoothly and reacts to driving dynamics. Toggleable with `C` key (when both cockpit and third-person cameras exist).

**Core behavior:**
- Smooth position follow: lerp camera position toward target (behind + above car).
- Look-ahead: rotate camera to look slightly in the car's velocity direction, not just where the car points.
- Drift lean: when the car is sliding (angle between velocity and forward), offset the camera laterally in the drift direction.
- Distance modulation: optionally pull camera closer at high speed or when drifting.
- Spring arm: maintain distance from car, push through walls if needed (RayCast).

**Pseudocode:**
```csharp
// In _PhysicsProcess or _Process:
var targetPos = Car.GlobalTransform.Origin + Car.GlobalTransform.Basis.Z * DistanceBehind;
targetPos += Vector3.Up * HeightAbove;

// Drift offset: lateral shift based on velocity vs forward angle
var velDir = Car.LinearVelocity.Normalized();
var forward = -Car.GlobalTransform.Basis.Z;
var driftAngle = Mathf.Abs(forward.AngleTo(velDir));
var driftDir = Car.GlobalTransform.Basis.X * Mathf.Sign(forward.Cross(velDir).Y);
targetPos += driftDir * driftAngle * DriftOffsetFactor;

// Smooth follow
CameraPivot.GlobalTransform.Origin = CameraPivot.GlobalTransform.Origin.Lerp(targetPos, smoothing * delta);

// Look at car or look-ahead point
Camera.LookAt(Car.GlobalTransform.Origin + Car.LinearVelocity * LookAheadFactor);
```

**Configurable properties (exported):**
- `DistanceBehind`: how far behind the car (default 4-5)
- `HeightAbove`: how high above (default 2.5-3)
- `Smoothing`: follow speed (default 4-6)
- `LookAheadFactor`: how far ahead to look (default 0.5-1)
- `DriftOffsetFactor`: how much to lean in drifts (default 1-2)
- `MinDistance`, `MaxDistance`: clamp follow distance

**Camera toggle (C key):**
```csharp
if (Input.IsKeyJustPressed(Key.C))
{
    cockpitCamera.Current = !cockpitCamera.Current;
    thirdPersonCamera.Current = !thirdPersonCamera.Current;
}
```

---

### Milestone 4 — Improved physics feel

**Current state:** Basic RigidBody3D with central force + torque.

**Target state:** Arcade-drift feel inspired by `DriftCarG4` PID approach:

- **Lift-off oversteer:** When the player releases the accelerator while turning, reduce rear grip to initiate drift.
- **PID-style controllers** (conceptual, adapted to C#):
  - *Omega controller:* regulates angular velocity (how fast the car rotates).
  - *Drift controller:* regulates drift angle (how sideways the car is).
  - *Steering controller:* ensures normal steering behavior by applying lateral forces to reduce sideways velocity.
- **Weight transfer:** Apply pitch torque on acceleration/brake, roll torque on steer (visual lean).
- **Steering response:** At low speed, full steering angle; at high speed, reduced steering (speed-sensitive).
- **Drift trigger:** When steering at speed, reduce forward friction on rear axis to initiate drift.
- **Counter-steer:** Allow recovery by steering opposite to drift direction.

Keep these as exported properties so tuning can happen in the editor without code changes:
- `EnginePower` (default 15000)
- `SteeringTorque` (default 5000)
- `BrakeForce` (default 20000)
- `GripFactor` (default 0.8 — lower = more drift)
- `DriftThreshold` (default 0.5 — speed fraction to trigger drift)
- `OmegaMax` (default 3.0 — max angular velocity in normal steering)
- `OmegaMaxDrift` (default 6.0 — max angular velocity in drift mode)

---

### Milestone 5 — Environment improvements

- Add `WorldEnvironment` node with `ProceduralSkyMaterial` for a nicer sky gradient.
- Add `Fog` in the Environment for depth and atmosphere.
- Add more obstacles, road-like markings, or a simple track layout.
- Add `OmniLight3D` or `SpotLight3D` for headlight effects.
- Optionally add a ground grid texture or road strip.

---

### Vehicle model (primitives-based)

Build a recognizable car from Godot primitives. Do not require external 3D assets for MVP.

```
Car (RigidBody3D)
├── Body (BoxMesh, ~1.8 x 0.25 x 3.6)       — main body, red
├── Cabin (BoxMesh, ~1.6 x 0.35 x 2.0)      — passenger cabin, darker red
├── BumperF (BoxMesh, ~1.9 x 0.12 x 0.2)    — front bumper, dark gray
├── BumperR (BoxMesh, ~1.9 x 0.12 x 0.2)    — rear bumper, dark gray
├── WheelFL (CylinderMesh, r=0.2 h=0.12)    — front left wheel, dark
├── WheelFR (CylinderMesh, r=0.2 h=0.12)    — front right wheel, dark
├── WheelRL (CylinderMesh, r=0.2 h=0.12)    — rear left wheel, dark
├── WheelRR (CylinderMesh, r=0.2 h=0.12)    — rear right wheel, dark
├── HeadlightL (BoxMesh, small, yellow)      — left headlight
├── HeadlightR (BoxMesh, small, yellow)      — right headlight
├── TaillightL (BoxMesh, small, red)         — left taillight
├── TaillightR (BoxMesh, small, red)         — right taillight
├── Interior (Node3D)                        — cockpit interior meshes
│   ├── DriverSeat, Dashboard, SteeringWheel, etc.
├── CameraCockpit (Camera3D, current=true)   — driver's POV
├── CameraPivot (Node3D)                     — for third-person (future)
│   └── Camera3D (current=false)             — third-person cam (future)
└── CollisionShape3D (BoxShape3D)            — simplified collision
```

Guidelines:
- Use `material_override` on each MeshInstance3D for colors (StandardMaterial3D albedo).
- Orient CylinderMesh wheels with rotation `(0, 0, 1.5708)` (cylinder Y-axis → X-axis for left-right axle).
- Keep collision shape as a single simplified BoxShape3D covering the main body.
- Position all meshes relative to the Car node; keep the Car node position for physics.

---

## Mandatory guardrails

- **Do not add Godot engine API calls to `BackendClient`** — keep it as a pure HTTP/API client. Camera, HUD, and physics are separate concerns.
- **Do not modify telemetry contracts** (`timestamp`, `speed`, `steeringAngle`, `positionX/Y/Z`, `collision`) unless explicitly coordinated with `backend-telemetry-reporting`.
- **Keep the existing session lifecycle** (Pending → Active → Completed → Evaluated) intact. Visual changes must not alter session state transitions.
- **Extract UI logic from VehicleController.** HUD should read state but not be coupled to VehicleController internals; use properties or signals.
- **Camera and HUD scripts should be testable outside the engine** where feasible (pure math for camera target calculation, HUD data formatting).
- **Keep drift physics tunable** via exported properties, not hardcoded magic numbers.
- **No hardware/input changes** in this skill — keyboard (WASD + Space) remains the MVP input method. HAL extraction is covered by `godot-telemetry-hal`.
- **Cockpit camera is the default view.** Third-person is added later as a toggle.
- **Do not implement Viewport-based mirrors in the first pass** — use static meshes with reflective material. Viewport mirrors are post-MVP due to performance cost.

## MVP scope limits and anti-overengineering

- **Build the car from primitives**; do not require Blender/FBX imports for MVP.
- **Cockpit interior is THE priority** — do not skip it in favor of third-person camera first.
- **Cockpit meshes:** steering wheel + dashboard + driver seat is sufficient. Do not model pedals, gear stick, center console, or door panels initially.
- **HUD:** speed + steering + finish button is sufficient. Do not implement tachometer needle, gear indicator, minimap, or lap timing initially.
- **Third-person camera:** smooth follow + drift lean is sufficient. Do not implement cinematic modes, multiple presets, or full spring-arm physics initially.
- **Physics:** basic drift feel (PID-style or friction reduction + weight transfer) is sufficient. Do not implement true tire model, suspension simulation, or aerodynamics.
- **Viewport mirrors:** do NOT implement in the first pass. Static reflective meshes only.
- **Environment:** sky + fog + a few more obstacles is sufficient. Do not build a track editor, terrain, or city.
- **Approach/walk animation** (Trainee approaching car, opening door, sitting down) is post-P2, not in scope for this milestone.

## Acceptance/test checklist

### Milestone 1 — Cockpit interior
- [ ] Cockpit camera is the default camera (current = true) when the scene loads.
- [ ] Camera is positioned at driver's eye level ~(0.6, 0.5, 0.3) relative to car center.
- [ ] Steering wheel mesh is visible in cockpit view and rotates with steering input.
- [ ] Dashboard mesh is visible in front of the driver.
- [ ] Driver seat mesh is visible (in peripheral vision or reflections).
- [ ] All exterior car meshes are still present (body, cabin, wheels, etc.).
- [ ] Existing telemetry, session lifecycle, and BackendClient remain unchanged.

### Milestone 2 — HUD
- [ ] Speed label shows km/h (updated every frame/physics tick).
- [ ] Steering bar/indicator shows current steering angle.
- [ ] Controls hint ("WASD | SPACE | F") is visible.
- [ ] Finish Simulation button calls `BackendClient.FinishSession()` and then `GetTree().Quit()`.
- [ ] Session ID and status are displayed.
- [ ] No Godot engine APIs leaked into pure classes.

### Milestone 3 — Third-person drift camera
- [ ] Camera follows smoothly with no jitter; responds to car velocity direction.
- [ ] Camera drifts/leans laterally when the car slides at speed.
- [ ] Pressing `C` toggles between cockpit and third-person view.
- [ ] All exported properties (`DistanceBehind`, `Smoothing`, etc.) are tunable in the editor.

### Milestone 4 — Physics
- [ ] Physics feel is noticeably more fun/arcade-like (easier to drift, weight transfer visible).
- [ ] Lift-off oversteer works: release accelerator while turning → drift.
- [ ] All tunable values are exported properties, not hardcoded.
- [ ] Car recovers from drift with counter-steer.

### Milestone 5 — Environment
- [ ] WorldEnvironment exists with ProceduralSkyMaterial.
- [ ] Fog or atmospheric effects are present.
- [ ] Lighting is improved (at minimum, directional light + ambient).

### Global
- [ ] All 120/120 existing tests still pass.
- [ ] No telemetry contract or BackendClient code was modified.
