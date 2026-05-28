using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimApi.DTOs;
using Xunit;

namespace SimApi.IntegrationTests;

public class TraineeControllerTests : IntegrationTestBase
{
    public TraineeControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    /// <summary>
    /// Seeds a session setup: creates org + instructor + trainee + session (Pending).
    /// </summary>
    private async Task<(AuthResponse instructor, AuthResponse trainee, SessionResponse session)> SeedSession()
    {
        var (_, instructor, trainee, _) = await SeedFullHierarchy();

        SetAuthToken(instructor.AccessToken);
        var createResp = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = trainee.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResp);
        ClearAuthToken();

        return (instructor, trainee, session);
    }

    [Fact]
    public async Task GetSessions_Returns_TraineeSessions()
    {
        var (_, trainee, session) = await SeedSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.GetAsync("/api/trainee/sessions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sessions = await DeserializeAsync<List<SessionResponse>>(response);
        Assert.NotEmpty(sessions);
        Assert.Contains(sessions, s => s.Id == session.Id);
    }

    [Fact]
    public async Task GetSession_ById_Returns_Session()
    {
        var (_, trainee, session) = await SeedSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.GetAsync($"/api/trainee/sessions/{session.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeAsync<SessionResponse>(response);
        Assert.Equal(session.Id, result.Id);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task StartSession_Changes_Status_To_Active()
    {
        var (_, trainee, session) = await SeedSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeAsync<Dictionary<string, object>>(response);
        var statusVal = (System.Text.Json.JsonElement)result["status"];
        Assert.Equal("Active", statusVal.GetString());
    }

    [Fact]
    public async Task StartSession_Twice_Returns_BadRequest()
    {
        var (_, trainee, session) = await SeedSession();
        SetAuthToken(trainee.AccessToken);

        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));
        var response = await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FinishSession_Changes_Status_To_Completed()
    {
        var (_, trainee, session) = await SeedSession();
        SetAuthToken(trainee.AccessToken);

        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));
        var response = await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await DeserializeAsync<Dictionary<string, object>>(response);
        var statusVal = (System.Text.Json.JsonElement)result["status"];
        Assert.Equal("Completed", statusVal.GetString());
    }

    [Fact]
    public async Task FinishSession_Before_Start_Returns_BadRequest()
    {
        var (_, trainee, session) = await SeedSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetEvaluations_Returns_Evaluations()
    {
        var (instructor, trainee, session) = await SeedSession();

        // Complete the full flow
        SetAuthToken(trainee.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        // Instructor evaluates
        SetAuthToken(instructor.AccessToken);
        await Client.PostAsJsonAsync($"/api/instructor/sessions/{session.Id}/evaluate", new EvaluateSessionRequest
        {
            Score = 95,
            Comments = "Great job!"
        });

        // Trainee gets evaluations
        SetAuthToken(trainee.AccessToken);
        var response = await Client.GetAsync("/api/trainee/evaluations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var evals = await DeserializeAsync<List<EvaluationResponse>>(response);
        Assert.NotEmpty(evals);
    }

    // ====================================================================
    // P1.2: GET /api/trainee/sessions/{id}/report endpoint
    // ====================================================================

    [Fact]
    public async Task GetSessionReport_Returns_Report_ForCompletedSession()
    {
        var (instructor, trainee, session) = await SeedSession();

        // Trainee starts, sends telemetry, finishes
        SetAuthToken(trainee.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));

        var now = DateTime.UtcNow;
        await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new() { Timestamp = now, Speed = 30, SteeringAngle = 0, PositionX = 0, PositionY = 0, PositionZ = 0, Collision = false },
                new() { Timestamp = now.AddSeconds(1), Speed = 50, SteeringAngle = 0.5, PositionX = 10, PositionY = 0, PositionZ = 5, Collision = false },
                new() { Timestamp = now.AddSeconds(2), Speed = 70, SteeringAngle = -0.3, PositionX = 20, PositionY = 0, PositionZ = 10, Collision = true }
            }
        });
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        // Trainee requests the report
        var response = await Client.GetAsync($"/api/trainee/sessions/{session.Id}/report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await DeserializeAsync<SessionReportResponse>(response);

        // Verify report structure
        Assert.Equal(session.Id, report.SessionId);
        Assert.Equal("Completed", report.Status);

        // Verify telemetry data accuracy
        Assert.Equal(3, report.TotalTelemetryPoints);
        Assert.Equal(50.0, report.AverageSpeed.GetValueOrDefault(), 2);
        Assert.Equal(70.0, report.MaxSpeed.GetValueOrDefault(), 2);
        Assert.Equal(30.0, report.MinSpeed.GetValueOrDefault(), 2);
        Assert.Equal(1, report.CollisionCount);

        // Verify critical events
        Assert.NotEmpty(report.CriticalEvents);
        Assert.Contains(report.CriticalEvents, e => e.EventType == "collision");

        // Verify evaluation info (not evaluated yet)
        Assert.False(report.IsEvaluated);

        // Verify user names
        Assert.Equal("Instructor User", report.InstructorName);
        Assert.Equal("Trainee User", report.TraineeName);
    }

    [Fact]
    public async Task GetSessionReport_ActiveSession_Returns_BadRequest()
    {
        var (_, trainee, session) = await SeedSession();

        // Trainee starts session but does NOT finish
        SetAuthToken(trainee.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));

        // Trainee requests report while session is Active
        var response = await Client.GetAsync($"/api/trainee/sessions/{session.Id}/report");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSessionReport_NonexistentSession_Returns_NotFound()
    {
        var (_, trainee, _) = await SeedSession();
        SetAuthToken(trainee.AccessToken);

        var response = await Client.GetAsync($"/api/trainee/sessions/{Guid.NewGuid()}/report");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSessionReport_OtherTraineeSession_Returns_NotFound()
    {
        var (instructor, trainee, session) = await SeedSession();
        var orgId = instructor.OrganizationId;

        // First trainee completes the session
        SetAuthToken(trainee.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));
        ClearAuthToken();

        // Register a second trainee in the same org
        var secondTraineeResp = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"trainee2_{Guid.NewGuid():N}@test.com",
            Password = "Pass123!",
            Name = "Trainee Two",
            Role = "Trainee",
            OrganizationId = orgId
        });
        var secondTrainee = await DeserializeAsync<AuthResponse>(secondTraineeResp);

        // Second trainee tries to access first trainee's session report
        SetAuthToken(secondTrainee.AccessToken);
        var response = await Client.GetAsync($"/api/trainee/sessions/{session.Id}/report");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
