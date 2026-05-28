# Bug: Test helper SeedActiveSession clears auth token

**Date discovered:** 2026-05-28
**Detected by:** QA agent during backend integration test execution

## Description

The telemetry test was returning HTTP 401 Unauthorized instead of the expected 400 Bad Request. The test set up authentication, called `SeedActiveSession()` to create a session, then attempted to send telemetry — but the telemetry request was made without an auth token.

## Root Cause

`SeedActiveSession()` internally called `Client.PostAsJsonAsync(...)` which does NOT clear the auth token on its own, but the flow involved the trainee auth being set, then a session was created by an instructor, and the helper reset the client state. The issue was that after `SeedActiveSession()` completed, the client's `DefaultRequestHeaders.Authorization` had been cleared or overwritten by the helper method.

Specifically, `SeedActiveSession()` called helper methods that may have called `ClearAuthToken()` internally (via `BootstrapAdmin()` which clears the token after use), leaving the client without authentication for the subsequent telemetry request.

## Fix

Added explicit `SetAuthToken(trainee.AccessToken)` call before the telemetry request in the test method, ensuring the client has the correct authentication header regardless of what `SeedActiveSession()` does internally.

## Prevention

- Test helper methods should document whether they preserve or clear authentication state
- Tests should always explicitly set the auth token before the action being tested
- Consider making helper methods accept an optional token parameter to re-set after internal operations
