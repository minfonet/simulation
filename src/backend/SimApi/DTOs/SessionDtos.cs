using System.ComponentModel.DataAnnotations;

namespace SimApi.DTOs;

public class CreateSessionRequest
{
    [Required]
    public Guid TraineeId { get; set; }

    public string Scenario { get; set; } = "default";
}

public class EvaluateSessionRequest
{
    [Required, Range(0, 100)]
    public double Score { get; set; }

    public string? Comments { get; set; }
}

public class ScenarioPresetResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GodotScenePath { get; set; } = string.Empty;
}

public class SessionResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public Guid TraineeId { get; set; }
    public string TraineeName { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double? Score { get; set; }
    public string? InstructorNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
