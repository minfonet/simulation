# Bug: Parallel test execution race condition on static DbContextConfigOverride

**Date discovered:** 2026-05-28
**Detected by:** QA agent during backend integration test execution

## Description

When running all integration tests, tests from different classes would intermittently fail because the static `Program.DbContextConfigOverride` property was being overwritten by one test class while another was still using it. Since xUnit runs test classes in parallel by default, multiple test classes would compete for the same static configuration.

## Root Cause

The original test infrastructure used a static `Action<IServiceCollection>? Program.DbContextConfigOverride` to swap the database provider from PostgreSQL to SQLite in-memory. Each test class set this override in its constructor and cleared it in `Dispose()`. When two test classes ran in parallel, the second class's constructor overwrote the override before the first class finished, causing the first class's tests to use the wrong database configuration.

## Initial Fix

The first mitigation was `xunit.runner.json` with `parallelizeTestCollections: false`, which forced sequential execution and avoided the race.

## Final Fix

The static override was removed entirely on 2026-05-28.

Current approach:
- `Program.cs` skips production Npgsql registration when the environment is `Testing`
- `IntegrationTestBase` uses `WebApplicationFactory.WithWebHostBuilder(...)`
- The test host sets `builder.UseEnvironment("Testing")`
- The test host injects SQLite in-memory via `ConfigureServices`
- `xunit.runner.json` was deleted because static mutable DbContext state is no longer needed

This allows test isolation without application-level static state.

## Prevention

- Avoid static mutable hooks for test-only service replacement
- Prefer environment-specific registration plus `WebApplicationFactory.ConfigureServices`
- Keep SQLite provider registration inside the test host, not production startup
