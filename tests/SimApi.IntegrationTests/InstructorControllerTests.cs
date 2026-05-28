using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimApi.DTOs;
using SimApi.Models;
using Xunit;

namespace SimApi.IntegrationTests;

public class InstructorControllerTests : IntegrationTestBase
{
    public InstructorControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task CreateSession_Returns_Created()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var session = await DeserializeAsync<SessionResponse>(response);
        Assert.Equal("default", session.Scenario);
        Assert.Equal("Pending", session.Status);
        Assert.Equal(traineeAuth.UserId, session.TraineeId);
    }

    [Fact]
    public async Task GetSessions_Returns_InstructorSessions()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });

        var response = await Client.GetAsync("/api/instructor/sessions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sessions = await DeserializeAsync<List<SessionResponse>>(response);
        Assert.NotEmpty(sessions);
    }

    [Fact]
    public async Task GetSession_ById_Returns_Session()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        var createResponse = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });
        var created = await DeserializeAsync<SessionResponse>(createResponse);

        var response = await Client.GetAsync($"/api/instructor/sessions/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var session = await DeserializeAsync<SessionResponse>(response);
        Assert.Equal(created.Id, session.Id);
    }

    [Fact]
    public async Task EvaluateSession_Returns_NoContent()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        var createResponse = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResponse);

        // Trainee starts and finishes the session
        SetAuthToken(traineeAuth.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        // Instructor evaluates
        SetAuthToken(instructorAuth.AccessToken);
        var response = await Client.PostAsJsonAsync($"/api/instructor/sessions/{session.Id}/evaluate", new EvaluateSessionRequest
        {
            Score = 85,
            Comments = "Good driving skills"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetTrainees_Returns_TraineeList()
    {
        var (_, instructorAuth, _, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        var response = await Client.GetAsync("/api/instructor/trainees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var trainees = await DeserializeAsync<List<Dictionary<string, object>>>(response);
        Assert.NotEmpty(trainees);
    }

    [Fact]
    public async Task GetEvaluations_Returns_EvaluationList()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        var createResp = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResp);

        SetAuthToken(traineeAuth.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        SetAuthToken(instructorAuth.AccessToken);
        await Client.PostAsJsonAsync($"/api/instructor/sessions/{session.Id}/evaluate", new EvaluateSessionRequest
        {
            Score = 90,
            Comments = "Excellent"
        });

        var response = await Client.GetAsync("/api/instructor/evaluations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var evals = await DeserializeAsync<List<EvaluationResponse>>(response);
        Assert.NotEmpty(evals);
    }

    [Fact]
    public async Task Evaluate_NonCompleted_Session_Returns_BadRequest()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        var createResp = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResp);

        // Try to evaluate while still Pending (not Completed)
        var response = await Client.PostAsJsonAsync($"/api/instructor/sessions/{session.Id}/evaluate", new EvaluateSessionRequest
        {
            Score = 50,
            Comments = "Should fail"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ====================================================================
    // P1.2: GET /api/instructor/sessions/{id}/report endpoint
    // ====================================================================

    [Fact]
    public async Task GetSessionReport_Returns_Report_ForCompletedSession()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();

        // Instructor creates session
        SetAuthToken(instructorAuth.AccessToken);
        var createResp = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResp);

        // Trainee starts session and sends telemetry with known values
        SetAuthToken(traineeAuth.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));

        var now = DateTime.UtcNow;
        var telemetryResp = await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new() { Timestamp = now, Speed = 30, SteeringAngle = 0, PositionX = 0, PositionY = 0, PositionZ = 0, Collision = false },
                new() { Timestamp = now.AddSeconds(1), Speed = 50, SteeringAngle = 0.5, PositionX = 10, PositionY = 0, PositionZ = 5, Collision = false },
                new() { Timestamp = now.AddSeconds(2), Speed = 70, SteeringAngle = -0.3, PositionX = 20, PositionY = 0, PositionZ = 10, Collision = true }
            }
        });
        Assert.Equal(HttpStatusCode.OK, telemetryResp.StatusCode);

        // Trainee finishes session
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        // Instructor requests the report
        SetAuthToken(instructorAuth.AccessToken);
        var response = await Client.GetAsync($"/api/instructor/sessions/{session.Id}/report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await DeserializeAsync<SessionReportResponse>(response);

        // Verify report structure
        Assert.Equal(session.Id, report.SessionId);
        Assert.Equal("default", report.Scenario);
        Assert.Equal("Completed", report.Status);
        Assert.True(report.CompletedAt.HasValue);

        // Verify telemetry data accuracy
        Assert.Equal(3, report.TotalTelemetryPoints);
        Assert.Equal(50.0, report.AverageSpeed.GetValueOrDefault(), 2);
        Assert.Equal(70.0, report.MaxSpeed.GetValueOrDefault(), 2);
        Assert.Equal(30.0, report.MinSpeed.GetValueOrDefault(), 2);
        Assert.Equal(1, report.CollisionCount);

        // Verify critical events
        Assert.NotEmpty(report.CriticalEvents);
        Assert.Contains(report.CriticalEvents, e => e.EventType == "collision");
        var collisionEvent = report.CriticalEvents.First(e => e.EventType == "collision");
        Assert.Equal(70.0, collisionEvent.Speed.GetValueOrDefault(), 0); // Speed from the collision telemetry point

        // Verify evaluation info (not evaluated yet)
        Assert.False(report.IsEvaluated);
        Assert.Null(report.Score);
        Assert.Null(report.InstructorNotes);

        // Verify user names
        Assert.Equal("Instructor User", report.InstructorName);
        Assert.Equal("Trainee User", report.TraineeName);
    }

    [Fact]
    public async Task GetSessionReport_ActiveSession_Returns_BadRequest()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();

        SetAuthToken(instructorAuth.AccessToken);
        var createResp = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResp);

        // Trainee starts session but does NOT finish
        SetAuthToken(traineeAuth.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));

        // Instructor requests report while session is Active (not Completed)
        SetAuthToken(instructorAuth.AccessToken);
        var response = await Client.GetAsync($"/api/instructor/sessions/{session.Id}/report");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetSessionReport_NonexistentSession_Returns_NotFound()
    {
        var (_, instructorAuth, _, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        var response = await Client.GetAsync($"/api/instructor/sessions/{Guid.NewGuid()}/report");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSessionReport_OtherInstructorSession_Returns_NotFound()
    {
        var (_, instructorAuth, traineeAuth, orgId) = await SeedFullHierarchy();

        // First instructor creates and completes a session
        SetAuthToken(instructorAuth.AccessToken);
        var createResp = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResp);

        SetAuthToken(traineeAuth.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        // Register a second instructor in the same org
        var secondInstructorResp = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"inst2_{Guid.NewGuid():N}@test.com",
            Password = "Pass123!",
            Name = "Instructor Two",
            Role = "Instructor",
            OrganizationId = orgId
        });
        var secondInstructor = await DeserializeAsync<AuthResponse>(secondInstructorResp);

        // Second instructor tries to access first instructor's session report
        SetAuthToken(secondInstructor.AccessToken);
        var response = await Client.GetAsync($"/api/instructor/sessions/{session.Id}/report");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSessionReport_AfterEvaluation_IsEvaluated_True()
    {
        var (_, instructorAuth, traineeAuth, _) = await SeedFullHierarchy();

        // Full flow: create, start, send telemetry, finish
        SetAuthToken(instructorAuth.AccessToken);
        var createResp = await Client.PostAsJsonAsync("/api/instructor/sessions", new CreateSessionRequest
        {
            TraineeId = traineeAuth.UserId,
            Scenario = "default"
        });
        var session = await DeserializeAsync<SessionResponse>(createResp);

        SetAuthToken(traineeAuth.AccessToken);
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/start", new StringContent("{}"));

        var now = DateTime.UtcNow;
        await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = session.Id,
            Points = new List<TelemetryPoint>
            {
                new() { Timestamp = now, Speed = 50, SteeringAngle = 0, PositionX = 0, PositionY = 0, PositionZ = 0, Collision = false }
            }
        });
        await Client.PostAsync($"/api/trainee/sessions/{session.Id}/finish", new StringContent("{}"));

        // GET report before evaluation — IsEvaluated should be false
        SetAuthToken(instructorAuth.AccessToken);
        var preEvalResponse = await Client.GetAsync($"/api/instructor/sessions/{session.Id}/report");
        Assert.Equal(HttpStatusCode.OK, preEvalResponse.StatusCode);
        var preEvalReport = await DeserializeAsync<SessionReportResponse>(preEvalResponse);
        Assert.False(preEvalReport.IsEvaluated);
        Assert.Null(preEvalReport.Score);
        Assert.Null(preEvalReport.InstructorNotes);

        // Instructor evaluates the session
        var evalResponse = await Client.PostAsJsonAsync($"/api/instructor/sessions/{session.Id}/evaluate", new EvaluateSessionRequest
        {
            Score = 92,
            Comments = "Very good performance"
        });
        Assert.Equal(HttpStatusCode.NoContent, evalResponse.StatusCode);

        // GET report after evaluation — IsEvaluated should now be true
        var postEvalResponse = await Client.GetAsync($"/api/instructor/sessions/{session.Id}/report");
        Assert.Equal(HttpStatusCode.OK, postEvalResponse.StatusCode);
        var postEvalReport = await DeserializeAsync<SessionReportResponse>(postEvalResponse);

        Assert.True(postEvalReport.IsEvaluated);
        Assert.Equal(92.0, postEvalReport.Score);
        Assert.Equal("Very good performance", postEvalReport.InstructorNotes);
    }
}
