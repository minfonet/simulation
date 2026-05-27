---
id: mem_learning_nextjs16_changes
type: learning
tags:
  - frontend
  - nextjs
  - upgrade
---

# Topic

Next.js 16 introduce cambios breaking respecto a 14/15.

# Details

- **middleware.ts → proxy.ts**: El archivo se renombra a `proxy.ts` y la función exportada pasa a llamarse `proxy`. Edge runtime ya no es soportado para proxy (solo Node.js).
- **params y searchParams son Promises**: En layout y page components, `params` y `searchParams` deben resolverse con `await` (Server Component) o `use()` (Client Component).
- **cookies() y headers() son async**: Deben usarse con `await`.
- **Turbopack es default**: `next dev` y `next build` usan Turbopack. Para usar webpack custom hay que pasar `--webpack`.
- **ESLint Flat Config es default**: `eslint.config.mjs` en vez de `.eslintrc.json`.
- **React 19.2**: Incluye View Transitions, `useEffectEvent`, y Activity.
- **Parallel Routes requieren default.js**: Ya no se infiere.

# Impact

Cualquier tutorial o ejemplo de Next.js 14-15 puede no funcionar. Verificar siempre la documentación local en `node_modules/next/dist/docs/` antes de implementar.
