---
id: mem_p1_1_critical_events_test_coverage
type: learning
tags:
  - testing
  - telemetry
  - critical-events
  - integration-tests
  - p1.1
---

# P1.1 Critical Events & Telemetry Boundaries — Test Coverage

## What was tested

P1.1 added the CriticalEvent entity, ITelemetryIngestor/ITelemetryStore boundaries, and a new `GET /api/telemetry/session/{sessionId}/events` endpoint. Tests cover:

### Critical Events auto-generation from collisions
- `IngestTelemetry_WithCollision_ReturnsCriticalEventCount` — 1 collision point → `criticalEvents` count = 1
- `IngestTelemetry_NoCollision_ReturnsZeroCriticalEvents` — 0 collision points → `criticalEvents` count = 0
- `IngestTelemetry_MultipleCollisions_ReturnsMatchingCriticalEventCount` — 2 collision points out of 3 → `criticalEvents` count = 2

### GET /api/telemetry/session/{sessionId}/events endpoint
- `GetCriticalEvents_ReturnsEvents_AfterCollisionIngestion` — verifies EventType="collision", Severity="medium", Metadata contains speed/steeringAngle/position keys
- `GetCriticalEvents_ReturnsEmptyList_WhenNoCollisions` — session with telemetry but no collisions returns empty list
- `GetCriticalEvents_NonexistentSession_ReturnsNotFound` — 404 for non-existent session ID
- `GetCriticalEvents_ReturnsInTimestampOrder` — events sorted ascending by Timestamp even when ingested out of order

### Backward compatibility
- `IngestTelemetry_Returns_Ingested_Count` — existing test updated to verify both `ingested` and `criticalEvents` fields in the POST response

### Session existence check
- Existing `GetTelemetry_Nonexistent_Session_Returns_NotFound` confirms GET telemetry 404
- New `GetCriticalEvents_NonexistentSession_ReturnsNotFound` confirms GET events 404

## Test results

**Suite**: `TelemetryControllerTests` (12 tests — 5 existing + 7 new)
**Full backend suite**: 57 tests — all pass, 0 failures, 0 skipped

## Architecture boundaries verified

- `TelemetryController` is thin: delegates to `ITelemetryIngestor` and `ITelemetryStore`
- `ITelemetryIngestor` handles validation (session existence, active status), persistence delegation, and critical event auto-generation
- `ITelemetryStore` provides persistence/query boundaries for both telemetry records and critical events
- No EF Core or business logic leaked into the controller

## Notable: no bugs discovered

All P1.1 implementations were correct on first test. Key implementation details confirmed:

1. Critical events are generated for each point with `Collision = true` during `IngestBatchAsync`
2. Metadata JSON uses lowercase keys: `speed`, `steeringAngle`, `position.x/y/z`
3. Events endpoint returns events ordered by `Timestamp` ascending
4. Session existence check returns 404 (not 200 with empty list) for both telemetry and events endpoints
