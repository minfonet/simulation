---
id: mem_ef_core_ensurecreated
type: decision
tags:
  - backend
  - database
  - ef-core
---

# Context

We needed to decide how to create the database schema in the MVP. Options: EF Core migrations, EnsureCreated(), or manual SQL scripts.

# Decision

Use `EnsureCreated()` in `Program.cs` when the application starts. No migrations in the MVP.

# Why

- EnsureCreated() creates all tables automatically from the DbContext
- Eliminates the need to run `dotnet ef migrations add` manually
- For an MVP with 5 tables and no complex schemas, migrations are overengineering
- When the model matures, it will migrate to formal migrations

# Consequences

- No schema change history (not applicable until there is production data)
- To add a column, the DB must be deleted or updated manually
- Swapping to migrations will be required before any real deployment
