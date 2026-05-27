---
id: mem_docker_compose_approach
type: decision
tags:
  - docker
  - infrastructure
  - mvp
---

# Context

Necesitábamos definir cómo orquestar los servicios del MVP (backend, base de datos, frontend futuro) para desarrollo local y eventual despliegue.

# Decision

Usar Docker Compose con un solo archivo `docker/docker-compose.yml`. Backend con Dockerfile multi-stage, Postgres 16 con health check, volúmenes persistentes. Frontend se agregará en fase 2 del MVP.

# Why

- Docker Compose es el estándar para desarrollo local de microservicios
- Multi-stage build mantiene la imagen de producción pequeña (solo runtime, no SDK)
- Health check en Postgres evita race conditions al iniciar backend
- Un solo archivo docker-compose.yml es suficiente para MVP — evita overengineering con Docker Swarm/K8s

# Consequences

- El equipo necesita Docker Desktop en Windows
- La base de datos se persiste en un volumen nombrado (pgdata)
- Agregar frontend solo requiere añadir un service al compose
