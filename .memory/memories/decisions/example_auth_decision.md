---
id: mem_auth_jwt_strategy
type: decision
tags:
  - auth
  - jwt
  - backend
---

# Context

Need simple local authentication for MVP.

# Decision

Use JWT access tokens with rotating refresh tokens.

# Why

Simple implementation and stateless backend.

# Consequences

Need secure refresh token storage.
