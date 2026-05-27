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

- Godot 4 C# usa Godot.NET.Sdk/4.4.1 con target net8.0
- RigidBody3D requiere `ContactMonitor = true` y `MaxContactsReported > 0` para que el evento `BodyEntered` funcione — esto no estaba en la implementación inicial
- OS.GetCmdlineArgs() funciona en standalone pero no devuelve args en editor — hay que configurarlos manualmente en Debug > Customize Run > Arguments
- Las escenas .tscn son archivos de texto con formato Godot 4 — se pueden crear manualmente sin el editor

# Problems

- No hay Godot 4 instalado en la máquina de desarrollo — no se pudo verificar la escena ni compilar el proyecto C#
- BackendClient usa async void — funcional para fire-and-forget pero cualquier excepción no atrapada dentro del try-crash-ea la app
- Collision detection no funciona hasta que se agregue ContactMonitor = true

# Decisions Made

- Usar HttpClient de System.Net.Http en lugar del nodo HTTPRequest de Godot (más estándar para C#)
- Telemetría se envía en batches cada 10 frames (~166ms a 60fps)
- El vehículo es un RigidBody3D con fuerzas (no VehicleBody3D con ruedas) por simplicidad MVP
- CLI args: --session-id (obligatorio), --token (opcional), --api-url (opcional con default localhost:8080)

# Memories Created

- mem_bug_godot_contact_monitor
- mem_learning_godot4_csharp_args

# Next Steps

- Instalar Godot 4 para probar la escena y compilar C#
- Agregar ContactMonitor y MaxContactsReported al VehicleController
- Probar integración con backend (start → telemetry → finish)
