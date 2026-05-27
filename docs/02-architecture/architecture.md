# Architecture

## MVP Stack (current)

| Layer | Technology | Version |
|---|---|---|
| Simulation Engine | Godot + C# | 4.x |
| Backend API | ASP.NET Core | 8+ |
| Frontend | Next.js + TypeScript | 16 |
| UI Framework | Tailwind CSS | 4 |
| Database | PostgreSQL | 16 |
| ORM | Entity Framework Core | 8+ |
| Authentication | JWT (access + refresh tokens) | — |
| Telemetry | REST batch polling | — |
| Infrastructure | Docker Compose | — |

## Post-MVP (planned)

| Technology | Replaces |
|---|---|
| TimescaleDB | Vanilla PostgreSQL for telemetry |
| SignalR + WebSockets | REST polling for realtime |
| Hardware abstraction layer | Manual input only |
| AI evaluation system | Instructor-only evaluation |

## Architecture Goals
- modularity
- scalability
- hardware abstraction
- realtime telemetry (post-MVP)
- persistent operational data
