using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SimApi.Models;

public class SimulationSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [JsonIgnore]
    public Organization? Organization { get; set; }

    public Guid InstructorId { get; set; }

    [ForeignKey(nameof(InstructorId))]
    [JsonIgnore]
    public User? Instructor { get; set; }

    public Guid TraineeId { get; set; }

    [ForeignKey(nameof(TraineeId))]
    [JsonIgnore]
    public User? Trainee { get; set; }

    [MaxLength(200)]
    public string Scenario { get; set; } = "default";

    public SessionStatus Status { get; set; } = SessionStatus.Pending;

    public double? Score { get; set; }

    public string? InstructorNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    [JsonIgnore]
    public ICollection<TelemetryRecord> TelemetryRecords { get; set; } = new List<TelemetryRecord>();

    [JsonIgnore]
    public Evaluation? Evaluation { get; set; }

    [JsonIgnore]
    public ICollection<CriticalEvent> CriticalEvents { get; set; } = new List<CriticalEvent>();
}
