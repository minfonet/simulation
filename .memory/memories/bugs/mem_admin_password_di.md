---
id: mem_bug_admin_password_di
type: bug
tags:
  - backend
  - di
  - consistency
  - low
---

# Issue

AdminController.InviteUser creates a direct instance of `PasswordService`:
```csharp
PasswordHash = new Services.PasswordService().Hash(request.Password)
```
While AuthController.Register uses dependency injection:
```csharp
// constructor injection: IPasswordService _password
PasswordHash = _password.Hash(request.Password)
```

# Root cause

The controller was written without following the pattern established by AuthController. There was no consistency review across controllers.

# Impact

Low — functionally identical (same BCrypt algorithm). But it violates the DI principle and creates a new instance on every request.

# Fix

Inject `IPasswordService` into `AdminController` via constructor, as in `AuthController`.

# Status

✅ Fixed 2026-05-27 — added `using SimApi.Services;`, private field `_password`, constructor parameter, and replaced `new Services.PasswordService().Hash(...)` with `_password.Hash(...)`.

# Reference

docs/99-reference/architecture-review.md — item 5
