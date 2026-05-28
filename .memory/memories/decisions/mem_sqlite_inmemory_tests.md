# Decision: Use SQLite in-memory for integration tests

**Date:** 2026-05-28

## Context

We needed an integration testing strategy for the ASP.NET Core 10 backend API. The Docker Compose database is PostgreSQL 16 via EF Core Npgsql. Options considered:

1. **Testcontainers (PostgreSQL in Docker)** — most realistic, but requires Docker running
2. **SQLite in-memory** — fast, no dependencies, but has SQL dialect differences from PostgreSQL
3. **PostgreSQL directly** — requires a running PostgreSQL instance
4. **EF Core InMemory provider** — simplest, but doesn't support relational features (transactions, etc.)

## Decision

Use **SQLite in-memory with shared cache** for integration tests.

## Rationale

- Fastest execution with no external dependencies
- Shared cache allows all DbContext instances within a test class to share the same database
- `Microsoft.Data.Sqlite` is lightweight and packages well
- SQL dialect differences are minimal for the CRUD operations we test
- WebApplicationFactory integration is straightforward via a `Testing` environment plus per-test `ConfigureServices`
- If we need true PostgreSQL testing, we can add Testcontainers later

## Trade-offs

| Aspect | Impact |
|---|---|
| **Speed** | ✅ Very fast — in-memory, no network or Docker |
| **Portability** | ✅ No Docker, no external DB needed |
| **Realism** | ⚠️ SQLite vs PostgreSQL differences (e.g., `NOW()` vs `datetime('now')`) |
| **Parallelism** | ✅ No application-level static override is used, so provider setup does not require forced sequential execution |
| **EF Core features** | ⚠️ SQLite may not support all PostgreSQL-specific EF Core features |

## Implementation

```csharp
// In test base class constructor:
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
```

## Future

If we encounter PostgreSQL-specific bugs that SQLite doesn't catch, add Testcontainers as an additional test layer (not replacement).
