---
id: mem_bug_proxy_vs_localstorage
type: bug
tags:
  - frontend
  - auth
  - proxy
  - critical
---

# Issue

proxy.ts verifica `request.cookies.get("accessToken")` pero el frontend almacena el token en `localStorage`. El proxy (server-side) no puede leer localStorage. En page refresh, el proxy redirige a /login aunque el usuario esté autenticado.

# Root cause

proxy.ts se escribió asumiendo cookies (patrón común en Next.js), pero auth-context.tsx se implementó con localStorage (decisión explícita por simplicidad). No hubo revisión cruzada entre ambos archivos.

# Impact

Alto en page refresh (redirect loop), bajo en navegación SPA (client-side navigation no pasa por proxy en dev).

# Fix

Opción A (recomendada MVP): proxy.ts solo debe hacer `NextResponse.next()` y dejar la autenticación al backend + AuthProvider del frontend.
Opción B: Migrar a cookies para que el proxy pueda leer el token.

# Status

✅ Fixed 2026-05-27 — applied Option A: proxy function body replaced with `return NextResponse.next()`.

# Referencia

docs/99-reference/architecture-review.md — item 8
