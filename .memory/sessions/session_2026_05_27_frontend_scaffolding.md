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

- create-next-app generates Next.js 16.2.6 with Tailwind v4 by default
- Tailwind v4 uses `@import "tailwindcss"` instead of the v3 `@tailwind` directives
- Next.js 16 changes `middleware.ts` to `proxy.ts`, and the exported function becomes `proxy`
- `params` and `searchParams` are Promises in Next.js 16 — use `async/await` or `use()`
- Complete scaffolding required ~25 files: 4 layouts, 12 pages, 7 UI components, 2 layout components, 3 lib files

# Problems

- proxy.ts checks cookies but the frontend uses localStorage — the proxy does not really protect routes (always redirects to /login on page refresh)
- There is no token refresh mechanism — if the JWT expires (60 min), calls fail without recovery
- api.delete returns Promise<T> but DELETE returns 204 — the generic type is imprecise

# Decisions Made

- Use localStorage for JWT tokens instead of cookies (MVP simplicity, although it breaks proxy.ts)
- AuthContext as Client Component with React Context + useAuth hook
- Hand-built UI components (without shadcn CLI) to avoid interactive dependencies
- Sidebar with dynamic menu by user role
- Output mode "standalone" in next.config.ts for multi-stage Docker

# Memories Created

- mem_decision_nextjs16_proxy
- mem_decision_localstorage_auth
- mem_learning_tailwind_v4
- mem_learning_nextjs16_changes

# Next Steps

- Fix proxy.ts so it does not try to read a cookie that does not exist (remove server-side check)
- Add token refresh interceptor in api.ts
- Start Godot 4 simulation
