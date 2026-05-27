---
id: mem_decision_localstorage_auth
type: decision
tags:
  - frontend
  - auth
  - jwt
  - localStorage
---

# Context

Necesitábamos almacenar el JWT en el frontend para incluirlo en los headers Authorization de las llamadas API. Opciones: localStorage, sessionStorage, cookies (HTTP-only o no).

# Decision

Usar localStorage para accessToken y refreshToken. El user object se guarda como JSON en localStorage para persistir sesión entre refreshes.

# Why

- localStorage sobrevive a cierres de pestaña (a diferencia de sessionStorage)
- Es accesible desde JavaScript sin necesidad de endpoint backend (a diferencia de HTTP-only cookies)
- Implementación directa: getItem/setItem en auth-context.tsx y api.ts
- Suficiente para MVP — HTTP-only cookies se considerarán post-MVP cuando haya seguridad real

# Consequences

- proxy.ts no puede leer el token (corre en servidor) — auth server-side no funciona
- Vulnerable a XSS (como cualquier localStorage) — aceptable para MVP
- No hay server-side rendering authenticated — todas las páginas son Client Components
- El token persiste hasta logout manual o localStorage.clear()
