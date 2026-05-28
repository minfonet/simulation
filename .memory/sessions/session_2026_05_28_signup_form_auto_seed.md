# Session: Admin Self-Registration (Signup Form + Auto-Seed)

**Date:** 2026-05-28
**Type:** Implementation
**Status:** Complete

---

## What was done

### Problem
Users had to use `curl` to register the first Admin because:
1. The bootstrap org didn't exist without manual Docker PostgreSQL seeding
2. The login page had no register form

### Solution

**Backend — Auto-seed bootstrap org** (`src/backend/SimApi/Program.cs`):
- After `db.Database.EnsureCreated()`, checks if bootstrap org exists. If not, creates it with ID `00000000-0000-0000-0000-000000000001` and name "Bootstrap Organization".
- Added `using SimApi.Models;` import.

**Frontend — Register function** (`lib/auth-context.tsx`):
- Added `register(email, password, name, organizationId)` to AuthContext
- Calls `POST /api/auth/register` with `{ email, password, name, role: "Admin", organizationId }`
- Same pattern as `login`: stores tokens + user in localStorage

**Frontend — Signup toggle** (`app/login/page.tsx`):
- Added `isSignup` state toggle
- Signup mode shows: Name field, info paragraph about first-time Admin setup
- Role hardcoded to "Admin", orgId hardcoded to bootstrap org (both hidden)
- Toggle text: "Don't have an account? Sign up" / "Already have an account? Sign in"
- Title changes: "Create Account" / "Simulation Platform"
- Error handling for both modes

### Documentation updated
- `docs/01-product/execution-readiness.md`: Flow 1 updated — self-service steps
- `docs/01-product/user-flows.md`: Added 1.1 Sign Up section, renumbered 1.2→1.3, 1.3→1.4
- `.memory/memories/decisions/mem_admin_self_registration_signup.md`: Decision record
- `.memory/active-context.md`: Updated state, decisions, risks, tasks

## Files changed

| File | Action |
|------|--------|
| `src/backend/SimApi/Program.cs` | Added auto-seed bootstrap org + `using SimApi.Models` |
| `src/frontend/sim-web/lib/auth-context.tsx` | Added `register` function to AuthContext |
| `src/frontend/sim-web/app/login/page.tsx` | Added signup toggle with register form |
| `docs/01-product/execution-readiness.md` | Updated Flow 1 with self-service steps |
| `docs/01-product/user-flows.md` | Added Sign Up section, renumbered |
| `.memory/active-context.md` | Updated state, decisions, risks, tasks |
| `.memory/memories/decisions/mem_admin_self_registration_signup.md` | Decision record |

## Verification
- Backend builds: ✅ 0 errors
- Frontend compiles: ✅ 0 errors in changed files
- Docker Compose rebuild: ✅ 
- E2E smoke test: **19/19 PASS** ✅

## Next steps
- Frontend page component tests (~6 files) — known gap
- Fix 4 pre-existing TypeScript errors in `api.test.ts`
- Restart opencode to load skills before new work
