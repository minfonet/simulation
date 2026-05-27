# Role-Based UI — MVP

## Admin

### Pages
| Route | Component | Description |
|---|---|---|
| `/admin` | Dashboard | Shows total organizations and total users |
| `/admin/organizations` | Organization List | Create / delete organizations, see user count per org |
| `/admin/users` | User Management | Select org from dropdown, invite users (email, password, name, role) |

### Layout
- Sidebar with menu: Dashboard, Organizations, Users
- Top navbar with user name + email
- Sign out button at sidebar bottom

## Instructor

### Pages
| Route | Component | Description |
|---|---|---|
| `/instructor` | Dashboard | Shows total sessions, active, completed counts + trainee list |
| `/instructor/sessions` | Session List | Create session (select trainee + scenario), view all sessions with status badges |
| `/instructor/sessions/[id]` | Session Detail | View telemetry table, fill score + comments, submit evaluation |
| `/instructor/evaluations` | Evaluation History | List of issued evaluations with scores and dates |

### Layout
- Sidebar with menu: Dashboard, Sessions, Evaluations
- Top navbar with user name + email
- Sign out button at sidebar bottom

## Trainee

### Pages
| Route | Component | Description |
|---|---|---|
| `/trainee` | Dashboard | Shows total sessions, pending count, evaluations count |
| `/trainee/sessions` | Session List | View assigned sessions, click "Start" for pending, "View" for completed |
| `/trainee/sessions/[id]` | Session Detail | View status, telemetry, instructor notes, start/finish buttons |
| `/trainee/evaluations` | Evaluation History | List of received evaluations with scores, comments, color badges |

### Layout
- Sidebar with menu: Dashboard, Sessions, Evaluations
- Top navbar with user name + email
- Sign out button at sidebar bottom

## Shared Components

| Component | File | Used in |
|---|---|---|
| Sidebar | `components/layout/sidebar.tsx` | All role layouts |
| Navbar | `components/layout/navbar.tsx` | All role pages |
| Button | `components/ui/button.tsx` | All pages |
| Card | `components/ui/card.tsx` | All pages |
| Badge | `components/ui/badge.tsx` | Status indicators, scores |
| Input | `components/ui/input.tsx` | Forms |
| Select | `components/ui/select.tsx` | Dropdowns |
| Textarea | `components/ui/textarea.tsx` | Evaluation comments |

## Color Conventions

| Element | Variant |
|---|---|
| Session Pending | `badge variant="secondary"` |
| Session Active | `badge variant="success"` |
| Session Completed | `badge variant="default"` |
| Score ≥ 70% | `badge variant="success"` |
| Score 40-69% | `badge variant="warning"` |
| Score < 40% | `badge variant="destructive"` |
| Delete action | `button variant="destructive"` |
| Primary action | `button variant="default"` |
| Secondary action | `button variant="outline"` |
