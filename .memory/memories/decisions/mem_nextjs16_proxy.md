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

Next.js 16 reemplaza `middleware.ts` por `proxy.ts` y la función exportada pasa de llamarse `middleware` a `proxy`. El runtime cambia de Edge a Node.js.

# Decision

Usar `proxy.ts` en la raíz del proyecto (no `src/`) con función exportada `proxy`. El matcher define qué rutas pasan por el proxy. No se usa Edge runtime.

# Why

Es la convención de Next.js 16. Usar `middleware.ts` generaría warnings de deprecación y eventualmente dejaría de funcionar.

# Consequences

- El proxy corre en Node.js runtime (no Edge) — puede usar APIs de Node
- El proxy NO puede leer localStorage del browser — solo cookies/headers
- Para MVP, el proxy solo debe hacer NextResponse.next() y dejar la auth al backend + frontend
- Documentado en docs/06-engineering/run-and-debug.md
