using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimApi.Data;
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
        var ingested = (System.Text.Json.JsonElement)result["ingested"];
        Assert.Equal(2, ingested.GetInt32());
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
}
