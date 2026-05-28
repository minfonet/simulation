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
            Scenario = "trainee-test"
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
}
