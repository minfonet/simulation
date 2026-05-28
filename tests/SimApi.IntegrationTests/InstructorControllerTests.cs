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
}
