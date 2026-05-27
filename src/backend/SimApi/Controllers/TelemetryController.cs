using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimApi.Data;
using SimApi.DTOs;
using SimApi.Models;

namespace SimApi.Controllers;

[ApiController]
[Route("api/telemetry")]
[Authorize]
public class TelemetryController : ControllerBase
{
    private readonly AppDbContext _db;

    public TelemetryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult> IngestTelemetry(TelemetryBatchRequest request)
    {
        var session = await _db.SimulationSessions.FindAsync(request.SessionId);
        if (session == null) return NotFound("Session not found");

        if (session.Status != SessionStatus.Active)
            return BadRequest("Session is not active");

        var records = request.Points.Select(p => new TelemetryRecord
        {
            SessionId = request.SessionId,
            Timestamp = p.Timestamp,
            Speed = p.Speed,
            SteeringAngle = p.SteeringAngle,
            PositionX = p.PositionX,
            PositionY = p.PositionY,
            PositionZ = p.PositionZ,
            Collision = p.Collision
        }).ToList();

        _db.TelemetryRecords.AddRange(records);
        await _db.SaveChangesAsync();

        return Ok(new { ingested = records.Count });
    }

    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<List<TelemetryResponse>>> GetTelemetry(Guid sessionId)
    {
        var session = await _db.SimulationSessions.FindAsync(sessionId);
        if (session == null) return NotFound();

        var records = await _db.TelemetryRecords
            .Where(t => t.SessionId == sessionId)
            .OrderBy(t => t.Timestamp)
            .Select(t => new TelemetryResponse
            {
                Id = t.Id,
                Timestamp = t.Timestamp,
                Speed = t.Speed,
                SteeringAngle = t.SteeringAngle,
                PositionX = t.PositionX,
                PositionY = t.PositionY,
                PositionZ = t.PositionZ,
                Collision = t.Collision
            })
            .ToListAsync();

        return Ok(records);
    }
}
