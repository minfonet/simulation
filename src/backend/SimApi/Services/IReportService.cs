using SimApi.DTOs;

namespace SimApi.Services;

public interface IReportService
{
    /// <summary>
    /// Generates a session report for a completed session.
    /// The report includes session info, telemetry summary, critical events, and evaluation (if any).
    /// </summary>
    Task<SessionReportResponse> GetSessionReportAsync(Guid sessionId);
}
