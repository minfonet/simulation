---
id: mem_tsc_noemit_test_file_errors
type: learning
tags:
  - frontend
  - typescript
  - test
  - pre-existing
---

# Pre-Existing TypeScript Errors in `api.test.ts` with `tsc --noEmit`

## Observation

Running `npx tsc --noEmit` in `src/frontend/sim-web` reports 4 errors, all in `lib/__tests__/api.test.ts`:

1. **Lines 27, 29**: Unused `@ts-expect-error` directives above `json` and `text` mock properties in `mockResponse()`. TypeScript no longer considers these lines erroneous, so the suppression directives are flagged.
2. **Lines 105, 144**: Mock `fetch` implementation parameters typed as `(url: string)` or `(url: string, opts?: RequestInit)` are incompatible with the actual `fetch` signature which accepts `(input: string | URL | Request, init?: RequestInit | undefined)`.

## Impact

- **None on builds**: `npm run build` (Next.js production build) compiles successfully.
- **None on tests**: `npm test` (Vitest) runs all 23 tests passing.
- These are pre-existing issues in test-only code, not introduced by the current P0 changes.

## Root Cause

The `@ts-expect-error` directives were added when the project was first scaffolded and the TypeScript version at that time flagged those lines. A newer TypeScript version (or different configuration resolution) no longer flags them. The mock function signatures diverged from the evolving `fetch` type definitions.

## Resolution

Fix by:
- Removing the two unused `@ts-expect-error` directives in `mockResponse()`.
- Widening mock implementation parameter types to `(input: string | URL | Request, init?: RequestInit)` or using `vi.fn<typeof fetch>()` with proper implementation signatures.
