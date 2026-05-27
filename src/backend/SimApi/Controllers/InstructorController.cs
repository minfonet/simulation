using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimApi.Data;
using SimApi.DTOs;
using SimApi.Models;

namespace SimApi.Controllers;

[ApiController]
[Route("api/instructor")]
[Authorize(Roles = "Instructor")]
public class InstructorController : ControllerBase
{
    private readonly AppDbContext _db;

    public InstructorController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private Guid GetOrganizationId() =>
        Guid.Parse(User.FindFirstValue("organizationId")!);

    [HttpGet("sessions")]
    public async Task<ActionResult<List<SessionResponse>>> GetSessions()
    {
        var sessions = await _db.SimulationSessions
            .Where(s => s.InstructorId == GetUserId())
            .Include(s => s.Trainee)
            .Select(s => new SessionResponse
            {
                Id = s.Id,
                OrganizationId = s.OrganizationId,
                InstructorId = s.InstructorId,
                InstructorName = "",
                TraineeId = s.TraineeId,
                TraineeName = s.Trainee!.Name,
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

    [HttpPost("sessions")]
    public async Task<ActionResult<SessionResponse>> CreateSession(CreateSessionRequest request)
    {
        var trainee = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.TraineeId && u.Role == UserRole.Trainee);

        if (trainee == null)
            return BadRequest("Trainee not found");

        var session = new SimulationSession
        {
            OrganizationId = GetOrganizationId(),
            InstructorId = GetUserId(),
            TraineeId = request.TraineeId,
            Scenario = request.Scenario
        };

        _db.SimulationSessions.Add(session);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSessions), new SessionResponse
        {
            Id = session.Id,
            OrganizationId = session.OrganizationId,
            InstructorId = session.InstructorId,
            TraineeId = session.TraineeId,
            TraineeName = trainee.Name,
            Scenario = session.Scenario,
            Status = session.Status.ToString(),
            CreatedAt = session.CreatedAt
        });
    }

    [HttpGet("sessions/{id}")]
    public async Task<ActionResult<SessionResponse>> GetSession(Guid id)
    {
        var session = await _db.SimulationSessions
            .Include(s => s.Trainee)
            .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == GetUserId());

        if (session == null) return NotFound();

        return Ok(new SessionResponse
        {
            Id = session.Id,
            OrganizationId = session.OrganizationId,
            InstructorId = session.InstructorId,
            TraineeId = session.TraineeId,
            TraineeName = session.Trainee!.Name,
            Scenario = session.Scenario,
            Status = session.Status.ToString(),
            Score = session.Score,
            InstructorNotes = session.InstructorNotes,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt
        });
    }

    [HttpPost("sessions/{id}/evaluate")]
    public async Task<ActionResult> EvaluateSession(Guid id, EvaluateSessionRequest request)
    {
        var session = await _db.SimulationSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == GetUserId());

        if (session == null) return NotFound();

        if (session.Status != SessionStatus.Completed)
            return BadRequest("Session must be completed before evaluation");

        var evaluation = new Evaluation
        {
            SessionId = id,
            InstructorId = GetUserId(),
            Score = request.Score,
            Comments = request.Comments
        };

        session.Score = request.Score;
        session.InstructorNotes = request.Comments;

        _db.Evaluations.Add(evaluation);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("trainees")]
    public async Task<ActionResult> GetTrainees()
    {
        var trainees = await _db.Users
            .Where(u => u.OrganizationId == GetOrganizationId() && u.Role == UserRole.Trainee)
            .Select(u => new { u.Id, u.Email, u.Name, u.CreatedAt })
            .ToListAsync();

        return Ok(trainees);
    }

    [HttpGet("evaluations")]
    public async Task<ActionResult<List<EvaluationResponse>>> GetEvaluations()
    {
        var evaluations = await _db.Evaluations
            .Where(e => e.InstructorId == GetUserId())
            .Include(e => e.Session)
            .Select(e => new EvaluationResponse
            {
                Id = e.Id,
                SessionId = e.SessionId,
                InstructorId = e.InstructorId,
                Score = e.Score,
                Comments = e.Comments,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(evaluations);
    }
}
