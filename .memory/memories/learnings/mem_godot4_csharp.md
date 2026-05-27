---
id: mem_learning_godot4_csharp
type: learning
tags:
  - godot
  - csharp
  - physics
---

# Topic

Godot 4 C# tiene diferencias importantes respecto a Godot 3 y GDScript.

# Details

- **ContactMonitor**: RigidBody3D requiere `ContactMonitor = true` y `MaxContactsReported > 0` para que `BodyEntered` se dispare. En Godot 3 esto no era necesario.
- **OS.GetCmdlineArgs()**: Funciona en standalone. En el editor de Godot 4, los CLI args se configuran en Debug → Customize Run → Arguments (no se pasan automáticamente).
- **Señales en C#**: Se conectan con `BodyEntered += Handler` (eventos C#), no con `Connect("body_entered", this, "MethodName")` como en GDScript.
- **Mathf**: Godot 4 C# tiene `Mathf` como clase estática con métodos como `MoveToward`, `Lerp`, `Clamp`.
- **Input**: Se usa `Input.IsActionPressed("action_name")` con las actions definidas en `project.godot`.
- **HttpClient**: Funciona en Godot 4 C# sin problemas. Alternativa nativa: nodo `HTTPRequest`.
- **.csproj**: Usa `Godot.NET.Sdk/4.4.x` con `net8.0`. No hereda de Microsoft.NET.Sdk.

# Impact

Código escrito para Godot 3 o basado en GDScript no es directamente portable. Verificar siempre la sintaxis C# de Godot 4.
