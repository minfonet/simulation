using SimApi.Models;

namespace SimApi.Services;

public interface ITelemetryStore
{
    Task<bool> SessionExistsAsync(Guid sessionId);
    Task SaveTelemetryBatchAsync(List<TelemetryRecord> records);
    Task<List<TelemetryRecord>> GetTelemetryBySessionAsync(Guid sessionId);
    Task SaveCriticalEventsAsync(List<CriticalEvent> events);
    Task<List<CriticalEvent>> GetCriticalEventsBySessionAsync(Guid sessionId);
}
