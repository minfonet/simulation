using System.Text.Json;
using Xunit;

namespace GodotSim.Tests;

/// <summary>
/// Tests for the telemetry data structures used by VehicleController and BackendClient.
/// These tests verify the serialization format matches what the backend expects.
/// Note: VehicleController inherits from Godot.RigidBody3D and requires the full Godot
/// engine to instantiate. Pure logic tests are done here on the data structures.
/// </summary>
public class TelemetryDataTests
{
    /// <summary>
    /// The TelemetryPoint struct from VehicleController must serialize to JSON
    /// in the format expected by the backend TelemetryBatchRequest.Points array.
    /// </summary>
    [Fact]
    public void TelemetryPoint_Serializes_To_Expected_Format()
    {
        // Arrange
        var point = new DrivingSim.VehicleController.TelemetryPoint
        {
            Timestamp = new DateTime(2026, 5, 28, 12, 0, 0, DateTimeKind.Utc),
            Speed = 45.5,
            SteeringAngle = 0.15,
            PositionX = 10.0,
            PositionY = 0.0,
            PositionZ = 20.0,
            Collision = false
        };

        // Act
        var json = JsonSerializer.Serialize(point);

        // Assert
        Assert.Contains("\"timestamp\":", json);
        Assert.Contains("\"speed\":45.5", json);
        Assert.Contains("\"steeringAngle\":0.15", json);
        Assert.Contains("\"positionX\":10", json);
        Assert.Contains("\"positionY\":0", json);
        Assert.Contains("\"positionZ\":20", json);
        Assert.Contains("\"collision\":false", json);
    }

    [Fact]
    public void TelemetryPoint_Default_Values_Are_Zero()
    {
        var point = new DrivingSim.VehicleController.TelemetryPoint();

        Assert.Equal(default, point.Timestamp);
        Assert.Equal(0, point.Speed);
        Assert.Equal(0, point.SteeringAngle);
        Assert.Equal(0, point.PositionX);
        Assert.Equal(0, point.PositionY);
        Assert.Equal(0, point.PositionZ);
        Assert.False(point.Collision);
    }

    [Fact]
    public void TelemetryPoint_Can_Hold_Max_Values()
    {
        var point = new DrivingSim.VehicleController.TelemetryPoint
        {
            Timestamp = DateTime.MaxValue,
            Speed = double.MaxValue,
            SteeringAngle = double.MaxValue,
            PositionX = double.MaxValue,
            PositionY = double.MaxValue,
            PositionZ = double.MaxValue,
            Collision = true
        };

        Assert.Equal(DateTime.MaxValue, point.Timestamp);
        Assert.Equal(double.MaxValue, point.Speed);
        Assert.True(point.Collision);
    }

    [Fact]
    public void TelemetryPoint_Struct_Is_Copyable()
    {
        var original = new DrivingSim.VehicleController.TelemetryPoint
        {
            Speed = 100.0,
            Collision = true
        };

        var copy = original;
        copy.Speed = 200.0; // Modify copy

        Assert.Equal(100.0, original.Speed); // Original unchanged
        Assert.Equal(200.0, copy.Speed);
    }

    [Fact]
    public void Batch_Serialization_Matches_Backend_Contract()
    {
        // This simulates how BackendClient.SendTelemetryBatch constructs the payload
        var points = new List<DrivingSim.VehicleController.TelemetryPoint>
        {
            new()
            {
                Timestamp = DateTime.UtcNow,
                Speed = 50.0,
                SteeringAngle = 0.1,
                PositionX = 1.0,
                PositionY = 2.0,
                PositionZ = 3.0,
                Collision = false
            }
        };

        var payload = new
        {
            sessionId = Guid.NewGuid().ToString(),
            points = points.ConvertAll(p => new
            {
                timestamp = p.Timestamp.ToString("o"),
                speed = p.Speed,
                steeringAngle = p.SteeringAngle,
                positionX = p.PositionX,
                positionY = p.PositionY,
                positionZ = p.PositionZ,
                collision = p.Collision
            })
        };

        var json = JsonSerializer.Serialize(payload);

        // Verify the JSON structure matches what the backend expects
        Assert.Contains("\"sessionId\"", json);
        Assert.Contains("\"points\"", json);
        Assert.Contains("\"timestamp\"", json);
        Assert.Contains("\"speed\"", json);
        Assert.Contains("\"steeringAngle\"", json);
        Assert.Contains("\"positionX\"", json);
        Assert.Contains("\"positionY\"", json);
        Assert.Contains("\"positionZ\"", json);
        Assert.Contains("\"collision\"", json);
    }

    [Fact]
    public void Point_Collection_Can_Be_Manipulated()
    {
        var list = new List<DrivingSim.VehicleController.TelemetryPoint>();
        Assert.Empty(list);

        list.Add(new DrivingSim.VehicleController.TelemetryPoint { Speed = 10 });
        list.Add(new DrivingSim.VehicleController.TelemetryPoint { Speed = 20 });
        list.Add(new DrivingSim.VehicleController.TelemetryPoint { Speed = 30 });

        Assert.Equal(3, list.Count);
        Assert.Equal(20, list[1].Speed);

        list.RemoveAt(1);
        Assert.Equal(2, list.Count);
        Assert.Equal(30, list[1].Speed);
    }
}
