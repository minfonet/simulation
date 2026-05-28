using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SimApi.DTOs;
using Xunit;

namespace SimApi.IntegrationTests;

public class AuthSecurityTests : IntegrationTestBase
{
    public AuthSecurityTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Admin_Endpoint_Without_Auth_Returns_Unauthorized()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/admin/organizations");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Endpoint_With_Trainee_Role_Returns_Forbidden()
    {
        var (_, _, traineeAuth, _) = await SeedFullHierarchy();
        SetAuthToken(traineeAuth.AccessToken);

        var response = await Client.GetAsync("/api/admin/organizations");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Instructor_Endpoint_Without_Auth_Returns_Unauthorized()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/instructor/sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Instructor_Endpoint_With_Trainee_Role_Returns_Forbidden()
    {
        var (_, _, traineeAuth, _) = await SeedFullHierarchy();
        SetAuthToken(traineeAuth.AccessToken);

        var response = await Client.GetAsync("/api/instructor/sessions");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Trainee_Endpoint_Without_Auth_Returns_Unauthorized()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/trainee/sessions");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Trainee_Endpoint_With_Instructor_Role_Returns_Forbidden()
    {
        var (_, instructorAuth, _, _) = await SeedFullHierarchy();
        SetAuthToken(instructorAuth.AccessToken);

        var response = await Client.GetAsync("/api/trainee/sessions");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Telemetry_Endpoint_Without_Auth_Returns_Unauthorized()
    {
        ClearAuthToken();
        var response = await Client.PostAsJsonAsync("/api/telemetry", new TelemetryBatchRequest
        {
            SessionId = Guid.NewGuid(),
            Points = new List<TelemetryPoint>()
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Auth_Me_Without_Token_Returns_Unauthorized()
    {
        ClearAuthToken();
        var response = await Client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_Token_Returns_Unauthorized()
    {
        SetAuthToken("invalid.token.here");
        var response = await Client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
