# Bug: System.Text.Json camelCase serialization mismatch

**Date discovered:** 2026-05-28
**Detected by:** QA agent during backend integration test execution

## Description

Tests were failing with `KeyNotFoundException` when accessing JSON response properties like `result["Status"]`. ASP.NET Core's default JSON serializer (`System.Text.Json`) serializes properties using camelCase, so `Status` becomes `status` in the actual JSON response.

## Root Cause

The test code used `Dictionary<string, object>` with property names matching the C# PascalCase convention (e.g., `result["Status"]`), but `System.Text.Json` by default serializes to camelCase (`result["status"]`).

The `IntegrationTestBase` already had `PropertyNameCaseInsensitive = true` for `ReadFromJsonAsync<T>()`, but when deserializing as `Dictionary<string, object>`, the keys preserve the original casing from the JSON, so case-insensitive matching doesn't apply.

## Fix

Changed from indexer access:
```csharp
result["Status"]
```

To proper JSON element access:
```csharp
((JsonElement)result["status"]).GetString()
```

## Prevention

When using `DeserializeAsync<Dictionary<string, object>>()`, always use camelCase property names matching the actual JSON output, or use strongly-typed DTOs with `ReadFromJsonAsync<T>()` instead of dictionaries.
