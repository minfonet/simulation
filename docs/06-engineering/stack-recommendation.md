# Stack Recommendation

## MVP Stack

| Component | Technology | Version | Status |
|---|---|---|---|
| Simulation | Godot 4 + C# | 4.x | ✅ Implemented |
| Backend API | ASP.NET Core | 8+ | ✅ Implemented |
| Frontend | Next.js + TypeScript | 16 | ✅ Implemented |
| UI Framework | Tailwind CSS | 4 | ✅ Implemented |
| Database | PostgreSQL | 16 | ✅ Implemented |
| ORM | Entity Framework Core | 8+ | ✅ Implemented |
| Auth | JWT (access + refresh tokens) | — | ✅ Implemented |
| Telemetry | REST batch polling (vanilla PostgreSQL) | — | ✅ Implemented |
| Infrastructure | Docker Compose | — | ✅ Implemented |

## Post-MVP (planned additions)

| Technology | Purpose | When |
|---|---|---|
| TimescaleDB | Time-series optimized telemetry storage | Post-MVP |
| SignalR + WebSockets | Real-time telemetry streaming | Post-MVP |
| AI Evaluation System | Automated behavioral scoring | Post-MVP |
| Hardware SDK | Steering wheels, pedals, custom controllers | Post-MVP |

## Why This Stack

Benefits:
- scalable architecture
- strong realtime support (post-MVP)
- enterprise-ready backend
- flexible hardware integration (post-MVP)
- low operational cost
- strong modularity
- future migration path to Unreal Engine if needed
