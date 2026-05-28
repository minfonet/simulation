using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimApi.Data;
using System.Text.Json;
using SimApi.DTOs;
using SimApi.Models;
using Xunit;

namespace SimApi.IntegrationTests;

public class TelemetryControllerTests : IntegrationTestBase
{
    public TelemetryControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    /// <summary>
    /// Seeds: org + instructor + trainee + session in Active status.
    /// </summary>
    private async Task<(AuthResponse instructor, AuthResponse trainee, SessionResponse session)> SeedActiveSession()
    {
        var (_, instructor, trainee, _) = await SeedFullHierarchy();

        SetAuthToken(instructor.AccessToken);
        var createResp = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = trainee.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResp);

        SetAuthToken(trainee.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));
        ClearAuthToken();

        return (instructor, trainee, session);
    }

    [Fact]
    public async Task IngestTelemetry_Returns_Ingested_Count()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = DateTime.UtcNow,
                    Speed = 50.0,
                    SteeringAngle = 0.1,
                    PositionX = 10.0,
                    PositionY = 0.0,
                    PositionZ = 20.0,
                    Collision = false
                },
                new()
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(1),
                    Speed = 55.0,
                    SteeringAngle = -0.2,
                    PositionX = 15.0,
                    PositionY = 0.0,
                    PositionZ = 25.0,
                    Collision = true
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeAsync<Dictionary<string, object>>(response);

        // Verify backward compatibility: ingested field
        var ingested = (JsonElement)result["ingested"];
        Assert.Equal(2, ingested.GetInt32());

        // Verify new criticalEvents field (1 of 2 points has collision=true)
        var criticalEvents = (JsonElement)result["criticalEvents"];
        Assert.Equal(1, criticalEvents.GetInt32());
    }

    [Fact]
    public async Task GetTelemetry_Returns_Records()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = DateTime.UtcNow,
                    Speed = 60.0,
                    SteeringAngle = 0.0,
                    PositionX = 0.0,
                    PositionY = 0.0,
                    PositionZ = 0.0,
                    Collision = false
                }
            }
        });

        var response = await Client.GetAsync($"/api/telemetry/session/{session.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var records = await DeserializeAsync<List<TelemetryResponse>>(response);
        Assert.NotEmpty(records);
        Assert.Equal(60.0, records[0].Speed);
    }

    [Fact]
    public async Task IngestTelemetry_To_NonActive_Session_Returns_BadRequest()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        // Directly update the session to Completed
        var db = Factory.Services.CreateScope()
            .ServiceProvider.GetRequiredService<AppDbContext>();
        var dbSession = await db.SimulationSessions.FindAsync(session.Id);
        if (dbSession != null)
        {
            dbSession.Status = SessionStatus.Completed;
            await db.SaveChangesAsync();
        }

        var response = await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = DateTime.UtcNow,
                    Speed = 10,
                    SteeringAngle = 0,
                    PositionX = 0,
                    PositionY = 0,
                    PositionZ = 0,
                    Collision = false
                }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTelemetry_Nonexistent_Session_Returns_NotFound()
    {
        var (_, trainee, _) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.GetAsync($"/api/telemetry/session/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task IngestTelemetry_Without_Auth_Returns_Unauthorized()
    {
        ClearAuthToken();
        var response = await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = Guid.NewGuid(),
            Points = new List<TelemetryPoint>()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ====================================================================
    // P1.1: Critical Events auto-generation from collisions
    // ====================================================================

    [Fact]
    public async Task IngestTelemetry_WithCollision_ReturnsCriticalEventCount()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = DateTime.UtcNow,
                    Speed = 50.0,
                    SteeringAngle = 0.0,
                    PositionX = 10.0,
                    PositionY = 0.0,
                    PositionZ = 20.0,
                    Collision = true
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeAsync<Dictionary<string, object>>(response);
        var criticalEvents = (JsonElement)result["criticalEvents"];
        Assert.Equal(1, criticalEvents.GetInt32());
    }

    [Fact]
    public async Task IngestTelemetry_NoCollision_ReturnsZeroCriticalEvents()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = DateTime.UtcNow,
                    Speed = 50.0,
                    SteeringAngle = 0.0,
                    PositionX = 10.0,
                    PositionY = 0.0,
                    PositionZ = 20.0,
                    Collision = false
                },
                new()
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(1),
                    Speed = 55.0,
                    SteeringAngle = -0.1,
                    PositionX = 15.0,
                    PositionY = 0.0,
                    PositionZ = 25.0,
                    Collision = false
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeAsync<Dictionary<string, object>>(response);
        var criticalEvents = (JsonElement)result["criticalEvents"];
        Assert.Equal(0, criticalEvents.GetInt32());
    }

    [Fact]
    public async Task IngestTelemetry_MultipleCollisions_ReturnsMatchingCriticalEventCount()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = DateTime.UtcNow,
                    Speed = 50.0,
                    SteeringAngle = 0.0,
                    PositionX = 10.0,
                    PositionY = 0.0,
                    PositionZ = 20.0,
                    Collision = true
                },
                new()
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(1),
                    Speed = 55.0,
                    SteeringAngle = -0.1,
                    PositionX = 15.0,
                    PositionY = 0.0,
                    PositionZ = 25.0,
                    Collision = false
                },
                new()
                {
                    Timestamp = DateTime.UtcNow.AddSeconds(2),
                    Speed = 0.0,
                    SteeringAngle = 0.5,
                    PositionX = 5.0,
                    PositionY = 0.0,
                    PositionZ = 30.0,
                    Collision = true
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeAsync<Dictionary<string, object>>(response);
        var criticalEvents = (JsonElement)result["criticalEvents"];
        Assert.Equal(2, criticalEvents.GetInt32());
    }

    // ====================================================================
    // P1.1: GET /api/telemetry/session/{sessionId}/events endpoint
    // ====================================================================

    [Fact]
    public async Task GetCriticalEvents_ReturnsEvents_AfterCollisionIngestion()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        var now = DateTime.UtcNow;
        await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = now,
                    Speed = 50.0,
                    SteeringAngle = 0.2,
                    PositionX = 10.0,
                    PositionY = 0.0,
                    PositionZ = 20.0,
                    Collision = true
                }
            }
        });

        var response = await Client.GetAsync($"/api/telemetry/session/{session.Id}/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await DeserializeAsync<List<CriticalEventResponse>>(response);

        Assert.NotEmpty(events);
        var ev = events[0];
        Assert.Equal(session.Id, ev.SessionId);
        Assert.Equal("collision", ev.EventType);
        Assert.Equal("medium", ev.Severity);
        Assert.NotNull(ev.Metadata);

        // Verify metadata contains expected fields (lowercase keys from ingestor)
        var metadata = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ev.Metadata);
        Assert.NotNull(metadata);
        Assert.True(metadata.ContainsKey("speed"));
        Assert.True(metadata.ContainsKey("steeringAngle"));
        Assert.True(metadata.ContainsKey("position"));

        // Verify position sub-object
        var position = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(metadata["position"].GetRawText());
        Assert.NotNull(position);
        Assert.True(position.ContainsKey("x"));
        Assert.True(position.ContainsKey("y"));
        Assert.True(position.ContainsKey("z"));
    }

    [Fact]
    public async Task GetCriticalEvents_ReturnsEmptyList_WhenNoCollisions()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = DateTime.UtcNow,
                    Speed = 50.0,
                    SteeringAngle = 0.0,
                    PositionX = 10.0,
                    PositionY = 0.0,
                    PositionZ = 20.0,
                    Collision = false
                }
            }
        });

        var response = await Client.GetAsync($"/api/telemetry/session/{session.Id}/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await DeserializeAsync<List<CriticalEventResponse>>(response);
        Assert.Empty(events);
    }

    [Fact]
    public async Task GetCriticalEvents_NonexistentSession_ReturnsNotFound()
    {
        var (_, trainee, _) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.GetAsync($"/api/telemetry/session/{Guid.NewGuid()}/events");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCriticalEvents_ReturnsInTimestampOrder()
    {
        var (_, trainee, session) = await SeedActiveSession();
        SetAuthToken(trainee.AccessToken);

        var now = DateTime.UtcNow;
        // Send batches in reverse timestamp order to verify sort
        await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = now.AddSeconds(2),
                    Speed = 30.0,
                    SteeringAngle = 0.3,
                    PositionX = 5.0,
                    PositionY = 0.0,
                    PositionZ = 10.0,
                    Collision = true
                }
            }
        });

        await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new()
                {
                    Timestamp = now.AddSeconds(1),
                    Speed = 50.0,
                    SteeringAngle = 0.1,
                    PositionX = 15.0,
                    PositionY = 0.0,
                    PositionZ = 25.0,
                    Collision = true
                }
            }
        });

        var response = await Client.GetAsync($"/api/telemetry/session/{session.Id}/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await DeserializeAsync<List<CriticalEventResponse>>(response);
        Assert.Equal(2, events.Count);

        // Verify ascending timestamp order
        Assert.True(events[0].Timestamp <= events[1].Timestamp,
            "Events should be ordered by Timestamp ascending");
    }
}
