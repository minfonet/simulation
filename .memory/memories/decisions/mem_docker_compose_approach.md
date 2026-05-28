---
id: mem_docker_compose_approach
type: decision
tags:
  - docker
  - infrastructure
  - mvp
---

# Context

We needed to define how to orchestrate MVP services (backend, database, future frontend) for local development and eventual deployment.

# Decision

Use Docker Compose with a single `docker/docker-compose.yml` file. Backend with multi-stage Dockerfile, Postgres 16 with health check, persistent volumes. Frontend will be added in MVP phase 2.

# Why

- Docker Compose is the standard for local microservice development
- Multi-stage build keeps the production image small (runtime only, no SDK)
- Health check in Postgres avoids race conditions when starting backend
- A single docker-compose.yml file is sufficient for MVP — avoids overengineering with Docker Swarm/K8s

# Consequences

- The team needs Docker Desktop on Windows
- The database is persisted in a named volume (pgdata)
- Adding frontend only requires adding a service to the compose file
