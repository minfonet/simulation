using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimApi.Data;
using SimApi.DTOs;
using SimApi.Models;

namespace SimApi.Controllers;

[ApiController]
[Route("api/trainee")]
[Authorize(Roles = "Trainee")]
public class TraineeController : ControllerBase
{
    private readonly AppDbContext _db;

    public TraineeController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("sessions")]
    public async Task<ActionResult<List<SessionResponse>>> GetSessions()
    {
        var sessions = await _db.SimulationSessions
            .Where(s => s.TraineeId == GetUserId())
            .Include(s => s.Instructor)
            .Select(s => new SessionResponse
            {
                Id = s.Id,
                OrganizationId = s.OrganizationId,
                InstructorId = s.InstructorId,
                InstructorName = s.Instructor!.Name,
                TraineeId = s.TraineeId,
                Scenario = s.Scenario,
                Status = s.Status.ToString(),
                Score = s.Score,
                InstructorNotes = s.InstructorNotes,
                CreatedAt = s.CreatedAt,
                CompletedAt = s.CompletedAt
            })
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("sessions/{id}")]
    public async Task<ActionResult<SessionResponse>> GetSession(Guid id)
    {
        var session = await _db.SimulationSessions
            .Include(s => s.Instructor)
            .FirstOrDefaultAsync(s => s.Id == id && s.TraineeId == GetUserId());

        if (session == null) return NotFound();

        return Ok(new SessionResponse
        {
            Id = session.Id,
            OrganizationId = session.OrganizationId,
            InstructorId = session.InstructorId,
            InstructorName = session.Instructor!.Name,
            TraineeId = session.TraineeId,
            Scenario = session.Scenario,
            Status = session.Status.ToString(),
            Score = session.Score,
            InstructorNotes = session.InstructorNotes,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt
        });
    }

    [HttpPost("sessions/{id}/start")]
    public async Task<ActionResult> StartSession(Guid id)
    {
        var session = await _db.SimulationSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.TraineeId == GetUserId());

        if (session == null) return NotFound();

        if (session.Status != SessionStatus.Pending)
            return BadRequest("Session is not in pending status");

        session.Status = SessionStatus.Active;
        await _db.SaveChangesAsync();

        return Ok(new { session.Id, session.Status.ToString() });
    }

    [HttpPost("sessions/{id}/finish")]
    public async Task<ActionResult> FinishSession(Guid id)
    {
        var session = await _db.SimulationSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.TraineeId == GetUserId());

        if (session == null) return NotFound();

        if (session.Status != SessionStatus.Active)
            return BadRequest("Session is not active");

        session.Status = SessionStatus.Completed;
        session.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { session.Id, session.Status.ToString() });
    }

    [HttpGet("evaluations")]
    public async Task<ActionResult<List<EvaluationResponse>>> GetEvaluations()
    {
        var evaluations = await _db.Evaluations
            .Where(e => e.Session!.TraineeId == GetUserId())
            .Include(e => e.Instructor)
            .Select(e => new EvaluationResponse
            {
                Id = e.Id,
                SessionId = e.SessionId,
                InstructorId = e.InstructorId,
                InstructorName = e.Instructor!.Name,
                Score = e.Score,
                Comments = e.Comments,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(evaluations);
    }
}
