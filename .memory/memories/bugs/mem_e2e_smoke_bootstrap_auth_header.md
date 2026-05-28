# Bug: E2E smoke test was not autonomous and missed Authorization header assignment

**Date discovered:** 2026-05-28
**Detected by:** Consistency audit before applying recommended fixes

## Description

The E2E smoke test could not reliably execute the complete happy path because the first admin user requires an existing organization, but the script only attempted to register against a fixed organization ID and skipped most tests when that org did not exist.

The script also had a PowerShell variable bug:

```powershell
headers["Authorization"] = "Bearer $Token"
```

This omits the `$` prefix, so the header assignment is invalid.

## Root Cause

- The API intentionally requires an organization to exist before `/api/auth/register` can create a user.
- Admin organization creation requires an authenticated Admin user.
- The script did not seed the first organization before registering the bootstrap Admin.
- The Authorization header line referenced `headers` instead of `$headers`.

## Fix

- Rewrote `tests/e2e/smoke-test.ps1` to seed the bootstrap organization through Docker Compose PostgreSQL using `psql`.
- Added `-SkipBootstrapSeed` for pre-seeded environments.
- Corrected Authorization header assignment to `$headers["Authorization"]`.
- Added unique test emails per run.
- Added fail-fast validation for expected IDs, tokens, statuses, and telemetry counts.
- Validated PowerShell syntax with the parser.

## Prevention

- E2E tests must prepare their own required data or explicitly fail with instructions.
- Avoid smoke tests that silently skip core flows.
- Validate PowerShell scripts syntactically before marking them ready.
