# User Flows — MVP

---

## 1. Authentication

### 1.1 Login

```
[User] → Opens /login → Enters email + password → POST /api/auth/login
                                                          │
                                                     ┌────┴────┐
                                                     │ 200 OK  │ 401
                                                     └────┬────┘ └──→ Show error
                                                          │
                                           Store accessToken + refreshToken
                                           in localStorage
                                                          │
                                              Redirect to role dashboard:
                                              Admin → /admin
                                              Instructor → /instructor
                                              Trainee → /trainee
```

**Frontend**: `app/login/page.tsx`  
**Backend**: `POST /api/auth/login` → `AuthController.Login()`  
**Persistence**: `User` table, JWT in localStorage

### 1.2 Logout

```
[User] → Clicks "Sign out" → localStorage.clear()
                              Redirect to /login
```

**Frontend**: `components/layout/sidebar.tsx` → `useAuth().logout()`

### 1.3 Token refresh (automatic)

```
[API call returns 401] → Retry with refreshToken
                          POST /api/auth/refresh
                              │
                         ┌────┴────┐
                         │ 200 OK  │ 401 → Force logout
                         └────┬────┘
                              │
                    Store new tokens → Retry original request
```

**Frontend**: `lib/api.ts` (interceptor not yet implemented — direct call for MVP)  
**Backend**: `POST /api/auth/refresh` → `AuthController.Refresh()`

---

## 2. Admin Flows

### 2.1 Create Organization

```
[Admin] → /admin/organizations → Enters org name → Clicks "Create"
                                                      │
                                          POST /api/admin/organizations
                                                      │
                                                    201
                                                      │
                                              Org appears in list
```

**Frontend**: `app/admin/organizations/page.tsx`  
**Backend**: `POST /api/admin/organizations` → `AdminController.CreateOrganization()`  
**Persistence**: `Organization` table

### 2.2 Delete Organization

```
[Admin] → /admin/organizations → Clicks "Delete" on an org
                                    │
                          DELETE /api/admin/organizations/{id}
                                    │
                                  204
                                    │
                          Org removed from list
```

**Frontend**: `app/admin/organizations/page.tsx`  
**Backend**: `DELETE /api/admin/organizations/{id}` → `AdminController.DeleteOrganization()`

### 2.3 Invite User to Organization

```
[Admin] → /admin/users
            │
      Select organization from dropdown
            │
      Fill: email, password, name, role
            │
      Click "Invite User"
            │
      POST /api/admin/organizations/{id}/users
            │
          200
            │
      User appears in list below the form
```

**Frontend**: `app/admin/users/page.tsx`  
**Backend**: `POST /api/admin/organizations/{id}/users` → `AdminController.InviteUser()`  
**Persistence**: `User` table

### 2.4 View Admin Dashboard

```
[Admin] → /admin
            │
      GET /api/admin/organizations (list)
            │
      Display: total orgs count + total users count
```

**Frontend**: `app/admin/page.tsx`  
**Backend**: `GET /api/admin/organizations` → `AdminController.GetOrganizations()`

---

## 3. Instructor Flows

### 3.1 Create Session

```
[Instructor] → /instructor/sessions
                  │
          Select trainee from dropdown
          Enter scenario name
                  │
          Click "Create Session"
                  │
          POST /api/instructor/sessions
                  │
                201
                  │
          Session appears in list (status: Pending)
```

**Frontend**: `app/instructor/sessions/page.tsx`  
**Backend**: `POST /api/instructor/sessions` → `InstructorController.CreateSession()`  
**Persistence**: `SimulationSession` table

### 3.2 View Sessions

```
[Instructor] → /instructor/sessions
                  │
          GET /api/instructor/sessions
                  │
          List: trainee name, scenario, status badge, score
                  │
          Click a session → /instructor/sessions/{id}
```

**Frontend**: `app/instructor/sessions/page.tsx`  
**Backend**: `GET /api/instructor/sessions` → `InstructorController.GetSessions()`

### 3.3 Monitor Session + Evaluate

```
[Instructor] → /instructor/sessions/{id}
                  │
          GET /api/instructor/sessions/{id}
          GET /api/telemetry/session/{sessionId}
                  │
          View: status badge, scenario, telemetry table
                  │
          ┌── If session is "Completed": ──────────────┐
          │  Fill: score (0-100) + comments             │
          │  Click "Submit Evaluation"                  │
          │  POST /api/instructor/sessions/{id}/evaluate│
          │  Score + notes saved to session             │
          └─────────────────────────────────────────────┘
```

**Frontend**: `app/instructor/sessions/[id]/page.tsx`  
**Backend**: `POST /api/instructor/sessions/{id}/evaluate` → `InstructorController.EvaluateSession()`  
**Persistence**: `Evaluation` table + `SimulationSession.Score/InstructorNotes`

### 3.4 View Evaluation History

```
[Instructor] → /instructor/evaluations
                  │
          GET /api/instructor/evaluations
                  │
          List: score, comments, date
```

**Frontend**: `app/instructor/evaluations/page.tsx`  
**Backend**: `GET /api/instructor/evaluations` → `InstructorController.GetEvaluations()`

### 3.5 View Assigned Trainees

```
[Instructor] → /instructor (dashboard)
                  │
          GET /api/instructor/trainees
                  │
          Display: list of trainees in same organization
```

**Frontend**: `app/instructor/page.tsx`  
**Backend**: `GET /api/instructor/trainees` → `InstructorController.GetTrainees()`

---

## 4. Trainee Flows

### 4.1 View Assigned Sessions

```
[Trainee] → /trainee/sessions
               │
        GET /api/trainee/sessions
               │
        List: scenario, instructor name, status badge, score
               │
        If Pending  → "Start" button
        If Completed → "View" button (links to detail)
```

**Frontend**: `app/trainee/sessions/page.tsx`  
**Backend**: `GET /api/trainee/sessions` → `TraineeController.GetSessions()`

### 4.2 Start Simulation

```
[Trainee] → /trainee/sessions
               │
        Click "Start" on a Pending session
               │
        POST /api/trainee/sessions/{id}/start
               │
              200
               │
        Status changes to "Active"
               │
        (MVP: manual status change — Godot integration TBD)
```

**Frontend**: `app/trainee/sessions/page.tsx`  
**Backend**: `POST /api/trainee/sessions/{id}/start` → `TraineeController.StartSession()`  
**Persistence**: `SimulationSession.Status` → `Active`

### 4.3 Finish Simulation

```
[Trainee] → /trainee/sessions (or Godot calls API directly)
               │
        POST /api/trainee/sessions/{id}/finish
               │
              200
               │
        Status changes to "Completed"
        CompletedAt timestamp set
```

**Frontend**: `app/trainee/sessions/[id]/page.tsx` or `BackendClient.FinishSession()`  
**Backend**: `POST /api/trainee/sessions/{id}/finish` → `TraineeController.FinishSession()`  
**Persistence**: `SimulationSession.Status` → `Completed`, `CompletedAt`

### 4.4 View Session Detail (Telemetry + Evaluation)

```
[Trainee] → /trainee/sessions/{id}
               │
        GET /api/trainee/sessions/{id}
        GET /api/telemetry/session/{sessionId}
               │
        View: status, instructor, scenario, score
              telemetry table (last 50 points)
              instructor notes (if evaluated)
```

**Frontend**: `app/trainee/sessions/[id]/page.tsx`  
**Backend**: `GET /api/trainee/sessions/{id}`, `GET /api/telemetry/session/{sessionId}`

### 4.5 View Evaluations

```
[Trainee] → /trainee/evaluations
               │
        GET /api/trainee/evaluations
               │
        List: instructor name, score, comments, date
        Color badge: green (≥70%), yellow (≥40%), red (<40%)
```

**Frontend**: `app/trainee/evaluations/page.tsx`  
**Backend**: `GET /api/trainee/evaluations` → `TraineeController.GetEvaluations()`

---

## 5. Complete E2E Happy Path

```
┌────────────────────────────────────────────────────────────────────┐
│ 1. Admin creates organization "ABC Driving School"                 │
│    POST /api/admin/organizations                                   │
├────────────────────────────────────────────────────────────────────┤
│ 2. Admin invites Instructor (Mary) and Trainee (John)              │
│    POST /api/admin/organizations/{id}/users  (×2)                  │
├────────────────────────────────────────────────────────────────────┤
│ 3. Mary logs in → /instructor/sessions                             │
│    Creates session "Practice 1 - Sharp Turn" → assigns John        │
│    POST /api/instructor/sessions                                   │
├────────────────────────────────────────────────────────────────────┤
│ 4. John logs in → /trainee/sessions                                │
│    Sees the session → clicks "Start"                               │
│    POST /api/trainee/sessions/{id}/start                           │
├────────────────────────────────────────────────────────────────────┤
│ 5. John drives in Godot simulation                                 │
│    Godot sends telemetry every ~10 frames                          │
│    POST /api/telemetry (batch)                                     │
├────────────────────────────────────────────────────────────────────┤
│ 6. John finishes simulation                                        │
│    POST /api/trainee/sessions/{id}/finish                          │
├────────────────────────────────────────────────────────────────────┤
│ 7. Mary views session → /instructor/sessions/{id}                  │
│    Reviews telemetry, fills score=85, comments="Good control"      │
│    POST /api/instructor/sessions/{id}/evaluate                     │
├────────────────────────────────────────────────────────────────────┤
│ 8. John views evaluation → /trainee/evaluations                    │
│    Sees score 85% + instructor notes                               │
└────────────────────────────────────────────────────────────────────┘
```

---

## 6. Error / Edge Case Flows

### 6.1 Invalid credentials
```
Login → 401 → Show "Invalid email or password" → Stay on /login
```

### 6.2 Session not in correct state
```
Start a non-Pending session → 400 → Show "Session is not in pending status"
Evaluate a non-Completed session → 400 → Evaluation form disabled
```

### 6.3 Email already registered
```
Register with existing email → 409 → Show "Email already registered"
```

### 6.4 Unauthorized access
```
Access /admin without Admin role → 403 → Backend rejects
Frontend proxy redirects to /login if no token
```

### 6.5 Network error / backend down
```
API call fails → Frontend shows error message
Godot: GD.PrintErr, keeps driving (telemetry lost)
```

---

## 7. Route Map (Frontend)

| Path | Role | Page |
|---|---|---|
| `/login` | Public | Login form |
| `/admin` | Admin | Dashboard (orgs + users count) |
| `/admin/organizations` | Admin | CRUD organizations |
| `/admin/users` | Admin | Invite users per org |
| `/instructor` | Instructor | Dashboard (sessions count + trainees) |
| `/instructor/sessions` | Instructor | List + create sessions |
| `/instructor/sessions/[id]` | Instructor | Telemetry + evaluation form |
| `/instructor/evaluations` | Instructor | Evaluation history |
| `/trainee` | Trainee | Dashboard (sessions + evaluations count) |
| `/trainee/sessions` | Trainee | List sessions (start/view) |
| `/trainee/sessions/[id]` | Trainee | Telemetry + instructor notes |
| `/trainee/evaluations` | Trainee | Evaluation history |
