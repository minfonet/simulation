---
id: mem_decision_nextjs16_proxy
type: decision
tags:
  - frontend
  - nextjs
  - proxy
  - middleware
---

# Context

Next.js 16 replaces `middleware.ts` with `proxy.ts`, and the exported function changes from `middleware` to `proxy`. The runtime changes from Edge to Node.js.

# Decision

Use `proxy.ts` at the project root (not `src/`) with exported function `proxy`. The matcher defines which routes pass through the proxy. Edge runtime is not used.

# Why

This is the Next.js 16 convention. Using `middleware.ts` would generate deprecation warnings and eventually stop working.

# Consequences

- The proxy runs in Node.js runtime (not Edge) — it can use Node APIs
- The proxy CANNOT read browser localStorage — only cookies/headers
- For MVP, the proxy should only call NextResponse.next() and leave auth to backend + frontend
- Documentado en docs/06-engineering/run-and-debug.md
