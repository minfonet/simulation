using Microsoft.EntityFrameworkCore;
using SimApi.Data;
using SimApi.DTOs;
using SimApi.Models;

namespace SimApi.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly ITelemetryStore _telemetryStore;

    public ReportService(AppDbContext db, ITelemetryStore telemetryStore)
    {
        _db = db;
        _telemetryStore = telemetryStore;
    }

    public async Task<SessionReportResponse> GetSessionReportAsync(Guid sessionId)
    {
        var session = await _db.SimulationSessions
            .Include(s => s.Instructor)
            .Include(s => s.Trainee)
            .Include(s => s.Evaluation)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session not found");

        var telemetryRecords = await _telemetryStore.GetTelemetryBySessionAsync(sessionId);
        var criticalEvents = await _telemetryStore.GetCriticalEventsBySessionAsync(sessionId);

        var speeds = telemetryRecords.Select(t => t.Speed).ToList();

        var report = new SessionReportResponse
        {
            SessionId = session.Id,
            Scenario = session.Scenario,
            Status = session.Status.ToString(),
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt,
            TotalTelemetryPoints = telemetryRecords.Count,
            AverageSpeed = speeds.Count > 0 ? Math.Round(speeds.Average(), 2) : null,
            MaxSpeed = speeds.Count > 0 ? Math.Round(speeds.Max(), 2) : null,
            MinSpeed = speeds.Count > 0 ? Math.Round(speeds.Min(), 2) : null,
            CollisionCount = telemetryRecords.Count(t => t.Collision),
            IsEvaluated = session.Evaluation != null,
            Score = session.Score,
            InstructorNotes = session.InstructorNotes,
            InstructorName = session.Instructor?.Name,
            TraineeName = session.Trainee?.Name,
            CriticalEvents = criticalEvents.Select(e => new CriticalEventSummary
            {
                Timestamp = e.Timestamp,
                EventType = e.EventType,
                Severity = e.Severity,
                Speed = ExtractSpeedFromMetadata(e.Metadata)
            }).ToList()
        };

        return report;
    }

    private static double? ExtractSpeedFromMetadata(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata)) return null;
        try
        {
            var json = System.Text.Json.JsonDocument.Parse(metadata);
            if (json.RootElement.TryGetProperty("speed", out var speed))
                return speed.GetDouble();
        }
        catch
        {
            // Ignore parse errors for malformed metadata
        }
        return null;
    }
}
