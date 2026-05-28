---
id: mem_admin_self_registration_signup
type: decision
tags:
  - auth
  - registration
  - bootstrap
  - frontend
  - backend
---

# Context

Users need to register the first Admin without using curl or Docker PostgreSQL commands. The bootstrap organization must exist before any user can register.

# Decision

1. **Auto-seed bootstrap org in Program.cs**: On first startup, the backend checks if the bootstrap org (ID `00000000-0000-0000-0000-000000000001`) exists. If not, it creates it automatically after `EnsureCreated()`.

2. **Signup/Signin toggle on /login page**: The login page now has a toggle between "Sign in" and "Sign up" modes. In signup mode, users enter Name, Email, and Password. Role is hardcoded to "Admin" and organizationId is hardcoded to the bootstrap org ID (both hidden from the user).

# Why

- Eliminates the need for curl commands or Docker PostgreSQL access for first-time setup
- Standard SaaS pattern: first user registers, then creates org and invites others
- Bootstrap org auto-seed ensures the register endpoint always has a valid org reference

# Consequences

- First Admin registers from the browser, is auto-logged in, and redirected to /admin
- The smoke test's step 2 (bootstrap seed via Docker) is now redundant but harmless
- The `register` function was added to AuthContext and follows the same pattern as `login`
