using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SimApi.DTOs;
using Xunit;

namespace SimApi.IntegrationTests;

public class AdminControllerTests : IntegrationTestBase
{
    public AdminControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetOrganizations_Returns_OrgList()
    {
        var auth = await BootstrapAdmin();
        SetAuthToken(auth.AccessToken);
        await CreateOrganization("Org A");
        await CreateOrganization("Org B");

        var response = await Client.GetAsync("/api/admin/organizations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var orgs = await DeserializeAsync<List<OrganizationResponse>>(response);
        Assert.True(orgs.Count >= 2);
    }

    [Fact]
    public async Task CreateOrganization_Returns_Created()
    {
        var auth = await BootstrapAdmin();
        SetAuthToken(auth.AccessToken);

        var response = await Client.PostAsJsonAsync("/api/admin/organizations", new CreateOrganizationRequest
        {
            Name = "New Test Org"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var org = await DeserializeAsync<OrganizationResponse>(response);
        Assert.Equal("New Test Org", org.Name);
        Assert.Equal(0, org.UserCount);
    }

    [Fact]
    public async Task UpdateOrganization_Returns_NoContent()
    {
        var auth = await BootstrapAdmin();
        SetAuthToken(auth.AccessToken);
        var orgId = await CreateOrganization("Original Name");

        var response = await Client.PutAsJsonAsync($"/api/admin/organizations/{orgId}", new CreateOrganizationRequest
        {
            Name = "Updated Name"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrganization_Returns_NoContent()
    {
        var auth = await BootstrapAdmin();
        SetAuthToken(auth.AccessToken);
        var orgId = await CreateOrganization("To Delete");

        var response = await Client.DeleteAsync($"/api/admin/organizations/{orgId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationUsers_Returns_UserList()
    {
        var auth = await BootstrapAdmin();
        SetAuthToken(auth.AccessToken);
        var orgId = await CreateOrganization("User Test Org");

        await Client.PostAsJsonAsync($"/api/admin/organizations/{orgId}/users", new RegisterRequest
        {
            Email = "orguser@test.com",
            Password = "password123",
            Name = "Org User",
            Role = "Trainee"
        });

        var response = await Client.GetAsync($"/api/admin/organizations/{orgId}/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await DeserializeAsync<List<Dictionary<string, object>>>(response);
        Assert.Contains(users, u => u["email"]!.ToString() == "orguser@test.com");
    }

    [Fact]
    public async Task InviteUser_Returns_Ok()
    {
        var auth = await BootstrapAdmin();
        SetAuthToken(auth.AccessToken);
        var orgId = await CreateOrganization("Invite Test Org");

        var response = await Client.PostAsJsonAsync($"/api/admin/organizations/{orgId}/users", new RegisterRequest
        {
            Email = "invited@test.com",
            Password = "password123",
            Name = "Invited User",
            Role = "Instructor"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await DeserializeAsync<Dictionary<string, object>>(response);
        Assert.Equal("invited@test.com", user["email"]!.ToString());
        Assert.Equal("Instructor", user["role"]!.ToString());
    }

    [Fact]
    public async Task InviteUser_Duplicate_Email_Returns_Conflict()
    {
        var auth = await BootstrapAdmin();
        SetAuthToken(auth.AccessToken);
        var orgId = await CreateOrganization("Conflict Test Org");

        await Client.PostAsJsonAsync($"/api/admin/organizations/{orgId}/users", new RegisterRequest
        {
            Email = "dupe@test.com",
            Password = "password123",
            Name = "First User",
            Role = "Trainee"
        });

        var response = await Client.PostAsJsonAsync($"/api/admin/organizations/{orgId}/users", new RegisterRequest
        {
            Email = "dupe@test.com",
            Password = "password456",
            Name = "Second User",
            Role = "Trainee"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
