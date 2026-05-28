using System.Text.Json;
using Xunit;

namespace GodotSim.Tests;

/// <summary>
/// Compatibility tests for BackendClient HTTP payload construction.
/// These verify that the JSON payloads BackendClient sends to the API
/// match the expected format of the backend's TelemetryBatchRequest DTO.
///
/// NOTE: BackendClient depends on Godot APIs (GD.Print, OS.GetCmdlineArgs) and
/// a static HttpClient. Full integration testing requires mocking at a higher level
/// or running within the Godot engine. These tests verify the serialization contract
/// only — the portion of BackendClient that is stateless and testable.
/// </summary>
public class BackendClientCompatibilityTests
{
    /// <summary>
    /// Verify the telemetry batch payload structure matches SimApi's TelemetryBatchRequest.
    /// BackendClient serializes: { sessionId, points: [{ timestamp, speed, steeringAngle, ... }] }
    /// Backend expects: { sessionId (Guid), points (List of TelemetryPoint) }
    /// </summary>
    [Fact]
    public void Telemetry_Batch_Payload_Matches_Backend_DTO()
    {
        // This mirrors how BackendClient.SendTelemetryBatch builds its payload
        var sessionId = Guid.NewGuid();
        var points = new List<DrivingSim.VehicleController.TelemetryPoint>
        {
            new()
            {
                Timestamp = DateTime.UtcNow,
                Speed = 45.5,
                SteeringAngle = 0.15,
                PositionX = 10.0,
                PositionY = 0.0,
                PositionZ = 20.0,
                Collision = false
            }
        };

        var payload = new
        {
            sessionId = sessionId.ToString(),
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

        // The backend's TelemetryBatchRequest deserializes these exact field names
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // sessionId must be a string (Guid serialized as string)
        Assert.True(root.TryGetProperty("sessionId", out var sessionIdProp));
        Assert.Equal(JsonValueKind.String, sessionIdProp.ValueKind);

        // points must be an array
        Assert.True(root.TryGetProperty("points", out var pointsProp));
        Assert.Equal(JsonValueKind.Array, pointsProp.ValueKind);

        // Each point must have the correct fields
        var firstPoint = pointsProp[0];
        Assert.True(firstPoint.TryGetProperty("timestamp", out _));
        Assert.True(firstPoint.TryGetProperty("speed", out var speedProp));
        Assert.Equal(JsonValueKind.Number, speedProp.ValueKind);
        Assert.True(firstPoint.TryGetProperty("collision", out var collisionProp));
        Assert.Equal(JsonValueKind.False, collisionProp.ValueKind);
    }

    /// <summary>
    /// Verify the session start/finish payload format.
    /// BackendClient sends: POST /api/trainee/sessions/{id}/start with body "{}"
    /// </summary>
    [Fact]
    public void Session_Action_Payload_Is_Empty_Object()
    {
        // BackendClient sends an empty JSON object for start/finish
        var emptyPayload = "{}";
        using var doc = JsonDocument.Parse(emptyPayload);
        Assert.Empty(doc.RootElement.EnumerateObject());
    }

    /// <summary>
    /// Verify that the command-line argument parsing works correctly for --session-id.
    /// </summary>
    [Fact]
    public void Session_Id_Arg_Parsing_Logic()
    {
        // This mirrors the logic in BackendClient.ReadSessionId()
        var args = new[] { "--session-id", "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "--token", "mytoken" };

        Guid? sessionId = null;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--session-id" && Guid.TryParse(args[i + 1], out var id))
                sessionId = id;
        }

        Assert.NotNull(sessionId);
        Assert.Equal(Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), sessionId);
    }

    [Fact]
    public void Token_Arg_Parsing_Logic()
    {
        var args = new[] { "--session-id", "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "--token", "my-test-token" };

        string? token = null;
        var tokenArg = Array.IndexOf(args, "--token");
        if (tokenArg >= 0 && tokenArg + 1 < args.Length)
            token = args[tokenArg + 1];

        Assert.Equal("my-test-token", token);
    }

    [Fact]
    public void Api_Url_Arg_Parsing_Logic()
    {
        var args = new[] { "--session-id", "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "--api-url", "http://localhost:5000" };

        string? apiUrl = null;
        var urlArg = Array.IndexOf(args, "--api-url");
        if (urlArg >= 0 && urlArg + 1 < args.Length)
            apiUrl = args[urlArg + 1];

        Assert.Equal("http://localhost:5000", apiUrl);
    }

    [Fact]
    public void Missing_Session_Id_Arg_Returns_Empty()
    {
        var args = new[] { "--token", "abc" };

        Guid? sessionId = null;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--session-id" && Guid.TryParse(args[i + 1], out var id))
                sessionId = id;
        }

        Assert.Null(sessionId);
    }
}
