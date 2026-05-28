# Bug: Godot TelemetryPoint serialized PascalCase instead of backend camelCase contract

**Date discovered:** 2026-05-28
**Detected by:** `dotnet test tests/GodotSim.Tests/GodotSim.Tests.csproj`

## Description

After aligning the Godot project with `Godot.NET.Sdk/4.6.3`, the Godot tests exposed that direct `TelemetryPoint` serialization produced PascalCase JSON:

```json
{"Timestamp":"...","Speed":45.5}
```

The backend telemetry contract expects camelCase:

```json
{"timestamp":"...","speed":45.5}
```

## Root Cause

`VehicleController.TelemetryPoint` had plain C# properties without `JsonPropertyName` attributes. `System.Text.Json` defaults to property names as declared unless a naming policy or attributes are configured.

`BackendClient` manually projected telemetry to camelCase payloads, but the struct itself did not encode the contract. That made direct serialization inconsistent and fragile.

## Fix

Added `System.Text.Json.Serialization.JsonPropertyName` attributes to all `TelemetryPoint` properties:

- `timestamp`
- `speed`
- `steeringAngle`
- `positionX`
- `positionY`
- `positionZ`
- `collision`

Godot tests now pass: **12/12**.

## Prevention

- Encode cross-service JSON contracts in DTOs/data structures with explicit attributes when they may be serialized directly.
- Keep contract tests for simulation-to-backend payloads.
