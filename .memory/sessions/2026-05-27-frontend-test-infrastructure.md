# Session: 2026-05-27 — Frontend test infrastructure + tests + docs

## Summary
Set up Vitest testing infrastructure for the Next.js 16 frontend, wrote 23 passing tests across 3 test files, and updated run-and-debug.md documentation.

## What was done

### Infrastructure
- Installed `vitest@3`, `@testing-library/react@16`, `@testing-library/jest-dom@6`, `jsdom@25`
- Created `vitest.config.mts` with jsdom environment, `@/` path alias, and jest-dom setup
- Created `lib/__tests__/setup.ts` with `@testing-library/jest-dom/vitest` import
- Added `"test"` and `"test:watch"` scripts to `package.json`

### Tests written (23 passing)
- **`api.test.ts`** (10 tests): auth header injection (GET/POST/PUT/DELETE), 401 refresh flow, concurrent 401 queuing with single refresh, refresh failure → clearAuthState + onSessionExpired, 204 → undefined, non-OK error body handling
- **`auth-context.test.tsx`** (7 tests): loading state, localStorage restore, login stores tokens, logout clears state, validateSession calls `/api/auth/me`, validateSession redirects on failure, setOnSessionExpired registration
- **`proxy.test.ts`** (6 tests): all routes pass through (login, admin, instructor, trainee), config matcher exported

### Documentation
- Added "Testing" section to `run-and-debug.md` with run commands, test file table, and backend/godot notes
- Added troubleshooting row for Vitest jsdom errors
- Updated project structure reference to include `vitest.config.mts` and `lib/__tests__/`

### Memory recording
- Created learning memory: `mem_vitest_nextjs16_setup.md` (vi.hoisted(), dynamic import(), retry loop avoidance, jsdom environment)
- Updated `active-context.md` with test infrastructure table and updated task list

## Issues encountered
1. **OOM from infinite retry loop**: The concurrent 401 test mock returned 401 even for retried requests, causing `api.ts` to enter an infinite refresh → retry → 401 loop. Fixed by tracking retried URLs in the mock.
2. **`vi.mock` hoisting**: Factory variables needed `vi.hoisted()`. Fixed.
3. **`require()` not supported**: Test files needed `await import()` instead. Fixed.

## Next steps
- Backend integration tests (completed 2026-05-28: 50/50 passing)
- E2E smoke tests via PowerShell (script corrected 2026-05-28; live Docker execution pending)
