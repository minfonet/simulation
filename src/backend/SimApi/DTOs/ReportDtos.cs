namespace SimApi.DTOs;

public class SessionReportResponse
{
    public Guid SessionId { get; set; }
    public string Scenario { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Session summary
    public int TotalTelemetryPoints { get; set; }
    public double? AverageSpeed { get; set; }
    public double? MaxSpeed { get; set; }
    public double? MinSpeed { get; set; }
    public int CollisionCount { get; set; }

    // Evaluation info (if evaluated)
    public bool IsEvaluated { get; set; }
    public double? Score { get; set; }
    public string? InstructorNotes { get; set; }
    public string? InstructorName { get; set; }
    public string? TraineeName { get; set; }

    // Critical events
    public List<CriticalEventSummary> CriticalEvents { get; set; } = new();
}

public class CriticalEventSummary
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public double? Speed { get; set; }
}
