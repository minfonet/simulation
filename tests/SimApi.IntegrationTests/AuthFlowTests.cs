using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimApi.DTOs;
using Xunit;

namespace SimApi.IntegrationTests;

public class AuthFlowTests : IntegrationTestBase
{
    public AuthFlowTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Register_With_Valid_Data_Returns_Tokens()
    {
        // Arrange: bootstrap admin + org
        var adminAuth = await BootstrapAdmin();
        SetAuthToken(adminAuth.AccessToken);
        var orgId = await CreateOrganization("Test Org");
        ClearAuthToken();

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "trainee@test.com",
            Password = "password123",
            Name = "Test Trainee",
            Role = "Trainee",
            OrganizationId = orgId
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await DeserializeAsync<AuthResponse>(response);
        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
        Assert.Equal("trainee@test.com", auth.Email);
        Assert.Equal("Trainee", auth.Role);
    }

    [Fact]
    public async Task Register_Duplicate_Email_Returns_Conflict()
    {
        var adminAuth = await BootstrapAdmin();
        SetAuthToken(adminAuth.AccessToken);
        var orgId = await CreateOrganization("Test Org");

        await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "password123",
            Name = "First User",
            Role = "Trainee",
            OrganizationId = orgId
        });

        ClearAuthToken();

        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "duplicate@test.com",
            Password = "password456",
            Name = "Second User",
            Role = "Trainee",
            OrganizationId = orgId
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_With_Valid_Credentials_Returns_Tokens()
    {
        var adminAuth = await BootstrapAdmin();
        SetAuthToken(adminAuth.AccessToken);
        var orgId = await CreateOrganization("Test Org");
        await RegisterTestUser("loginuser@test.com", "password123", "Login User", "Trainee", orgId);
        ClearAuthToken();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "loginuser@test.com",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = await DeserializeAsync<AuthResponse>(response);
        Assert.NotEmpty(auth.AccessToken);
        Assert.Equal("loginuser@test.com", auth.Email);
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Returns_Unauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "wrongpassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_With_Valid_Token_Returns_New_Tokens()
    {
        var adminAuth = await BootstrapAdmin();
        SetAuthToken(adminAuth.AccessToken);
        var orgId = await CreateOrganization("Test Org");
        await RegisterTestUser("refreshuser@test.com", "password123", "Refresh User", "Trainee", orgId);
        ClearAuthToken();

        var loginAuth = await LoginUser("refreshuser@test.com", "password123");

        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest
        {
            RefreshToken = loginAuth.RefreshToken
        });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshedAuth = await DeserializeAsync<AuthResponse>(refreshResponse);
        Assert.NotEmpty(refreshedAuth.AccessToken);
        Assert.NotEqual(loginAuth.AccessToken, refreshedAuth.AccessToken);
    }

    [Fact]
    public async Task Me_Endpoint_Returns_Current_User()
    {
        var adminAuth = await BootstrapAdmin();
        SetAuthToken(adminAuth.AccessToken);
        var orgId = await CreateOrganization("Test Org");
        await RegisterTestUser("meuser@test.com", "password123", "Me User", "Trainee", orgId);
        ClearAuthToken();

        var loginAuth = await LoginUser("meuser@test.com", "password123");
        SetAuthToken(loginAuth.AccessToken);

        var response = await Client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await DeserializeAsync<Dictionary<string, object>>(response);
        Assert.Equal("meuser@test.com", me["email"]!.ToString());
        Assert.Equal("Me User", me["name"]!.ToString());
    }

    [Fact]
    public async Task Me_Endpoint_Without_Token_Returns_Unauthorized()
    {
        var response = await Client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
