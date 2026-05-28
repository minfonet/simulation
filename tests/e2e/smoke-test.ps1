<#
.SYNOPSIS
    E2E smoke test for the driving simulation API.

.DESCRIPTION
    Exercises the complete happy path:
    health -> seed bootstrap org -> register admin -> create org -> invite users ->
    login -> create session -> start session -> send telemetry -> finish -> evaluate ->
    retrieve telemetry -> verify auth/me.

    By default the script seeds the required bootstrap organization through the
    Docker Compose PostgreSQL service. Use -SkipBootstrapSeed only when the target
    database is already seeded.

.PARAMETER BaseUrl
    API base URL. Default: http://localhost:8080

.PARAMETER BootstrapOrgId
    Organization ID used only to register the first bootstrap admin.

.PARAMETER ComposeFile
    Path to docker-compose.yml. Default: <repo-root>/docker/docker-compose.yml

.PARAMETER SkipBootstrapSeed
    Skip the Docker Compose database seed step.

.EXAMPLE
    ./tests/e2e/smoke-test.ps1

.EXAMPLE
    ./tests/e2e/smoke-test.ps1 -BaseUrl "http://localhost:5000" -SkipBootstrapSeed
#>

param(
    [string]$BaseUrl = "http://localhost:8080",
    [string]$BootstrapOrgId = "00000000-0000-0000-0000-000000000001",
    [string]$ComposeFile = "",
    [switch]$SkipBootstrapSeed
)

$ErrorActionPreference = "Stop"
$passCount = 0
$failCount = 0
$projectRoot = (Resolve-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath "../..")).Path

if ([string]::IsNullOrWhiteSpace($ComposeFile)) {
    $ComposeFile = Join-Path -Path $projectRoot -ChildPath "docker/docker-compose.yml"
}

function Write-Step {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor Cyan
}

function Write-Pass {
    param([string]$Message)
    Write-Host "  [PASS] $Message" -ForegroundColor Green
    $script:passCount++
}

function Write-Fail {
    param([string]$Message)
    Write-Host "  [FAIL] $Message" -ForegroundColor Red
    $script:failCount++
}

function Assert-Present {
    param(
        [object]$Value,
        [string]$Name
    )

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace($Value.ToString())) {
        throw "Missing expected value: $Name"
    }
}

function Invoke-Api {
    param(
        [ValidateSet("GET", "POST", "PUT", "DELETE")]
        [string]$Method = "GET",
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [object]$Body = $null,
        [string]$Token = ""
    )

    $headers = @{
        Accept = "application/json"
    }

    if (-not [string]::IsNullOrWhiteSpace($Token)) {
        $headers["Authorization"] = "Bearer $Token"
    }

    $params = @{
        Method = $Method
        Uri = "$BaseUrl$Path"
        Headers = $headers
    }

    if ($null -ne $Body) {
        $headers["Content-Type"] = "application/json"
        $params["Body"] = ($Body | ConvertTo-Json -Depth 10)
    }

    try {
        Invoke-RestMethod @params
    }
    catch {
        $statusCode = "n/a"
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        $errorMessage = $_.Exception.Message
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
            $errorMessage = $_.ErrorDetails.Message
        }

        throw "HTTP $statusCode for $Method $Path - $errorMessage"
    }
}

function Invoke-BootstrapSeed {
    if ($SkipBootstrapSeed) {
        Write-Host "  Skipping bootstrap seed because -SkipBootstrapSeed was provided." -ForegroundColor Yellow
        return
    }

    if (-not (Test-Path -LiteralPath $ComposeFile)) {
        throw "Compose file not found: $ComposeFile. Pass -ComposeFile or use -SkipBootstrapSeed with a pre-seeded DB."
    }

    $sql = "INSERT INTO `"Organizations`" (`"Id`", `"Name`", `"CreatedAt`") VALUES ('$BootstrapOrgId', 'Smoke Bootstrap Org', NOW()) ON CONFLICT (`"Id`") DO NOTHING;"
    $dockerArgs = @("compose", "-f", $ComposeFile, "exec", "-T", "db", "psql", "-U", "postgres", "-d", "simulation", "-c", $sql)

    & docker @dockerArgs | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to seed bootstrap organization through Docker Compose PostgreSQL service."
    }

    Write-Pass "Bootstrap organization exists: $BootstrapOrgId"
}

try {
    $runId = Get-Date -Format "yyyyMMddHHmmssfff"

    Write-Step "1. Health Check"
    $health = Invoke-Api -Method GET -Path "/health"
    if ($health.status -ne "healthy") {
        throw "Health endpoint returned unexpected status: $($health.status)"
    }
    Write-Pass "Health endpoint returns healthy"

    Write-Step "2. Bootstrap Organization"
    Invoke-BootstrapSeed

    Write-Step "3. Register Bootstrap Admin"
    $adminEmail = "smoke-admin-$runId@test.com"
    $adminPassword = "SmokeTestPass123!"
    $bootstrapAdmin = Invoke-Api -Method POST -Path "/api/auth/register" -Body @{
        email = $adminEmail
        password = $adminPassword
        name = "Smoke Test Admin"
        role = "Admin"
        organizationId = $BootstrapOrgId
    }
    Assert-Present $bootstrapAdmin.accessToken "admin accessToken"
    $adminToken = $bootstrapAdmin.accessToken
    Write-Pass "Bootstrap admin registered: $adminEmail"

    Write-Step "4. Create Organization"
    $org = Invoke-Api -Method POST -Path "/api/admin/organizations" -Body @{
        name = "Smoke Test Org $runId"
    } -Token $adminToken
    Assert-Present $org.id "organization id"
    $orgId = $org.id
    Write-Pass "Organization created: $($org.name) ($orgId)"

    Write-Step "5. Invite Instructor and Trainee"
    $instructorEmail = "instructor-$runId@test.com"
    $instructorPassword = "InstructorPass123!"
    $instructor = Invoke-Api -Method POST -Path "/api/admin/organizations/$orgId/users" -Body @{
        email = $instructorEmail
        password = $instructorPassword
        name = "Smoke Instructor"
        role = "Instructor"
    } -Token $adminToken
    Assert-Present $instructor.id "instructor id"
    Write-Pass "Instructor created: $instructorEmail"

    $traineeEmail = "trainee-$runId@test.com"
    $traineePassword = "TraineePass123!"
    $trainee = Invoke-Api -Method POST -Path "/api/admin/organizations/$orgId/users" -Body @{
        email = $traineeEmail
        password = $traineePassword
        name = "Smoke Trainee"
        role = "Trainee"
    } -Token $adminToken
    Assert-Present $trainee.id "trainee id"
    Write-Pass "Trainee created: $traineeEmail"

    Write-Step "6. Login as Instructor and Trainee"
    $instructorLogin = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
        email = $instructorEmail
        password = $instructorPassword
    }
    Assert-Present $instructorLogin.accessToken "instructor accessToken"
    Assert-Present $instructorLogin.userId "instructor userId"
    $instructorToken = $instructorLogin.accessToken
    Write-Pass "Instructor logged in"

    $traineeLogin = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
        email = $traineeEmail
        password = $traineePassword
    }
    Assert-Present $traineeLogin.accessToken "trainee accessToken"
    Assert-Present $traineeLogin.userId "trainee userId"
    $traineeToken = $traineeLogin.accessToken
    $traineeId = $traineeLogin.userId
    Write-Pass "Trainee logged in"

    Write-Step "7. Create Session"
    $session = Invoke-Api -Method POST -Path "/api/instructor/sessions" -Body @{
        traineeId = $traineeId.ToString()
        scenario = "smoke-test"
    } -Token $instructorToken
    Assert-Present $session.id "session id"
    $sessionId = $session.id
    Write-Pass "Session created: $sessionId (status: $($session.status))"

    Write-Step "8. Start Session"
    $startResult = Invoke-Api -Method POST -Path "/api/trainee/sessions/$sessionId/start" -Token $traineeToken
    if ($startResult.status -ne "Active") {
        throw "Expected session status Active, got $($startResult.status)"
    }
    Write-Pass "Session started"

    Write-Step "9. Send Telemetry"
    $telemetry = Invoke-Api -Method POST -Path "/api/telemetry" -Body @{
        sessionId = $sessionId.ToString()
        points = @(
            @{
                timestamp = (Get-Date).ToUniversalTime().ToString("o")
                speed = 45.5
                steeringAngle = 0.15
                positionX = 10.0
                positionY = 0.0
                positionZ = 20.0
                collision = $false
            },
            @{
                timestamp = (Get-Date).ToUniversalTime().AddSeconds(1).ToString("o")
                speed = 55.0
                steeringAngle = -0.10
                positionX = 15.0
                positionY = 0.5
                positionZ = 25.0
                collision = $false
            }
        )
    } -Token $traineeToken
    if ($telemetry.ingested -ne 2) {
        throw "Expected 2 telemetry records ingested, got $($telemetry.ingested)"
    }
    Write-Pass "Telemetry ingested: $($telemetry.ingested) points"

    Write-Step "10. Finish Session"
    $finishResult = Invoke-Api -Method POST -Path "/api/trainee/sessions/$sessionId/finish" -Token $traineeToken
    if ($finishResult.status -ne "Completed") {
        throw "Expected session status Completed, got $($finishResult.status)"
    }
    Write-Pass "Session finished"

    Write-Step "11. Evaluate Session"
    Invoke-Api -Method POST -Path "/api/instructor/sessions/$sessionId/evaluate" -Body @{
        score = 92.5
        comments = "Good performance in smoke test"
    } -Token $instructorToken | Out-Null
    Write-Pass "Session evaluated"

    Write-Step "12. Retrieve Telemetry"
    $records = Invoke-Api -Method GET -Path "/api/telemetry/session/$sessionId" -Token $traineeToken
    $recordCount = @($records).Count
    if ($recordCount -lt 2) {
        throw "Expected at least 2 telemetry records, got $recordCount"
    }
    Write-Pass "Retrieved $recordCount telemetry records"

    Write-Step "13. Verify Auth/Me"
    $me = Invoke-Api -Method GET -Path "/api/auth/me" -Token $instructorToken
    if ($me.email -ne $instructorEmail) {
        throw "Expected auth/me email $instructorEmail, got $($me.email)"
    }
    Write-Pass "Auth/me returns instructor identity"
}
catch {
    Write-Fail $_.Exception.Message
}
finally {
    Write-Step "RESULTS"
    Write-Host "Tests passed: $passCount" -ForegroundColor Green
    Write-Host "Tests failed: $failCount" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })
    Write-Host "Total: $($passCount + $failCount)" -ForegroundColor Cyan
}

if ($failCount -gt 0) {
    exit 1
}

exit 0
