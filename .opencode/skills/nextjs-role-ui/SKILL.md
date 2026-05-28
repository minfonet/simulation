---
name: nextjs-role-ui
description: nextjs-role-ui, Next.js role pages, Admin, Instructor, Trainee, auth localStorage, proxy, route map, UI states, and Vitest Testing Library; use for frontend role-flow work.
---

# Next.js Role UI

## When to use

- Use for changes under `src/frontend/sim-web` involving Admin, Instructor/Evaluator, Trainee/Evaluated pages, auth flow, route protection, session UI states, launch handoff UI, report UI, or page/component tests.
- Use when changing API client behavior, localStorage auth handling, or Next.js 16 `proxy.ts` behavior.

## When not to use

- Do not use for backend-only service/storage logic or Godot-only implementation unless frontend contracts change.
- Do not use to move business rules into pages or components.

## Read first

- `.memory/active-context.md`
- `docs/01-product/user-flows.md`
- `docs/01-product/flow-gap-plan.md`
- `docs/05-ui-ux/role-based-ui.md`
- `.memory/memories/architecture/mem_scalable_architecture_guardrails.md`
- `.memory/memories/decisions/mem_localstorage_auth.md`
- `.memory/memories/decisions/mem_nextjs16_proxy.md`
- `.memory/memories/bugs/mem_proxy_vs_localstorage.md`
- `.memory/memories/learnings/mem_nextjs16_changes.md`
- `.memory/memories/learnings/mem_vitest_nextjs16_setup.md`
- `docs/06-engineering/language-policy.md`

## Route map

- Public: `/login`
- Admin: `/admin`, `/admin/organizations`, `/admin/users`
- Instructor/Evaluator: `/instructor`, `/instructor/sessions`, `/instructor/sessions/[id]`, `/instructor/evaluations`
- Trainee/Evaluated: `/trainee`, `/trainee/sessions`, `/trainee/sessions/[id]`, `/trainee/evaluations`

## Implementation guidance

- Treat pages/components as frontend adapters: render states, call API contracts, and show errors; keep domain rules in backend/application contracts.
- Auth tokens currently live in `localStorage`; `proxy.ts` cannot read them. Keep `proxy.ts` minimal unless the auth storage strategy changes.
- Cover UI states: loading, empty, success, validation error, API error, unauthorized/forbidden, invalid session state, and disabled actions.
- For launch handoff UI, display controlled errors and avoid ambiguous session state after failed launch preparation.
- Use Vitest and Testing Library for page/component behavior; mock API calls at the boundary.

## Mandatory guardrails

- Do not put evaluation/reporting rules or scenario mapping logic directly in Next.js pages.
- Do not duplicate backend DTOs casually; shared contracts need explicit versioning and producer/consumer tests.
- Do not introduce real-time transport details into UI logic; use stable telemetry query/stream contracts if live updates are later added.
- Preserve role/ownership expectations: UI gating is not a substitute for backend authorization.

## MVP scope limits and anti-overengineering

- Keep role pages simple and focused on the documented flow.
- Do not add a complex design system, global state framework, or SSR auth migration unless explicitly requested.
- Base preset selection can be a minimal selector fed by an explicit contract; do not build a scenario editor for MVP.
- Report UI can be basic and manually reviewed; do not add automated scoring dashboards.

## Acceptance/test checklist

- Page/component tests cover route-specific role states, loading/empty/error/success states, and visible authorization failures.
- Auth tests cover localStorage token use, refresh/logout behavior when touched, and proxy caveats.
- Session UI tests cover Pending start, Active/Completed display, invalid-state errors, and report/evaluation visibility.
- Contract-consuming tests verify expected API payload fields for presets, launch handoff, reports, and telemetry summaries when those are in scope.
- Manual or automated checks confirm no product code outside the frontend scope was changed for UI-only tasks.
