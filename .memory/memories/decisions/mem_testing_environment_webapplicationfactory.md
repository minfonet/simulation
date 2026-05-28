# Decision: Use Testing environment for WebApplicationFactory database replacement

**Date:** 2026-05-28

## Context

The initial backend integration test setup used a static `Program.DbContextConfigOverride` hook to replace PostgreSQL with SQLite in-memory. This worked when tests were sequential but created a race condition under parallel xUnit execution.

An attempted refactor using only `ConfigureServices` still loaded both Npgsql and SQLite providers because production startup registered Npgsql before test services were injected.

## Decision

Use `builder.UseEnvironment("Testing")` in `WebApplicationFactory` and make `Program.cs` skip production Npgsql registration when the environment is `Testing`.

The test host then injects SQLite in-memory through `ConfigureServices`.

## Rationale

- Removes static mutable state from production `Program`
- Avoids EF Core provider conflicts (`Npgsql` + `Sqlite` in one service provider)
- Keeps production startup simple
- Lets tests own their database provider configuration
- Preserves fast SQLite integration tests

## Implementation

`Program.cs`:

```csharp
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured")));
}
```

`IntegrationTestBase.cs`:

```csharp
Factory = factory.WithWebHostBuilder(builder =>
{
    builder.UseEnvironment("Testing");
    builder.ConfigureServices(services =>
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(_sqliteConnection));
    });
});
```

## Result

Backend integration tests pass: **50/50**.
