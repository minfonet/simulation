# Bug: Convert.ToInt32 fails on JsonElement

**Date discovered:** 2026-05-28
**Detected by:** QA agent during backend integration test execution

## Description

The test `TelemetryIngestion_And_Retrieval` failed with `InvalidOperationException` when calling `Convert.ToInt32(result["ingested"])`. The exception message was: "The 'JsonElement' does not implement 'IConvertible'."

## Root Cause

When deserializing JSON as `Dictionary<string, object>`, numeric values are deserialized as `JsonElement` structs, not as primitive types like `int` or `long`. `Convert.ToInt32()` requires the input to implement `IConvertible`, which `JsonElement` does not.

## Fix

Changed from:
```csharp
Convert.ToInt32(result["ingested"])
```

To:
```csharp
((JsonElement)result["ingested"]).GetInt32()
```

## Prevention

When using `Dictionary<string, object>` with `System.Text.Json`, always cast to `JsonElement` first before accessing typed values. Use `.GetInt32()`, `.GetString()`, `.GetBoolean()`, etc.
