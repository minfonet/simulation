using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SimApi.Models;

public class TelemetryRecord
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    [JsonIgnore]
    public SimulationSession? Session { get; set; }

    public DateTime Timestamp { get; set; }

    public double Speed { get; set; }

    public double SteeringAngle { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double PositionZ { get; set; }

    public bool Collision { get; set; }

    public string? RawData { get; set; }
}
