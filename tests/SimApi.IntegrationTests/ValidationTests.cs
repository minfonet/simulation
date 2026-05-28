using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimApi.DTOs;
using Xunit;

namespace SimApi.IntegrationTests;

public class ValidationTests : IntegrationTestBase
{
    public ValidationTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Register_With_Empty_Email_Returns_BadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "",
            password = "password123",
            name = "Test",
            role = "Trainee",
            organizationId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_With_Invalid_Email_Returns_BadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "not-an-email",
            password = "password123",
            name = "Test",
            role = "Trainee",
            organizationId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_With_Short_Password_Returns_BadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "valid@test.com",
            password = "123",
            name = "Test",
            role = "Trainee",
            organizationId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_With_Invalid_Role_Returns_BadRequest()
    {
        // Need a real org for this test
        var adminAuth = await BootstrapAdmin();
        SetAuthToken(adminAuth.AccessToken);
        var orgId = await CreateOrganization("Validation Test Org");
        ClearAuthToken();

        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "invalidrole@test.com",
            Password = "password123",
            Name = "Test",
            Role = "SuperAdmin",
            OrganizationId = orgId
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_With_Nonexistent_Org_Returns_BadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "noorganization@test.com",
            Password = "password123",
            Name = "Test",
            Role = "Trainee",
            OrganizationId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrganization_With_Empty_Name_Returns_BadRequest()
    {
        var adminAuth = await BootstrapAdmin();
        SetAuthToken(adminAuth.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/admin/organizations", new
        {
            name = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EvaluateSession_With_OutOfRange_Score_Returns_BadRequest()
    {
        var adminAuth = await BootstrapAdmin();
        SetAuthToken(adminAuth.AccessToken);
        var orgId = await CreateOrganization("Score Test Org");

        // Create an instructor
        var instructorResp = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"inst_{Guid.NewGuid():N}@test.com",
            Password = "Pass123!",
            Name = "Instructor",
            Role = "Instructor",
            OrganizationId = orgId
        });
        var instructor = await DeserializeAsync<AuthResponse>(instructorResp);
        SetAuthToken(instructor.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/instructor/sessions/{00000000-0000-0000-0000-000000000000}/evaluate", new
        {
            score = 150,
            comments = "Should fail validation"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
