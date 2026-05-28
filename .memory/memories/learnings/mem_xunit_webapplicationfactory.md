# Learning: Configuring xUnit + WebApplicationFactory + SQLite + JWT for integration tests

**Date:** 2026-05-28

## Overview

Integration tests for ASP.NET Core APIs using xUnit with `WebApplicationFactory<Program>` provide in-memory HTTP testing without needing a running server. This project uses SQLite in-memory to replace PostgreSQL during tests.

## Current Pattern

### 1. Production startup skips DB registration in Testing

`Program.cs` registers Npgsql for normal runtime, but skips it when `ASPNETCORE_ENVIRONMENT` is `Testing`:

```csharp
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured")));
}
```

This prevents EF Core from loading both Npgsql and SQLite providers in the same test service provider.

### 2. Test host injects SQLite per test instance

```csharp
public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    private readonly SqliteConnection _sqliteConnection;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
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
}
```

### 3. Avoid static service overrides

Do not use static mutable hooks such as `Program.DbContextConfigOverride`. They create race conditions when xUnit runs test classes in parallel.

The previous static override was replaced by the Testing environment + `WebApplicationFactory.ConfigureServices` pattern.

### 4. Auth token management

```csharp
protected void SetAuthToken(string token)
{
    Client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
}

protected void ClearAuthToken()
{
    Client.DefaultRequestHeaders.Authorization = null;
}
```

### 5. JSON deserialization

```csharp
private readonly JsonSerializerOptions _jsonOptions = new()
{
    PropertyNameCaseInsensitive = true
};
```

When deserializing to `Dictionary<string, object>`, numeric values are `JsonElement` structs, not primitive types. Use:
- `((JsonElement)dict["key"]).GetString()` for strings
- `((JsonElement)dict["key"]).GetInt32()` for integers
- `((JsonElement)dict["key"]).GetBoolean()` for booleans

Never use `Convert.ToInt32(dict["key"])`; `JsonElement` does not implement `IConvertible`.

### 6. Test data seeding

```csharp
protected async Task<(AuthResponse admin, AuthResponse instructor, AuthResponse trainee, Guid orgId)> SeedFullHierarchy()
{
    // 1. Create org directly via DbContext
    // 2. Register admin via API
    // 3. Register instructor via API
    // 4. Register trainee via API
    // Returns all auth responses + org ID
}
```

### 7. JWT token uniqueness

If refresh tokens are identical to original tokens, add a `jti` claim:

```csharp
new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
```

## Common Pitfalls

| Pitfall | Solution |
|---|---|
| Both Npgsql and SQLite providers are registered | Skip production DbContext registration in `Testing` and inject SQLite from tests |
| Tests fail in parallel due static state | Avoid static test hooks; use `WithWebHostBuilder` per test instance |
| `KeyNotFoundException` on JSON | Use camelCase keys or typed DTOs with case-insensitive serializer options |
| `InvalidOperationException` on `Convert.ToInt32()` | Cast to `JsonElement` and use `.GetInt32()` |
| Refresh token equals original | Add `jti` claim with `Guid.NewGuid()` |
| Auth token lost after helper methods | Re-set `SetAuthToken()` explicitly before the action under test |
| `Assert.Equal(double, double?, int)` doesn't compile | Use `.GetValueOrDefault()` on nullable double before precision comparison: `Assert.Equal(50.0, report.AverageSpeed.GetValueOrDefault(), 2)` |
| `Assert.NotNull()` on non-nullable value types (e.g., `DateTime`) triggers warning | Use `.HasValue` on nullable types or remove the assertion for non-nullable value types |
