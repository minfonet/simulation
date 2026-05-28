---
id: mem_architecture_three_layer
type: architecture
tags:
  - architecture
  - system
  - layers
---

# System Architecture (MVP)

## Layers

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   FRONTEND      │     │   BACKEND       │     │   SIMULATION    │
│   Next.js 16    │────>│   ASP.NET Core  │<────│   Godot 4 + C#  │
│   Port 3000     │     │   Port 8080     │     │   Standalone    │
│   (Browser)     │     │   (Server)      │     │   (Desktop)     │
└─────────────────┘     └───────┬─────────┘     └─────────────────┘
                                │
                        ┌───────▼─────────┐
                        │   DATABASE      │
                        │   PostgreSQL 16 │
                        │   Port 5432     │
                        └─────────────────┘
```

## Communication

| From | To | Protocol | Details |
|---|---|---|---|
| Browser (React) | Backend | REST/JSON | JWT in Authorization header |
| Godot | Backend | REST/JSON | JWT via --token CLI arg |
| Backend | PostgreSQL | EF Core (Npgsql) | EnsureCreated() on startup |

## Auth flow

1. User logs in → backend returns JWT access + refresh tokens
2. Tokens stored in localStorage (frontend) or --token arg (Godot)
3. Every API call includes `Authorization: Bearer <jwt>`
4. Backend validates JWT on every request via `[Authorize]` attribute
5. No token refresh on frontend yet (MVP gap)

## Key constraints

- CORS allows any origin (AllAnyOrigin) — acceptable for MVP
- No SignalR — telemetry uses REST polling
- No TimescaleDB — vanilla PostgreSQL
- No hardware integration

# References

docs/02-architecture/architecture.md
docs/01-product/mdvp.md
