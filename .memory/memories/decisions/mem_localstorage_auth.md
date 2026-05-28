---
id: mem_decision_localstorage_auth
type: decision
tags:
  - frontend
  - auth
  - jwt
  - localStorage
---

# Context

We needed to store the JWT in the frontend to include it in the Authorization headers for API calls. Options: localStorage, sessionStorage, cookies (HTTP-only or not).

# Decision

Use localStorage for accessToken and refreshToken. The user object is stored as JSON in localStorage to persist the session across refreshes.

# Why

- localStorage survives tab closes (unlike sessionStorage)
- It is accessible from JavaScript without a backend endpoint (unlike HTTP-only cookies)
- Direct implementation: getItem/setItem in auth-context.tsx and api.ts
- Sufficient for MVP — HTTP-only cookies will be considered post-MVP when real security is needed

# Consequences

- proxy.ts cannot read the token (runs on server) — server-side auth does not work
- Vulnerable to XSS (like any localStorage) — acceptable for MVP
- No authenticated server-side rendering — all pages are Client Components
- The token persists until manual logout or localStorage.clear()
