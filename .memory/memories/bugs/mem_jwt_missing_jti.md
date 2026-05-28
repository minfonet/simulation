# Bug: JWT refresh token identical to original (missing jti claim)

**Date discovered:** 2026-05-28
**Detected by:** QA agent during backend integration test execution

## Description

The test `Refresh_With_Valid_Token_Returns_New_Tokens` failed because the refreshed access token was identical to the original. The assertion `Assert.NotEqual(loginAuth.AccessToken, refreshedAuth.AccessToken)` failed.

## Root Cause

The `JwtService.cs` was not including a unique `jti` (JWT ID) claim in the token. Without a `jti`, the JWT token generation produced identical tokens for the same user because they had the same claims and the same security stamp. The token was purely deterministic based on claims.

The `CreateToken` method in `JwtService` had:
```csharp
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Email, user.Email),
    new Claim(JwtRegisteredClaimNames.GivenName, user.Name),
    new Claim(ClaimTypes.Role, user.Role.ToString()),
    new Claim("organizationId", user.OrganizationId.ToString()),
};
```

## Fix

Added `jti` claim with a new GUID:
```csharp
new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
```

This ensures each generated token has a unique identifier, making every refresh produce a different token even for the same user.

## Prevention

Always include `jti` (JWT ID) claim in JWT tokens to ensure token uniqueness. This is also important for token revocation scenarios.
