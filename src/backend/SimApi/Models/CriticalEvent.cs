using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SimApi.Models;

public class CriticalEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    [JsonIgnore]
    public SimulationSession? Session { get; set; }

    public DateTime Timestamp { get; set; }

    [MaxLength(50)]
    public string EventType { get; set; } = "collision";

    [MaxLength(20)]
    public string Severity { get; set; } = "medium";

    /// <summary>
    /// Optional JSON metadata for extensible event details.
    /// </summary>
    public string? Metadata { get; set; }
}
