---
session_id: session_2026_05_27_godot_scaffolding
agent: lead
---

# Goals

- Scaffold Godot 4 C# driving simulation project
- Create vehicle controller with WASD + space controls
- Implement telemetry emission and backend communication
- Define scene with ground, obstacles, and third-person camera

# Findings

- Godot 4 C# uses Godot.NET.Sdk/4.4.1 with target net8.0
- RigidBody3D requires `ContactMonitor = true` and `MaxContactsReported > 0` for the `BodyEntered` event to work — this was not in the initial implementation
- OS.GetCmdlineArgs() works in standalone but does not return args in the editor — configure them manually in Debug > Customize Run > Arguments
- .tscn scenes are text files with Godot 4 format — they can be created manually without the editor

# Problems

- Godot 4 is not installed on the development machine — could not verify the scene or compile the C# project
- BackendClient uses async void — functional for fire-and-forget, but any exception not caught inside the try crashes the app
- Collision detection does not work until ContactMonitor = true is added

# Decisions Made

- Use HttpClient from System.Net.Http instead of Godot's HTTPRequest node (more standard for C#)
- Telemetry is sent in batches every 10 frames (~166ms at 60fps)
- The vehicle is a force-based RigidBody3D (not VehicleBody3D with wheels) for MVP simplicity
- CLI args: --session-id (required), --token (optional), --api-url (optional with localhost:8080 default)

# Memories Created

- mem_bug_godot_contact_monitor
- mem_learning_godot4_csharp_args

# Next Steps

- Install Godot 4 to test the scene and compile C#
- Add ContactMonitor and MaxContactsReported to VehicleController
- Test integration with backend (start → telemetry → finish)
