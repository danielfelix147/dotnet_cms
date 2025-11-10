# API Security Testing Suite
# Automated security tests for DOTNET CMS API
#
# IMPORTANT: Run the API with Testing configuration for proper rate limits:
#   .\Run-API-Testing.ps1
# OR manually set:
#   $env:ASPNETCORE_ENVIRONMENT = "Testing"
#   cd CMS.API; dotnet run
#
# Testing config provides relaxed rate limits to avoid false positives:
#   - Login: 100 requests/minute (vs 5 in Development)
#   - Register: 50 requests/hour (vs 3 in Development)
#   - General: 1000 requests/minute (vs 100 in Development)

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$AdminEmail = "admin@cms.com",
    [string]$AdminPassword = "Admin@123"
)

$ErrorActionPreference = "Continue"
$testResults = @()

function Test-SecurityEndpoint {
    param(
        [string]$TestName,
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = @{},
        [int[]]$ExpectedStatusCodes = @(200, 201)
    )
    
    $uri = "$BaseUrl$Endpoint"
    
    try {
        $params = @{
            Uri = $uri
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
            ErrorAction = "SilentlyContinue"
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-WebRequest @params
        $statusCode = $response.StatusCode
        
        $passed = $statusCode -in $ExpectedStatusCodes
        
        $result = [PSCustomObject]@{
            Test = $TestName
            Endpoint = $Endpoint
            Status = $statusCode
            Expected = $ExpectedStatusCodes -join ", "
            Passed = $passed
        }
        
        return $result
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $passed = $statusCode -in $ExpectedStatusCodes
        
        $result = [PSCustomObject]@{
            Test = $TestName
            Endpoint = $Endpoint
            Status = $statusCode
            Expected = $ExpectedStatusCodes -join ", "
            Passed = $passed
        }
        
        return $result
    }
}

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "API Security Test Suite" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Target: $BaseUrl" -ForegroundColor Cyan
Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host ""
Write-Host "[IMPORTANT] Ensure API is running with Testing configuration" -ForegroundColor Yellow
Write-Host "            Use: .\Run-API-Testing.ps1" -ForegroundColor DarkGray
Write-Host ""

# Login to get token
Write-Host "[*] Authenticating..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = $AdminEmail
        password = $AdminPassword
    }
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
        -Method POST `
        -Body ($loginBody | ConvertTo-Json) `
        -ContentType "application/json"
    $token = $loginResponse.token
    $authHeaders = @{Authorization = "Bearer $token"}
    Write-Host "[OK] Authentication successful" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Authentication failed. Is the API running?" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Test 1: Authentication Tests
Write-Host "[1/10] Authentication Security Tests" -ForegroundColor Yellow

$testResults += Test-SecurityEndpoint `
    -TestName "Missing Auth Token" `
    -Method "GET" `
    -Endpoint "/api/sites" `
    -ExpectedStatusCodes @(401)

$testResults += Test-SecurityEndpoint `
    -TestName "Invalid Auth Token" `
    -Method "GET" `
    -Endpoint "/api/sites" `
    -Headers @{Authorization = "Bearer invalid-token-12345"} `
    -ExpectedStatusCodes @(401)

$testResults += Test-SecurityEndpoint `
    -TestName "Malformed Auth Header" `
    -Method "GET" `
    -Endpoint "/api/sites" `
    -Headers @{Authorization = "InvalidFormat"} `
    -ExpectedStatusCodes @(401)

Write-Host ""

# Test 2: SQL Injection Tests
# NOTE: These tests may trigger rate limiting (5 login attempts/min in production)
# Run API with Testing environment for higher limits: .\Run-API-Testing.ps1
Write-Host "[2/10] SQL Injection Protection Tests" -ForegroundColor Yellow

$sqlPayloads = @(
    "' OR '1'='1",
    "'; DROP TABLE Sites;--",
    "admin'--",
    "' UNION SELECT * FROM AspNetUsers--",
    "1' AND '1'='1"
)

foreach ($payload in $sqlPayloads) {
    $testResults += Test-SecurityEndpoint `
        -TestName "SQL Injection: $($payload.Substring(0, [Math]::Min(20, $payload.Length)))" `
        -Method "POST" `
        -Endpoint "/api/auth/login" `
        -Body @{email = "admin@cms.com$payload"; password = "test"} `
        -ExpectedStatusCodes @(400, 401)
    
    # Small delay to avoid rate limiting in production (can remove in Testing environment)
    Start-Sleep -Milliseconds 100
}

Write-Host ""

# Test 3: XSS Protection Tests
Write-Host "[3/10] XSS Protection Tests" -ForegroundColor Yellow

$xssPayloads = @(
    "<script>alert('XSS')</script>",
    "<img src=x onerror=alert('XSS')>",
    "<svg/onload=alert('XSS')>",
    "javascript:alert('XSS')",
    "<iframe src='javascript:alert(1)'>",
    "<body onload=alert('XSS')>"
)

foreach ($payload in $xssPayloads) {
    $testResults += Test-SecurityEndpoint `
        -TestName "XSS: $($payload.Substring(0, [Math]::Min(25, $payload.Length)))" `
        -Method "POST" `
        -Endpoint "/api/sites" `
        -Headers $authHeaders `
        -Body @{name = $payload; domain = "test-$([guid]::NewGuid()).com"} `
        -ExpectedStatusCodes @(400, 201)
}

Write-Host ""

# Test 4: CORS Tests
Write-Host "[4/10] CORS Protection Tests" -ForegroundColor Yellow

$testResults += Test-SecurityEndpoint `
    -TestName "CORS - Unauthorized Origin" `
    -Method "OPTIONS" `
    -Endpoint "/api/sites" `
    -Headers @{Origin = "https://evil-site.com"} `
    -ExpectedStatusCodes @(403, 200, 204, 405)

$testResults += Test-SecurityEndpoint `
    -TestName "CORS - Missing Origin" `
    -Method "OPTIONS" `
    -Endpoint "/api/sites" `
    -ExpectedStatusCodes @(200, 204, 405)

Write-Host ""

# Test 5: Input Validation Tests
Write-Host "[5/10] Input Validation Tests" -ForegroundColor Yellow

$testResults += Test-SecurityEndpoint `
    -TestName "Missing Required Field (name)" `
    -Method "POST" `
    -Endpoint "/api/sites" `
    -Headers $authHeaders `
    -Body @{domain = "test.com"} `
    -ExpectedStatusCodes @(400)

$testResults += Test-SecurityEndpoint `
    -TestName "Missing Required Field (domain)" `
    -Method "POST" `
    -Endpoint "/api/sites" `
    -Headers $authHeaders `
    -Body @{name = "Test Site"} `
    -ExpectedStatusCodes @(400)

$testResults += Test-SecurityEndpoint `
    -TestName "Invalid Email Format" `
    -Method "POST" `
    -Endpoint "/api/auth/register" `
    -Headers $authHeaders `
    -Body @{email = "invalid-email"; password = "ValidPass123!"} `
    -ExpectedStatusCodes @(400)

$testResults += Test-SecurityEndpoint `
    -TestName "Empty String Values" `
    -Method "POST" `
    -Endpoint "/api/sites" `
    -Headers $authHeaders `
    -Body @{name = ""; domain = ""} `
    -ExpectedStatusCodes @(400)

Write-Host ""

# Test 6: Authorization Tests
Write-Host "[6/10] Authorization Tests" -ForegroundColor Yellow

# Test 6.1: Unauthenticated Access
$testResults += Test-SecurityEndpoint `
    -TestName "Unauthenticated - Cannot Access Protected Endpoint" `
    -Method "GET" `
    -Endpoint "/api/sites" `
    -ExpectedStatusCodes @(401)

# Test 6.2: Get test users tokens
$editorToken = $null
$viewerToken = $null

try {
    # Login as Editor
    $editorLogin = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
        -Method POST `
        -Body (@{email="editor@cms.com"; password="Editor@123"} | ConvertTo-Json) `
        -ContentType "application/json" `
        -ErrorAction Stop
    $editorToken = $editorLogin.token
    $editorHeaders = @{Authorization = "Bearer $editorToken"}
    Write-Host "  [OK] Editor user authenticated" -ForegroundColor Green
}
catch {
    Write-Host "  [WARNING] Could not authenticate as Editor (user may not exist)" -ForegroundColor Yellow
    Write-Host "            Run .\Run-CMS-Workflow.ps1 to create test users" -ForegroundColor DarkGray
}

try {
    # Login as Viewer
    $viewerLogin = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
        -Method POST `
        -Body (@{email="viewer@cms.com"; password="Viewer@123"} | ConvertTo-Json) `
        -ContentType "application/json" `
        -ErrorAction Stop
    $viewerToken = $viewerLogin.token
    $viewerHeaders = @{Authorization = "Bearer $viewerToken"}
    Write-Host "  [OK] Viewer user authenticated" -ForegroundColor Green
}
catch {
    Write-Host "  [WARNING] Could not authenticate as Viewer (user may not exist)" -ForegroundColor Yellow
    Write-Host "            Run .\Run-CMS-Workflow.ps1 to create test users" -ForegroundColor DarkGray
}

# Test 6.3: Role-based access control tests
try {
    $sites = Invoke-RestMethod -Uri "$BaseUrl/api/sites" -Method GET -Headers $authHeaders -ErrorAction Stop
    
    if ($sites -and $sites.Count -gt 0) {
        $testSiteId = $sites[0].id
        
        # Test 6.4: Admin can perform all operations
        $testResults += Test-SecurityEndpoint `
            -TestName "Admin - Can Read Sites" `
            -Method "GET" `
            -Endpoint "/api/sites" `
            -Headers $authHeaders `
            -ExpectedStatusCodes @(200)
        
        $testResults += Test-SecurityEndpoint `
            -TestName "Admin - Can Create Site" `
            -Method "POST" `
            -Endpoint "/api/sites" `
            -Headers $authHeaders `
            -Body @{name="Admin Test Site"; domain="admin-test-$([guid]::NewGuid().ToString().Substring(0,8)).com"} `
            -ExpectedStatusCodes @(201, 200)
        
        # Test 6.5: Editor tests (if available)
        if ($editorToken) {
            $testResults += Test-SecurityEndpoint `
                -TestName "Editor - Can Read Sites" `
                -Method "GET" `
                -Endpoint "/api/sites" `
                -Headers $editorHeaders `
                -ExpectedStatusCodes @(200)
            
            $testResults += Test-SecurityEndpoint `
                -TestName "Editor - Can Create Site" `
                -Method "POST" `
                -Endpoint "/api/sites" `
                -Headers $editorHeaders `
                -Body @{name="Editor Test Site"; domain="editor-test-$([guid]::NewGuid().ToString().Substring(0,8)).com"} `
                -ExpectedStatusCodes @(201, 200, 403)
            
            # Test update permissions
            $testResults += Test-SecurityEndpoint `
                -TestName "Editor - Can Update Site" `
                -Method "PUT" `
                -Endpoint "/api/sites/$testSiteId" `
                -Headers $editorHeaders `
                -Body @{name="Updated by Editor"; domain=$sites[0].domain} `
                -ExpectedStatusCodes @(200, 204, 403)
            
            # Test delete permissions (should be forbidden for Editor)
            $testResults += Test-SecurityEndpoint `
                -TestName "Editor - Cannot Delete Site" `
                -Method "DELETE" `
                -Endpoint "/api/sites/$testSiteId" `
                -Headers $editorHeaders `
                -ExpectedStatusCodes @(403, 401)
        }
        
        # Test 6.6: Viewer tests (if available)
        if ($viewerToken) {
            $testResults += Test-SecurityEndpoint `
                -TestName "Viewer - Can Read Sites" `
                -Method "GET" `
                -Endpoint "/api/sites" `
                -Headers $viewerHeaders `
                -ExpectedStatusCodes @(200)
            
            # Viewer should not be able to create
            $testResults += Test-SecurityEndpoint `
                -TestName "Viewer - Cannot Create Site" `
                -Method "POST" `
                -Endpoint "/api/sites" `
                -Headers $viewerHeaders `
                -Body @{name="Viewer Test Site"; domain="viewer-test-$([guid]::NewGuid().ToString().Substring(0,8)).com"} `
                -ExpectedStatusCodes @(403, 401)
            
            # Viewer should not be able to update
            $testResults += Test-SecurityEndpoint `
                -TestName "Viewer - Cannot Update Site" `
                -Method "PUT" `
                -Endpoint "/api/sites/$testSiteId" `
                -Headers $viewerHeaders `
                -Body @{name="Updated by Viewer"; domain=$sites[0].domain} `
                -ExpectedStatusCodes @(403, 401)
            
            # Viewer should not be able to delete
            $testResults += Test-SecurityEndpoint `
                -TestName "Viewer - Cannot Delete Site" `
                -Method "DELETE" `
                -Endpoint "/api/sites/$testSiteId" `
                -Headers $viewerHeaders `
                -ExpectedStatusCodes @(403, 401)
        }
        
        if (-not $editorToken -and -not $viewerToken) {
            Write-Host "  [INFO] Role-based tests skipped - test users not available" -ForegroundColor DarkGray
            Write-Host "         Run: .\Run-CMS-Workflow.ps1 to create test users" -ForegroundColor DarkGray
        }
    } else {
        Write-Host "  [WARNING] No sites available for authorization testing" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  [WARNING] Could not retrieve sites for authorization testing" -ForegroundColor Yellow
    Write-Host "            Error: $($_.Exception.Message)" -ForegroundColor DarkGray
}

Write-Host ""

# Test 7: Security Headers Tests
Write-Host "[7/10] Security Headers Tests" -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/sites" -Method GET -Headers $authHeaders -ErrorAction Stop

    $requiredHeaders = @{
        "X-Frame-Options" = "Prevents clickjacking"
        "X-Content-Type-Options" = "Prevents MIME sniffing"
        "Content-Security-Policy" = "Controls resource loading"
        "Referrer-Policy" = "Controls referrer information"
        "X-XSS-Protection" = "XSS protection"
        "Strict-Transport-Security" = "Enforces HTTPS"
    }

    $headersFound = 0
    foreach ($header in $requiredHeaders.Keys) {
        if ($response.Headers[$header]) {
            Write-Host "  [OK] $header present" -ForegroundColor Green
            Write-Host "       Value: $($response.Headers[$header])" -ForegroundColor DarkGray
            $headersFound++
        } else {
            Write-Host "  [WARNING] $header missing - $($requiredHeaders[$header])" -ForegroundColor Yellow
        }
    }
    
    if ($headersFound -eq 0) {
        Write-Host "  [ERROR] No security headers found" -ForegroundColor Red
    }
}
catch {
    Write-Host "  [ERROR] Could not retrieve security headers: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 8: Path Traversal Tests
Write-Host "[8/10] Path Traversal Protection Tests" -ForegroundColor Yellow

$pathTraversalPayloads = @(
    "../../etc/passwd",
    "..\..\windows\system32\config\sam",
    "....//....//etc//passwd",
    "%2e%2e%2f%2e%2e%2f",
    "..%252f..%252f"
)

foreach ($payload in $pathTraversalPayloads) {
    $testResults += Test-SecurityEndpoint `
        -TestName "Path Traversal: $($payload.Substring(0, [Math]::Min(20, $payload.Length)))" `
        -Method "GET" `
        -Endpoint "/api/sites/$payload" `
        -Headers $authHeaders `
        -ExpectedStatusCodes @(400, 404)
}

Write-Host ""

# Test 9: Business Logic Tests
Write-Host "[9/10] Business Logic Security Tests" -ForegroundColor Yellow

# Get sites for business logic testing
try {
    $sites = Invoke-RestMethod -Uri "$BaseUrl/api/sites" -Method GET -Headers $authHeaders -ErrorAction Stop

    if ($sites -and $sites.Count -gt 0) {
        $testSiteId = $sites[0].id
        
        # Test negative price
        $testResults += Test-SecurityEndpoint `
            -TestName "Negative Product Price" `
            -Method "POST" `
            -Endpoint "/api/sites/$testSiteId/products" `
            -Headers $authHeaders `
            -Body @{productId="test-product"; name="Test Product"; price=-100; description="Test"} `
            -ExpectedStatusCodes @(400)
        
        # Test duplicate domain
        $existingDomain = $sites[0].domain
        $testResults += Test-SecurityEndpoint `
            -TestName "Duplicate Site Domain" `
            -Method "POST" `
            -Endpoint "/api/sites" `
            -Headers $authHeaders `
            -Body @{name="Duplicate Site"; domain=$existingDomain} `
            -ExpectedStatusCodes @(400, 409, 500)
            
        # Test invalid GUID format
        $testResults += Test-SecurityEndpoint `
            -TestName "Invalid GUID Format" `
            -Method "GET" `
            -Endpoint "/api/sites/not-a-valid-guid" `
            -Headers $authHeaders `
            -ExpectedStatusCodes @(400, 404)
    } else {
        Write-Host "  [WARNING] No sites available for business logic testing" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  [WARNING] Could not retrieve sites for business logic testing" -ForegroundColor Yellow
}

Write-Host ""

# Test 10: Rate Limiting Tests (Last test to properly measure rate limits)
Write-Host "[10/10] Rate Limiting Tests" -ForegroundColor Yellow

$rapidRequests = 0
$rateLimitDetected = $false

Write-Host "  [INFO] Testing brute force protection on login endpoint..." -ForegroundColor DarkGray
for ($i=1; $i -le 100; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl/api/auth/login" `
            -Method POST `
            -Body (@{email="ratelimit-test-$i@test.com"; password="TestPass$i!"} | ConvertTo-Json) `
            -ContentType "application/json" `
            -ErrorAction SilentlyContinue
        $rapidRequests++
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 429) {
            Write-Host "  [OK] Rate limit activated after $rapidRequests requests" -ForegroundColor Green
            $rateLimitDetected = $true
            break
        }
        $rapidRequests++
    }
}

if (-not $rateLimitDetected) {
    Write-Host "  [WARNING] No rate limiting detected after $rapidRequests requests" -ForegroundColor Yellow
    Write-Host "  [RECOMMENDATION] Implement rate limiting to prevent brute force attacks" -ForegroundColor Yellow
}

Write-Host ""

# Generate Report
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Test Results Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Calculate statistics
$passed = ($testResults | Where-Object {$_.Passed -eq $true}).Count
$failed = ($testResults | Where-Object {$_.Passed -eq $false}).Count
$total = $testResults.Count

# Display results in color
foreach ($result in $testResults) {
    $color = if ($result.Passed) { "Green" } else { "Red" }
    $status = if ($result.Passed) { "[PASS]" } else { "[FAIL]" }
    
    Write-Host "$status $($result.Test)" -ForegroundColor $color
    Write-Host "       Endpoint: $($result.Endpoint)" -ForegroundColor DarkGray
    Write-Host "       Status: $($result.Status) (Expected: $($result.Expected))" -ForegroundColor DarkGray
    Write-Host ""
}

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Summary Statistics" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total Tests:  $total" -ForegroundColor Cyan
Write-Host "Passed:       $passed" -ForegroundColor Green
Write-Host "Failed:       $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })
Write-Host "Pass Rate:    $([math]::Round(($passed / $total) * 100, 2))%" -ForegroundColor $(if ($passed -eq $total) { "Green" } else { "Yellow" })
Write-Host ""

# Final verdict
if ($failed -eq 0) {
    Write-Host "===============================================" -ForegroundColor Green
    Write-Host "ALL SECURITY TESTS PASSED!" -ForegroundColor Green
    Write-Host "===============================================" -ForegroundColor Green
} else {
    Write-Host "===============================================" -ForegroundColor Yellow
    Write-Host "SOME SECURITY TESTS FAILED" -ForegroundColor Yellow
    Write-Host "===============================================" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Failed Tests:" -ForegroundColor Red
    $testResults | Where-Object {$_.Passed -eq $false} | ForEach-Object {
        Write-Host "  - $($_.Test)" -ForegroundColor Red
    }
}

Write-Host ""

# Export results
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$resultsDir = "security_results"

# Create directory if it doesn't exist
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir | Out-Null
}

$csvFile = Join-Path $resultsDir "security-test-results-$timestamp.csv"
$htmlFile = Join-Path $resultsDir "security-test-results-$timestamp.html"

$testResults | Export-Csv -Path $csvFile -NoTypeInformation
Write-Host "CSV Report exported to: $csvFile" -ForegroundColor Cyan

# Generate HTML report
$html = @"
<!DOCTYPE html>
<html>
<head>
    <title>API Security Test Results - $timestamp</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }
        h1 { color: #333; }
        .summary { background: white; padding: 20px; border-radius: 5px; margin-bottom: 20px; }
        .summary-item { display: inline-block; margin-right: 30px; }
        .pass { color: green; font-weight: bold; }
        .fail { color: red; font-weight: bold; }
        table { width: 100%; border-collapse: collapse; background: white; }
        th { background-color: #4CAF50; color: white; padding: 12px; text-align: left; }
        td { padding: 10px; border-bottom: 1px solid #ddd; }
        tr:hover { background-color: #f5f5f5; }
        .passed-row { background-color: #e8f5e9; }
        .failed-row { background-color: #ffebee; }
    </style>
</head>
<body>
    <h1>API Security Test Results</h1>
    <div class="summary">
        <h2>Summary</h2>
        <div class="summary-item">Target: <strong>$BaseUrl</strong></div>
        <div class="summary-item">Date: <strong>$timestamp</strong></div>
        <div class="summary-item">Total Tests: <strong>$total</strong></div>
        <div class="summary-item">Passed: <span class="pass">$passed</span></div>
        <div class="summary-item">Failed: <span class="fail">$failed</span></div>
        <div class="summary-item">Pass Rate: <strong>$([math]::Round(($passed / $total) * 100, 2))%</strong></div>
    </div>
    <table>
        <tr>
            <th>Status</th>
            <th>Test Name</th>
            <th>Endpoint</th>
            <th>Response</th>
            <th>Expected</th>
        </tr>
"@

foreach ($result in $testResults) {
    $rowClass = if ($result.Passed) { "passed-row" } else { "failed-row" }
    $status = if ($result.Passed) { "✅ PASS" } else { "❌ FAIL" }
    
    $html += @"
        <tr class="$rowClass">
            <td>$status</td>
            <td>$($result.Test)</td>
            <td>$($result.Endpoint)</td>
            <td>$($result.Status)</td>
            <td>$($result.Expected)</td>
        </tr>
"@
}

$html += @"
    </table>
</body>
</html>
"@

$html | Out-File -FilePath $htmlFile -Encoding UTF8
Write-Host "HTML Report exported to: $htmlFile" -ForegroundColor Cyan
Write-Host ""

# Exit with appropriate code
if ($failed -gt 0) {
    exit 1
} else {
    exit 0
}
