using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimApi.Data;
using SimApi.DTOs;
using Xunit;

namespace SimApi.IntegrationTests;

/// <summary>
/// Base class for integration tests. Uses SQLite in-memory with shared cache
/// so all DbContext instances within a test class share the same database.
///
/// The WebApplicationFactory<Program> is configured per test instance to use
/// SQLite instead of PostgreSQL without mutating application-level static state.
/// </summary>
public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    private readonly SqliteConnection _sqliteConnection;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private bool _disposed;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        // Create a SQLite in-memory connection and open it so it stays alive
        _sqliteConnection = new SqliteConnection("DataSource=:memory:");
        _sqliteConnection.Open();

        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_sqliteConnection));
            });
        });

        Client = Factory.CreateClient();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Client.Dispose();
        Factory.Dispose();
        _sqliteConnection?.Close();
        _sqliteConnection?.Dispose();
    }

    protected async Task<AuthResponse> RegisterTestUser(string email, string password, string name, string role, Guid orgId)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
            Name = name,
            Role = role,
            OrganizationId = orgId
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
        Assert.NotNull(result);
        return result!;
    }

    protected async Task<AuthResponse> LoginUser(string email, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
        Assert.NotNull(result);
        return result!;
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthToken()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task<Guid> CreateOrganization(string name)
    {
        var response = await Client.PostAsJsonAsync("/api/admin/organizations", new CreateOrganizationRequest
        {
            Name = name
        });
        response.EnsureSuccessStatusCode();
        var org = await response.Content.ReadFromJsonAsync<OrganizationResponse>(_jsonOptions);
        Assert.NotNull(org);
        return org!.Id;
    }

    protected async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        Assert.NotNull(content);
        return content!;
    }

    /// <summary>
    /// Seeds a bootstrap organization and registers an admin user.
    /// Used by tests that need admin-level access for organization CRUD.
    /// </summary>
    protected async Task<AuthResponse> BootstrapAdmin()
    {
        var db = Factory.Services.CreateScope()
            .ServiceProvider.GetRequiredService<SimApi.Data.AppDbContext>();
        var orgId = Guid.NewGuid();
        db.Organizations.Add(new SimApi.Models.Organization
        {
            Id = orgId,
            Name = "Bootstrap Org",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var response = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"admin_{Guid.NewGuid():N}@bootstrap.com",
            Password = "AdminPass123!",
            Name = "Bootstrap Admin",
            Role = "Admin",
            OrganizationId = orgId
        });
        response.EnsureSuccessStatusCode();
        return await DeserializeAsync<AuthResponse>(response);
    }

    /// <summary>
    /// Seeds all three user roles (Admin, Instructor, Trainee) in a fresh org.
    /// Used by tests that need the full user hierarchy.
    /// </summary>
    protected async Task<(AuthResponse admin, AuthResponse instructor, AuthResponse trainee, Guid orgId)> SeedFullHierarchy()
    {
        var db = Factory.Services.CreateScope()
            .ServiceProvider.GetRequiredService<SimApi.Data.AppDbContext>();
        var orgId = Guid.NewGuid();
        db.Organizations.Add(new SimApi.Models.Organization
        {
            Id = orgId,
            Name = "Test Org",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var adminResp = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"admin_{Guid.NewGuid():N}@test.com",
            Password = "Pass123!",
            Name = "Admin User",
            Role = "Admin",
            OrganizationId = orgId
        });
        var admin = await DeserializeAsync<AuthResponse>(adminResp);

        var instructorResp = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"inst_{Guid.NewGuid():N}@test.com",
            Password = "Pass123!",
            Name = "Instructor User",
            Role = "Instructor",
            OrganizationId = orgId
        });
        var instructor = await DeserializeAsync<AuthResponse>(instructorResp);

        var traineeResp = await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"trainee_{Guid.NewGuid():N}@test.com",
            Password = "Pass123!",
            Name = "Trainee User",
            Role = "Trainee",
            OrganizationId = orgId
        });
        var trainee = await DeserializeAsync<AuthResponse>(traineeResp);

        return (admin, instructor, trainee, orgId);
    }
}
