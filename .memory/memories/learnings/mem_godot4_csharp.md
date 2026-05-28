---
id: mem_learning_godot4_csharp
type: learning
tags:
  - godot
  - csharp
  - physics
---

# Topic

Godot 4 C# has important differences compared with Godot 3 and GDScript.

# Details

- **ContactMonitor**: RigidBody3D requires `ContactMonitor = true` and `MaxContactsReported > 0` for `BodyEntered` to fire. In Godot 3 this was not necessary.
- **OS.GetCmdlineArgs()**: Works in standalone. In the Godot 4 editor, CLI args are configured in Debug → Customize Run → Arguments (they are not passed automatically).
- **Signals in C#**: Connected with `BodyEntered += Handler` (C# events), not with `Connect("body_entered", this, "MethodName")` as in GDScript.
- **Mathf**: Godot 4 C# has `Mathf` as a static class with methods such as `MoveToward`, `Lerp`, `Clamp`.
- **Input**: Uses `Input.IsActionPressed("action_name")` with actions defined in `project.godot`.
- **HttpClient**: Works in Godot 4 C# without issues. Native alternative: `HTTPRequest` node.
- **.csproj**: Uses `Godot.NET.Sdk/4.4.x` with `net8.0`. Does not inherit from Microsoft.NET.Sdk.

# Impact

Code written for Godot 3 or based on GDScript is not directly portable. Always verify Godot 4 C# syntax.
