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

AdminController.InviteUser crea una instancia directa de `PasswordService`:
```csharp
PasswordHash = new Services.PasswordService().Hash(request.Password)
```
Mientras que AuthController.Register usa la inyección de dependencias:
```csharp
// constructor injection: IPasswordService _password
PasswordHash = _password.Hash(request.Password)
```

# Root cause

El controller se escribió sin seguir el patrón establecido por AuthController. No hubo revisión de consistencia entre controladores.

# Impact

Bajo — funcionalmente idéntico (mismo algoritmo BCrypt). Pero viola el principio de DI y crea una instancia nueva en cada request.

# Fix

Inyectar `IPasswordService` en `AdminController` via constructor, igual que en `AuthController`.

# Status

✅ Fixed 2026-05-27 — added `using SimApi.Services;`, private field `_password`, constructor parameter, and replaced `new Services.PasswordService().Hash(...)` with `_password.Hash(...)`.

# Referencia

docs/99-reference/architecture-review.md — item 5
