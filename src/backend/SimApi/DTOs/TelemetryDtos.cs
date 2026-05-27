using System.ComponentModel.DataAnnotations;

namespace SimApi.DTOs;

public class TelemetryBatchRequest
{
    [Required]
    public Guid SessionId { get; set; }

    [Required, MinLength(1)]
    public List<TelemetryPoint> Points { get; set; } = new();
}

public class TelemetryPoint
{
    public DateTime Timestamp { get; set; }
    public double Speed { get; set; }
    public double SteeringAngle { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double PositionZ { get; set; }
    public bool Collision { get; set; }
}

public class TelemetryResponse
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double Speed { get; set; }
    public double SteeringAngle { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double PositionZ { get; set; }
    public bool Collision { get; set; }
}
