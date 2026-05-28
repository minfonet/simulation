using Microsoft.EntityFrameworkCore;
using SimApi.Data;
using SimApi.DTOs;
using SimApi.Models;

namespace SimApi.Services;

public class TelemetryIngestor : ITelemetryIngestor
{
    private readonly AppDbContext _db;
    private readonly ITelemetryStore _store;

    public TelemetryIngestor(AppDbContext db, ITelemetryStore store)
    {
        _db = db;
        _store = store;
    }

    public async Task<IngestResult> IngestBatchAsync(Guid sessionId, List<TelemetryPoint> points)
    {
        var session = await _db.SimulationSessions.FindAsync(sessionId);
        if (session == null)
            throw new KeyNotFoundException("Session not found");

        if (session.Status != SessionStatus.Active)
            throw new InvalidOperationException("Session is not active");

        var records = points.Select(p => new TelemetryRecord
        {
            SessionId = sessionId,
            Timestamp = p.Timestamp,
            Speed = p.Speed,
            SteeringAngle = p.SteeringAngle,
            PositionX = p.PositionX,
            PositionY = p.PositionY,
            PositionZ = p.PositionZ,
            Collision = p.Collision
        }).ToList();

        await _store.SaveTelemetryBatchAsync(records);

        // Auto-generate critical events for collisions
        var criticalEvents = points
            .Where(p => p.Collision)
            .Select(p => new CriticalEvent
            {
                SessionId = sessionId,
                Timestamp = p.Timestamp,
                EventType = "collision",
                Severity = "medium",
                Metadata = System.Text.Json.JsonSerializer.Serialize(new
                {
                    speed = p.Speed,
                    steeringAngle = p.SteeringAngle,
                    position = new { x = p.PositionX, y = p.PositionY, z = p.PositionZ }
                })
            }).ToList();

        if (criticalEvents.Count > 0)
        {
            await _store.SaveCriticalEventsAsync(criticalEvents);
        }

        return new IngestResult
        {
            TelemetryCount = records.Count,
            CriticalEventCount = criticalEvents.Count
        };
    }
}
