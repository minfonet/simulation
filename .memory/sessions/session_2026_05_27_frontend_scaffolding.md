---
session_id: session_2026_05_27_frontend_scaffolding
agent: lead
---

# Goals

- Scaffold Next.js 16 frontend with TypeScript, Tailwind v4, App Router
- Create all role-based pages (Admin, Instructor, Trainee)
- Implement JWT auth flow (login, localStorage, auth context)
- Build UI component library (button, card, input, badge, etc.)
- Add Docker support (Dockerfile.frontend, docker-compose service)

# Findings

- create-next-app genera Next.js 16.2.6 con Tailwind v4 por defecto
- Tailwind v4 usa `@import "tailwindcss"` en vez de las directivas `@tailwind` de v3
- Next.js 16 cambia `middleware.ts` por `proxy.ts` y la función exportada pasa a llamarse `proxy`
- Los `params` y `searchParams` son Promises en Next.js 16 — hay que usar `async/await` o `use()`
- El scaffolding completo requirió ~25 archivos: 4 layouts, 12 pages, 7 UI components, 2 layout components, 3 lib files

# Problems

- proxy.ts verifica cookies pero el frontend usa localStorage — el proxy no protege rutas realmente (redirige siempre a /login en page refresh)
- No hay mecanismo de token refresh — si el JWT expira (60 min), las llamadas fallan sin recuperación
- api.delete retorna Promise<T> pero DELETE devuelve 204 — el tipo genérico es impreciso

# Decisions Made

- Usar localStorage para tokens JWT en lugar de cookies (simplicidad MVP, aunque rompe proxy.ts)
- AuthContext como Client Component con React Context + useAuth hook
- UI components hechos a mano (sin shadcn CLI) para evitar dependencias interactivas
- Sidebar con menú dinámico según rol del usuario
- Output mode "standalone" en next.config.ts para Docker multi-stage

# Memories Created

- mem_decision_nextjs16_proxy
- mem_decision_localstorage_auth
- mem_learning_tailwind_v4
- mem_learning_nextjs16_changes

# Next Steps

- Corregir proxy.ts para que no intente leer cookie que no existe (remover check server-side)
- Agregar token refresh interceptor en api.ts
- Iniciar simulación Godot 4
