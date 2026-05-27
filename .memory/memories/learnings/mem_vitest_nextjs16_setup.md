# Vitest Setup with Next.js 16

## Context
Setting up Vitest test infrastructure for the Next.js 16 frontend (`src/frontend/sim-web/`).

## Learnings

### 1. Config extension: `.mts` over `.ts`
Next.js 16 uses ESM throughout. Using `vitest.config.mts` (instead of `.ts`) avoids module resolution issues with ESM import/export syntax. The config uses `import` + `defineConfig` from "vitest/config".

### 2. `vi.mock` hoisting requires `vi.hoisted()`
When `vi.mock("../api", factory)` is used, the factory function is hoisted to the top of the file by Vitest. Any variables referenced in the factory must be created with `vi.hoisted()`:

```typescript
const { mockGet } = vi.hoisted(() => ({
  mockGet: vi.fn(),
}))

vi.mock("../api", () => ({
  api: { get: mockGet },
}))
```

Without `vi.hoisted()`, the factory references undefined variables at hoist time.

### 3. Dynamic `import()` instead of `require()` in tests
Using `require("../auth-context")` inside test functions fails because Vitest/Vite's module resolution doesn't handle `require()` for `.tsx` files. Use `await import("../auth-context")` instead.

### 4. Retry loops in 401 mock
When testing the refresh/retry flow, ensure the mock returns 200 for retried requests, not 401. If retried requests also return 401, the `api.ts` internal logic will enter an infinite refresh → retry → 401 → refresh loop.

### 5. Memory considerations
Running all test files in sequence caused heap exhaustion (OOM) when the 401 retry loop was infinite. The OOM was a symptom, not the root cause. Once the infinite loop was fixed, all 26 tests run in ~1.5s with minimal memory.

### 6. jsdom environment is sufficient
Vitest with `environment: "jsdom"` provides `window`, `localStorage`, and `document` natively. No additional polyfills needed for testing browser-adjacent code. `globalThis.fetch` must be mocked via `vi.stubGlobal("fetch", mockFetch)`.

## Related files
- `src/frontend/sim-web/vitest.config.mts`
- `src/frontend/sim-web/lib/__tests__/setup.ts`
- `src/frontend/sim-web/lib/__tests__/api.test.ts`
- `src/frontend/sim-web/lib/__tests__/auth-context.test.tsx`
- `src/frontend/sim-web/lib/__tests__/proxy.test.ts`
