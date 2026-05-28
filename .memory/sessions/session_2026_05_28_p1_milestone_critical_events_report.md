# Session: P1 Milestone — Critical Events + Basic Final Report — 2026-05-28

## Summary

Completed the P1 milestone: CriticalEvent model with telemetry ingestion/storage boundaries, auto-generation of critical events from collisions, basic final report endpoints (Instructor + Trainee), and frontend report display on completed session pages.

## What was done

### P1.1 — Foundation: CriticalEvent model + ITelemetryIngestor/ITelemetryStore
- **Files created (6):**
  - `src/backend/SimApi/Models/CriticalEvent.cs` — New entity: SessionId, Timestamp, EventType (string), Severity (string), Metadata (JSON string)
  - `src/backend/SimApi/Services/ITelemetryStore.cs` — Storage boundary: SaveTelemetryBatchAsync, GetTelemetryBySessionAsync, SaveCriticalEventsAsync, GetCriticalEventsBySessionAsync, SessionExistsAsync
  - `src/backend/SimApi/Services/TelemetryStore.cs` — Implementation using AppDbContext
  - `src/backend/SimApi/Services/ITelemetryIngestor.cs` — Ingestion boundary: IngestBatchAsync validates session, persists telemetry, auto-generates critical events
  - `src/backend/SimApi/Services/TelemetryIngestor.cs` — Implementation: validates active session, maps telemetry points, generates CriticalEvent for each collision=true with JSON metadata (speed, steeringAngle, position)
  - `src/backend/SimApi/DTOs/CriticalEventDtos.cs` — CriticalEventResponse DTO for GET events endpoint
- **Files modified (3):**
  - `src/backend/SimApi/Data/AppDbContext.cs` — Added DbSet<CriticalEvent> with FK cascade + indexes
  - `src/backend/SimApi/Controllers/TelemetryController.cs` — Refactored to delegate to ITelemetryIngestor/ITelemetryStore; added GET /api/telemetry/session/{sessionId}/events endpoint; response now includes criticalEvents count
  - `src/backend/SimApi/Program.cs` — Registered ITelemetryStore and ITelemetryIngestor
- **Review fixes applied:** Added SessionExistsAsync to ITelemetryStore, fixed GET endpoints to check session existence first, added CriticalEvents nav property to SimulationSession, moved IngestResult to DTOs

### P1.2 — Basic Final Report
- **Files created (3):**
  - `src/backend/SimApi/DTOs/ReportDtos.cs` — SessionReportResponse: session info, telemetry summary (total, avg/max/min speed), collision count, critical events, evaluation data; CriticalEventSummary DTO
  - `src/backend/SimApi/Services/IReportService.cs` — GetSessionReportAsync(Guid sessionId)
  - `src/backend/SimApi/Services/ReportService.cs` — Implementation queries session (with includes), telemetry records via ITelemetryStore, critical events via ITelemetryStore; extracts speed from event metadata JSON
- **Files modified (3):**
  - `src/backend/SimApi/Controllers/InstructorController.cs` — Added GET /api/instructor/sessions/{id}/report with ownership check + completed status validation
  - `src/backend/SimApi/Controllers/TraineeController.cs` — Added GET /api/trainee/sessions/{id}/report with ownership check + completed status validation
  - `src/backend/SimApi/Program.cs` — Registered IReportService
- **Review fixes applied:** ReportService now uses ITelemetryStore, session status check added to both endpoints, AsNoTracking on ownership queries

### P1.2 — Integration Tests (QA)
- **Files modified:**
  - `tests/SimApi.IntegrationTests/InstructorControllerTests.cs` — Added 5 tests: report for completed session (data accuracy), active session returns 400, nonexistent session 404, other instructor 404, evaluation integration (isEvaluated flag)
  - `tests/SimApi.IntegrationTests/TraineeControllerTests.cs` — Added 4 tests: report for completed session, active session 400, nonexistent 404, other trainee 404

### P1.3 — Frontend Report Display
- **Files modified (2):**
  - `src/frontend/sim-web/app/instructor/sessions/[id]/page.tsx` — Added report card with telemetry summary grid (5 metrics), critical events list (type badge, timestamp, speed, severity coloring), evaluation info (score, evaluator name)
  - `src/frontend/sim-web/app/trainee/sessions/[id]/page.tsx` — Same report card, loading/error states

### P0 integration tests updated
- `tests/SimApi.IntegrationTests/TelemetryControllerTests.cs` — Added 7 tests for critical events: collision counting, event type/severity verification, metadata shape, empty events list, nonexistent session, timestamp ordering

## Files changed (P1 total)

| File | Status | Purpose |
|---|---|---|
| `src/backend/SimApi/Models/CriticalEvent.cs` | NEW | Critical event entity model |
| `src/backend/SimApi/Services/ITelemetryStore.cs` | NEW | Telemetry storage boundary |
| `src/backend/SimApi/Services/TelemetryStore.cs` | NEW | Telemetry store implementation |
| `src/backend/SimApi/Services/ITelemetryIngestor.cs` | NEW | Telemetry ingestion boundary |
| `src/backend/SimApi/Services/TelemetryIngestor.cs` | NEW | Telemetry ingestor with auto-generated events |
| `src/backend/SimApi/Services/IReportService.cs` | NEW | Report query service interface |
| `src/backend/SimApi/Services/ReportService.cs` | NEW | Report query service implementation |
| `src/backend/SimApi/DTOs/ReportDtos.cs` | NEW | Report DTOs |
| `src/backend/SimApi/DTOs/CriticalEventDtos.cs` | NEW | Critical event response DTO |
| `src/backend/SimApi/Data/AppDbContext.cs` | MODIFIED | Added CriticalEvent DbSet + config |
| `src/backend/SimApi/Models/SimulationSession.cs` | MODIFIED | Added CriticalEvents nav property |
| `src/backend/SimApi/Controllers/TelemetryController.cs` | MODIFIED | Refactored to use ITelemetryIngestor/ITelemetryStore |
| `src/backend/SimApi/Controllers/InstructorController.cs` | MODIFIED | Added report endpoint |
| `src/backend/SimApi/Controllers/TraineeController.cs` | MODIFIED | Added report endpoint |
| `src/backend/SimApi/DTOs/TelemetryDtos.cs` | MODIFIED | Added IngestResult DTO |
| `src/backend/SimApi/Program.cs` | MODIFIED | Registered new services |
| `tests/SimApi.IntegrationTests/TelemetryControllerTests.cs` | MODIFIED | Added critical events tests (7) |
| `tests/SimApi.IntegrationTests/InstructorControllerTests.cs` | MODIFIED | Added report tests (5) |
| `tests/SimApi.IntegrationTests/TraineeControllerTests.cs` | MODIFIED | Added report tests (4) |
| `src/frontend/sim-web/app/instructor/sessions/[id]/page.tsx` | MODIFIED | Added report display |
| `src/frontend/sim-web/app/trainee/sessions/[id]/page.tsx` | MODIFIED | Added report display |

## Bugs found

None. All implementations passed review and QA with no bugs.

## Decisions made

1. **EventType/Severity as strings**: Using strings instead of enums for extensibility. MVP values: "collision", "medium". Future event types can be added without changing the model.
2. **Metadata as JSON string**: CriticalEvent metadata is a JSON string for extensibility. The TelemetryIngestor serializes speed/steeringAngle/position; ReportService extracts speed with safe JSON parsing.
3. **Collision deduplication not implemented**: Each telemetry point with `Collision == true` generates one CriticalEvent. No grouping of consecutive collision frames (acceptable for MVP, documented limitation).
4. **Report endpoint requires Completed status**: Both report endpoints return 400 if session is not Completed, preventing confusing empty reports.

## Test results

| Suite | Result |
|---|---|
| Backend integration (xUnit) | **66/66 PASS** (+16 new: 7 critical events + 9 reports) |
| Frontend lib (Vitest) | **23/23 PASS** (unchanged) |
| Godot simulation (xUnit) | **12/12 PASS** (unchanged) |
| **Total** | **101/101 PASS** |

## QA coverage

- Critical events: collision auto-generation (1, 0, multiple), event type/severity, metadata shape, empty events, nonexistent session 404, timestamp ordering
- Report endpoints (Instructor): completed session report accuracy (speeds, counts, collisions, events), active session 400, nonexistent 404, other instructor 404, evaluation integration (isEvaluated flag)
- Report endpoints (Trainee): same coverage minus evaluation integration
- Architecture boundaries: ownership enforcement, status validation, data flow verification

## Known gaps (not blocking)

- Integration test for `GET /api/instructor/scenario-presets` endpoint (pre-existing)
- Frontend page component tests (pre-existing)
- Pre-existing TypeScript errors in `api.test.ts` (4 errors, documented)
- E2E smoke test requires live Docker Compose execution
- Collision deduplication not implemented (multiple telemetry frames during one collision each generate separate events)

## Next steps

1. Run E2E smoke test against live Docker Compose services
2. Add integration test for scenario-presets endpoint
3. Frontend page component tests
4. P2: Refactor session/evaluation controllers behind service boundaries
5. P2: HAL abstraction for control input in Godot
6. Consider collision deduplication for critical events
