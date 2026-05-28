using Microsoft.EntityFrameworkCore;
using SimApi.Data;
using SimApi.Models;

namespace SimApi.Services;

public class TelemetryStore : ITelemetryStore
{
    private readonly AppDbContext _db;

    public TelemetryStore(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> SessionExistsAsync(Guid sessionId)
    {
        return await _db.SimulationSessions.AnyAsync(s => s.Id == sessionId);
    }

    public async Task SaveTelemetryBatchAsync(List<TelemetryRecord> records)
    {
        _db.TelemetryRecords.AddRange(records);
        await _db.SaveChangesAsync();
    }

    public async Task<List<TelemetryRecord>> GetTelemetryBySessionAsync(Guid sessionId)
    {
        return await _db.TelemetryRecords
            .Where(t => t.SessionId == sessionId)
            .OrderBy(t => t.Timestamp)
            .ToListAsync();
    }

    public async Task SaveCriticalEventsAsync(List<CriticalEvent> events)
    {
        _db.CriticalEvents.AddRange(events);
        await _db.SaveChangesAsync();
    }

    public async Task<List<CriticalEvent>> GetCriticalEventsBySessionAsync(Guid sessionId)
    {
        return await _db.CriticalEvents
            .Where(e => e.SessionId == sessionId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync();
    }
}
