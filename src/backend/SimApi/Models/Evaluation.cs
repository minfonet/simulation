using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SimApi.Models;

public class Evaluation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    [JsonIgnore]
    public SimulationSession? Session { get; set; }

    public Guid InstructorId { get; set; }

    [ForeignKey(nameof(InstructorId))]
    [JsonIgnore]
    public User? Instructor { get; set; }

    public double Score { get; set; }

    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
