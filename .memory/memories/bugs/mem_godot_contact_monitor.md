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

VehicleController.cs conecta el evento `BodyEntered` pero `RigidBody3D` requiere `ContactMonitor = true` y `MaxContactsReported > 0` para que el evento se dispare. Sin esto, `_collidedThisFrame` siempre es `false` y la telemetría nunca reporta colisiones.

# Root cause

Desconocimiento del API de Godot 4 C#. En Godot 3, RigidBody detectaba colisiones sin configuración adicional. En Godot 4, los flags de contacto son explícitos.

# Impact

Medio — las colisiones no se registran en telemetría, pero el simulador funciona correctamente. La evaluación del instructor perderá datos de colisiones.

# Fix

Agregar en `_Ready()` antes de conectar el evento:
```
ContactMonitor = true;
MaxContactsReported = 1;
```

# Status

✅ Fixed 2026-05-27 — lines added to `_Ready()` before `BodyEntered += OnBodyEntered;`.

# Referencia

docs/99-reference/architecture-review.md — item 3
