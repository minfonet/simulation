---
id: mem_bug_proxy_vs_localstorage
type: bug
tags:
  - frontend
  - auth
  - proxy
  - critical
---

# Issue

proxy.ts checks `request.cookies.get("accessToken")`, but the frontend stores the token in `localStorage`. The proxy (server-side) cannot read localStorage. On page refresh, the proxy redirects to /login even though the user is authenticated.

# Root cause

proxy.ts was written assuming cookies (a common Next.js pattern), but auth-context.tsx was implemented with localStorage (an explicit decision for simplicity). There was no cross-review between both files.

# Impact

High on page refresh (redirect loop), low in SPA navigation (client-side navigation does not go through proxy in dev).

# Fix

Option A (recommended MVP): proxy.ts should only call `NextResponse.next()` and leave authentication to the backend + frontend AuthProvider.
Option B: Migrate to cookies so the proxy can read the token.

# Status

✅ Fixed 2026-05-27 — applied Option A: proxy function body replaced with `return NextResponse.next()`.

# Reference

docs/99-reference/architecture-review.md — item 8
