---
id: mem_learning_nextjs16_changes
type: learning
tags:
  - frontend
  - nextjs
  - upgrade
---

# Topic

Next.js 16 introduces breaking changes compared with 14/15.

# Details

- **middleware.ts → proxy.ts**: The file is renamed to `proxy.ts` and the exported function becomes `proxy`. Edge runtime is no longer supported for proxy (Node.js only).
- **params and searchParams are Promises**: In layout and page components, `params` and `searchParams` must be resolved with `await` (Server Component) or `use()` (Client Component).
- **cookies() and headers() are async**: They must be used with `await`.
- **Turbopack is default**: `next dev` and `next build` use Turbopack. To use custom webpack, pass `--webpack`.
- **ESLint Flat Config is default**: `eslint.config.mjs` instead of `.eslintrc.json`.
- **React 19.2**: Includes View Transitions, `useEffectEvent`, and Activity.
- **Parallel Routes require default.js**: It is no longer inferred.

# Impact

Any Next.js 14-15 tutorial or example may not work. Always verify local documentation in `node_modules/next/dist/docs/` before implementing.
