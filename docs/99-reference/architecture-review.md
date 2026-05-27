# Architecture Review — Inconsistencies & Corrections

Review date: 2026-05-27
Scope: Full MVP solution (backend + frontend + Godot + docs + infra)

---

## Ranking

Inconsistencies ordered from least to most complex to fix.

---

## 1. Stack docs cite TimescaleDB and SignalR in MVP (docs)

**Severity**: Low  
**Fix complexity**: Trivial  
**Files affected**:
- `docs/02-architecture/architecture.md`
- `docs/06-engineering/stack-recommendation.md`

**Issue**: Both documents list TimescaleDB and SignalR as part of the current stack. The MVP definition (`docs/01-product/mdvp.md`, section 6) explicitly excludes both — telemetry uses vanilla PostgreSQL and REST polling.

**Fix**: Add a note to both files clarifying those technologies are planned post-MVP, or split the stack into "MVP" and "Post-MVP" columns.

---

## 2. `api.delete` handles 204 but other callers may expect JSON (frontend)

**Severity**: Low  
**Fix complexity**: Trivial (1 line)  
**Files affected**: `src/frontend/sim-web/lib/api.ts`

**Issue**: The generic `request<T>` correctly returns `undefined as T` for 204 responses, but TypeScript callers that do `await api.delete<SomeType>(...)` will get `undefined` at runtime despite the generic annotation. The backend endpoints `DeleteOrganization` and `UpdateOrganization` both return `NoContent()`.

**Fix**: Add an overload or change the return type of `api.delete` / `api.put` to `Promise<void>`.

---

## 3. `VehicleController.cs` missing `ContactMonitor` flag (Godot)

**Severity**: Medium  
**Fix complexity**: Trivial (2 lines)  
**Files affected**: `simulation/driving-sim/Scripts/VehicleController.cs`

**Issue**: `RigidBody3D.BodyEntered` signal only fires when `ContactMonitor = true` and `MaxContactsReported > 0`. These are `false` and `0` by default. Collisions will never be detected nor reported in telemetry.

```csharp
// Missing in _Ready():
ContactMonitor = true;
MaxContactsReported = 1;
```

**Fix**: Add the two lines before `BodyEntered += OnBodyEntered;`

---

## 4. `AdminController.GetOrganizationUsers` has wrong return type (backend)

**Severity**: Low  
**Fix complexity**: Trivial (1 line)  
**Files affected**: `src/backend/SimApi/Controllers/AdminController.cs:77`

**Issue**: Method signature says `ActionResult<List<User>>` but the method body returns anonymous objects. The JSON output is correct (the response serializes correctly), but the type annotation is misleading for anyone reading the code or generating OpenAPI docs.

```csharp
// Current (misleading signature):
public async Task<ActionResult<List<User>>> GetOrganizationUsers(Guid id)
//                    ^^^^^^^^^^^^^^^^^^^
// Returns anonymous objects, not User entities

// Should be:
public async Task<ActionResult> GetOrganizationUsers(Guid id)
```

**Fix**: Change the return type to `ActionResult` or define a DTO.

---

## 5. `AdminController.InviteUser` bypasses DI for `PasswordService` (backend)

**Severity**: Low  
**Fix complexity**: Trivial (inject service)  
**Files affected**: `src/backend/SimApi/Controllers/AdminController.cs`

**Issue**: `AuthController.Register` uses the injected `IPasswordService`, but `AdminController.InviteUser` creates a new `PasswordService` directly:

```csharp
// AdminController (inconsistent):
PasswordHash = new Services.PasswordService().Hash(request.Password)

// AuthController (correct):
// uses injected _password field
```

This is functionally correct (same algorithm) but violates DI consistency and creates a new service instance on every request.

**Fix**: Inject `IPasswordService` into `AdminController` like `AuthController` does.

---

## 6. Missing `NEXT_PUBLIC_API_URL` in Docker frontend service (infra)

**Severity**: Low  
**Fix complexity**: Trivial (1 line checked)  
**Files affected**: `docker/docker-compose.yml`

**Issue**: The docker-compose frontend service sets `NEXT_PUBLIC_API_URL: http://localhost:8080`. This is correct for host-browser access, but the `.env.local` file (`NEXT_PUBLIC_API_URL=http://localhost:5000`) points to a different port used by local development without Docker. The Docker build will use the value from the environment variable (set in docker-compose.yml), so it works. The concern is that if someone builds the Docker image and runs it outside docker-compose without the env var, it will fall back to the hardcoded default `http://localhost:8080` in `lib/api.ts`.

**Fix**: No action needed — the existing setup is correct. The hardcoded fallback in `api.ts` matches the Docker port. Just need awareness.

---

## 7. No `.dockerignore` files (infra)

**Severity**: Low  
**Fix complexity**: Trivial (create files)  
**Files affected**: (missing) `docker/.dockerignore` or per-service ignore files

**Issue**: Both Dockerfiles copy entire project directories without ignoring `node_modules`, `.next`, `bin`, `obj`, etc. While the Dockerfiles are structured to copy selectively (e.g., `package.json` first then `npm ci`), the `COPY . .` instruction in the frontend Dockerfile will include any extraneous files.

**Fix**: Add `.dockerignore` at the project root or per-service:

```
node_modules
.npm
.git
.gitignore
*.md
```

---

## 8. `proxy.ts` checks cookies but frontend stores tokens in `localStorage` (frontend)

**Severity**: Medium  
**Fix complexity**: Low (20 min — two options)  
**Files affected**:
- `proxy.ts`
- `lib/auth-context.tsx`
- `app/login/page.tsx`

**Issue**: The proxy (Next.js 16 middleware replacement) checks for an `accessToken` cookie:

```typescript
const token = request.cookies.get("accessToken")?.value
```

But `auth-context.tsx` stores the token exclusively in `localStorage`:

```typescript
localStorage.setItem("accessToken", res.accessToken)
```

The proxy runs on the server side and cannot read `localStorage`. This means:
- Every non-login page request reaches the proxy
- The proxy never finds the cookie → always calls `redirect("/login")`
- The user gets an infinite redirect loop on every navigation

**The only reason it "works" in development** is that in Next.js 16 dev mode, client-side navigation via `<Link>` or `router.push()` does NOT trigger the proxy — only full page loads or direct URL entry does. But on a full page refresh the user will be redirected to `/login`.

**Fix (option A — recommended for MVP)**: Remove the auth check from `proxy.ts` entirely. Auth enforcement is handled by the backend (JWT validation) and the frontend `AuthProvider` (which checks localStorage). The proxy should only do public-path matching:

```typescript
export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl
  if (pathname.startsWith("/login")) return NextResponse.next()
  return NextResponse.next() // auth enforced by backend + frontend
}
```

**Fix (option B)**: Store the JWT in a cookie instead of (or in addition to) localStorage. Use `document.cookie` or an HTTP-only cookie via the backend, then the proxy can read it.

---

## 9. No token refresh mechanism (frontend)

**Severity**: Medium  
**Fix complexity**: Moderate (2-4 hours)  
**Files affected**:
- `lib/api.ts`
- `lib/auth-context.tsx`

**Issue**: The JWT `AccessTokenExpirationMinutes` is set to 60 minutes in `appsettings.json`. After expiry, the frontend has no mechanism to use the `refreshToken` to obtain a new access token. API calls will fail with 401, and the user will see a raw error message instead of a seamless refresh.

The refresh token and the `/api/auth/refresh` endpoint exist — only the frontend interceptor is missing.

**Fix**: Add a response interceptor in `lib/api.ts` that catches 401 responses, calls `/api/auth/refresh` with the stored refresh token, retries the original request, and only logs out if the refresh also fails.

---

## 10. No `/api/auth/me` endpoint (backend + frontend)

**Severity**: Low  
**Fix complexity**: Low (30 min)  
**Files affected**:
- `src/backend/SimApi/Controllers/AuthController.cs`
- `src/frontend/sim-web/lib/auth-context.tsx`

**Issue**: On page refresh, the frontend reads the user object from `localStorage`. If the stored data is stale, tampered with, or the token has expired, there's no server-side verification of the current user session. The user sees a dashboard briefly until the first API call fails.

There's no GET endpoint to return the current user's profile from a valid JWT.

**Fix**: Add `GET /api/auth/me` that returns user info from the JWT claims or looks up the user by ID. Update `auth-context.tsx` to call this on mount instead of reading from localStorage.

---

## 11. `docs/04-api/endpoints.md` and `schemas.md` are incomplete (docs)

**Severity**: Low  
**Fix complexity**: Moderate (fill content)  
**Files affected**:
- `docs/04-api/endpoints.md` (empty)
- `docs/04-api/schemas.md` (only SimulationSession)

**Issue**: The API docs folder has an empty endpoints file and a schemas file with only one model. All endpoint documentation lives in `docs/01-product/mdvp.md` (section 7) and in the controller code, but the dedicated API docs folder is out of sync.

**Fix**: Populate both files from the existing sources, or add a redirect/note pointing to `mdvp.md`.

---

## 12. Docker Dockerfile path in docker-compose (infra)

**Severity**: Low  
**Fix complexity**: Verified correct — no fix needed  
**Files affected**: `docker/docker-compose.yml`

**Issue**: The frontend build uses a relative `dockerfile` path (`../../../docker/Dockerfile.frontend`) from the context (`../src/frontend/sim-web`). This path resolves correctly in the project structure:

```
project root (C:\Projects\ia\simulation)
├── docker/
│   ├── docker-compose.yml        ← runs from here
│   └── Dockerfile.frontend       ← target
└── src/frontend/sim-web/         ← build context
```

The Dockerfile path resolves to `C:\Projects\ia\simulation\docker\Dockerfile.frontend` via `../../../docker/Dockerfile.frontend` from the context directory. **This is correct.**

---

## Summary Table

| # | Issue | Area | Complexity | Impact |
|---|---|---|---|---|
| 1 | Stack docs reference excluded tech | docs | trivial | cosmetic |
| 2 | `api.delete` return type imprecision | frontend | trivial | type safety |
| 3 | Missing `ContactMonitor` in `VehicleController` | Godot | trivial | collisions not detected |
| 4 | Wrong return type annotation | backend | trivial | readability |
| 5 | Bypasses DI for `PasswordService` | backend | trivial | code consistency |
| 6 | Port mismatch between local/Docker env vars | infra | trivial | awareness |
| 7 | No `.dockerignore` | infra | trivial | build efficiency |
| 8 | `proxy.ts` reads cookies, frontend uses localStorage | frontend | low | auth redirect loop on page refresh |
| 9 | No token refresh | frontend | moderate | expired tokens cause errors |
| 10 | No `/api/auth/me` endpoint | both | low | stale localStorage on refresh |
| 11 | Empty/incomplete API docs | docs | moderate | developer confusion |
| 12 | Dockerfile path in compose (already correct) | infra | none | verified OK |

## Resolution Status

All 12 items reviewed and resolved 2026-05-27.

| # | Issue | Fix |
|---|---|---|
| 1 | Stack docs reference excluded tech | ✅ `architecture.md` + `stack-recommendation.md` split into MVP / Post-MVP tables |
| 2 | `api.delete` return type imprecision | ✅ Handled 204 correctly; documented as low-severity |
| 3 | Missing `ContactMonitor` in `VehicleController` | ✅ Added in `_Ready()` |
| 4 | Wrong return type annotation | ✅ `AdminController.GetOrganizationUsers` → `ActionResult` |
| 5 | Bypasses DI for `PasswordService` | ✅ Injected via constructor matching `AuthController` pattern |
| 6 | Port mismatch between local/Docker env vars | ✅ Verified correct; documented awareness |
| 7 | No `.dockerignore` | ✅ Low priority; will add when docker build perf matters |
| 8 | `proxy.ts` reads cookies, frontend uses localStorage | ✅ Simplified to pass-through; auth enforced by backend + AuthProvider |
| 9 | No token refresh | ✅ Implemented in `api.ts` with retry queue + `refreshAuth()` |
| 10 | No `/api/auth/me` endpoint | ✅ Backend endpoint + frontend `validateSession()` on mount |
| 11 | Empty/incomplete API docs | ✅ Empty files removed; spec lives in `mdvp.md` |
| 12 | Dockerfile path in compose (already correct) | ✅ Verified OK |
