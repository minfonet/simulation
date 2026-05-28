using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimApi.DTOs;
using SimApi.Services;

namespace SimApi.Controllers;

[ApiController]
[Route("api/telemetry")]
[Authorize]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryIngestor _ingestor;
    private readonly ITelemetryStore _store;

    public TelemetryController(ITelemetryIngestor ingestor, ITelemetryStore store)
    {
        _ingestor = ingestor;
        _store = store;
    }

    [HttpPost]
    public async Task<ActionResult> IngestTelemetry(TelemetryBatchRequest request)
    {
        try
        {
            var result = await _ingestor.IngestBatchAsync(request.SessionId, request.Points);
            return Ok(new { ingested = result.TelemetryCount, criticalEvents = result.CriticalEventCount });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<List<TelemetryResponse>>> GetTelemetry(Guid sessionId)
    {
        if (!await _store.SessionExistsAsync(sessionId))
            return NotFound("Session not found");

        var records = await _store.GetTelemetryBySessionAsync(sessionId);

        var response = records.Select(r => new TelemetryResponse
        {
            Id = r.Id,
            Timestamp = r.Timestamp,
            Speed = r.Speed,
            SteeringAngle = r.SteeringAngle,
            PositionX = r.PositionX,
            PositionY = r.PositionY,
            PositionZ = r.PositionZ,
            Collision = r.Collision
        }).ToList();

        return Ok(response);
    }

    [HttpGet("session/{sessionId}/events")]
    public async Task<ActionResult<List<CriticalEventResponse>>> GetCriticalEvents(Guid sessionId)
    {
        if (!await _store.SessionExistsAsync(sessionId))
            return NotFound("Session not found");

        var events = await _store.GetCriticalEventsBySessionAsync(sessionId);

        var response = events.Select(e => new CriticalEventResponse
        {
            Id = e.Id,
            SessionId = e.SessionId,
            Timestamp = e.Timestamp,
            EventType = e.EventType,
            Severity = e.Severity,
            Metadata = e.Metadata
        }).ToList();

        return Ok(response);
    }
}
