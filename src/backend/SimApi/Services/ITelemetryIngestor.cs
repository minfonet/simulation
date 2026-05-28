using SimApi.DTOs;

namespace SimApi.Services;

public interface ITelemetryIngestor
{
    /// <summary>
    /// Ingests a telemetry batch for the given session.
    /// Validates session state, persists telemetry, and auto-generates critical events.
    /// Returns the result with telemetry count and critical event count.
    /// </summary>
    Task<IngestResult> IngestBatchAsync(Guid sessionId, List<TelemetryPoint> points);
}
