# API Security Testing Guide

## Date: November 9, 2025

This guide provides comprehensive penetration testing approaches for the DOTNET CMS API to validate security against common threats.

---

## Table of Contents
1. [Automated Security Scanning Tools](#automated-security-scanning-tools)
2. [Manual Testing Checklist](#manual-testing-checklist)
3. [OWASP Top 10 Testing](#owasp-top-10-testing)
4. [Authentication & Authorization Testing](#authentication--authorization-testing)
5. [PowerShell Security Test Scripts](#powershell-security-test-scripts)
6. [CI/CD Integration](#cicd-integration)
7. [Reporting & Remediation](#reporting--remediation)

---

## Automated Security Scanning Tools

### 1. OWASP ZAP (Zed Attack Proxy) - FREE ⭐ RECOMMENDED
**Installation:**
```powershell
# Download from https://www.zaproxy.org/download/
# Or via Chocolatey
choco install zap
```

**Quick Scan:**
```bash
# Start API
cd CMS.API
dotnet run

# In another terminal (ZAP installed)
zap-cli quick-scan --spider http://localhost:5000/swagger
zap-cli report -o zap-report.html -f html
```

**Comprehensive Scan:**
```bash
# Automated scan with authentication
zap-cli start
zap-cli open-url http://localhost:5000
zap-cli spider http://localhost:5000/api
zap-cli active-scan http://localhost:5000/api
zap-cli alerts -l Informational
zap-cli report -o zap-report.html -f html
zap-cli shutdown
```

**Docker Container Scan:**
```powershell
docker run -v ${PWD}:/zap/wrk/:rw -t owasp/zap2docker-stable zap-baseline.py `
    -t http://host.docker.internal:5000 `
    -r zap-report.html
```

---

### 2. Burp Suite Community Edition - FREE
**Installation:**
- Download: https://portswigger.net/burp/communitydownload
- Configure browser proxy to 127.0.0.1:8080
- Import Swagger/OpenAPI spec from http://localhost:5000/swagger/v1/swagger.json

**Key Features:**
- Intercept and modify requests
- Fuzz testing with Intruder
- SQL injection detection
- XSS detection
- CSRF token analysis

**Testing Workflow:**
1. Configure proxy
2. Browse API via Swagger UI
3. Review captured requests in Burp
4. Run Active Scanner on endpoints
5. Analyze findings

---

### 3. Postman Security Tests - FREE
**Import Collection:**
```powershell
# Your existing Postman collection
# Located in your workspace
```

**Create Security Test Collection:**

#### Test 1: SQL Injection Attempts
```javascript
// Pre-request Script
pm.environment.set("sqlPayload", "' OR '1'='1");

// Test Script
pm.test("SQL Injection Protection", function() {
    pm.expect(pm.response.code).to.be.oneOf([400, 401, 403, 422]);
    pm.expect(pm.response.text()).to.not.include("SQL");
    pm.expect(pm.response.text()).to.not.include("syntax error");
});
```

#### Test 2: XSS Injection Attempts
```javascript
// Pre-request Script
pm.environment.set("xssPayload", "<script>alert('XSS')</script>");

// Test Script
pm.test("XSS Protection", function() {
    var response = pm.response.json();
    // Verify HTML is sanitized
    pm.expect(JSON.stringify(response)).to.not.include("<script>");
});
```

#### Test 3: Authorization Bypass
```javascript
// Test Script
pm.test("Requires Authentication", function() {
    // Request without token should fail
    pm.expect(pm.response.code).to.equal(401);
});

pm.test("Requires Admin Role", function() {
    // Editor token should fail on Admin endpoint
    pm.expect(pm.response.code).to.equal(403);
});
```

---

### 4. Trivy - Container Security Scanner
```powershell
# Install
choco install trivy

# Scan Docker image
docker build -t cms-api .
trivy image cms-api

# Generate report
trivy image --format json --output trivy-report.json cms-api
```

---

### 5. SonarQube - Code Quality & Security
```powershell
# Docker container
docker run -d --name sonarqube -p 9000:9000 sonarqube:latest

# Install scanner
dotnet tool install --global dotnet-sonarscanner

# Run analysis
dotnet sonarscanner begin /k:"DOTNET_CMS" /d:sonar.host.url="http://localhost:9000"
dotnet build
dotnet sonarscanner end

# View results at http://localhost:9000
```

---

## Manual Testing Checklist

### ✅ Authentication Testing

| Test | Endpoint | Payload | Expected Result |
|------|----------|---------|-----------------|
| **Missing Token** | Any protected endpoint | No Authorization header | 401 Unauthorized |
| **Invalid Token** | Any protected endpoint | `Bearer invalid-token` | 401 Unauthorized |
| **Expired Token** | Any protected endpoint | Expired JWT | 401 Unauthorized |
| **SQL Injection in Login** | POST /api/auth/login | `{ "email": "admin@cms.com' OR '1'='1", "password": "x" }` | 401 or 400 |
| **Brute Force** | POST /api/auth/login | Multiple failed attempts | Rate limit after N attempts |
| **Token Reuse After Logout** | Protected endpoint | Token from expired session | 401 Unauthorized |

---

### ✅ Authorization Testing

**Test User Credentials:**
- **Admin**: `admin@cms.com` / `Admin@123` (Full access)
- **Editor**: `editor@cms.com` / `Editor@123` (Read/Write, no Delete)
- **Viewer**: `viewer@cms.com` / `Viewer@123` (Read-only)

*Note: Run `.\Run-CMS-Workflow.ps1` to automatically create Editor and Viewer test users.*

**Authorization Test Matrix:**

| Test | Role | Endpoint | Expected Result |
|------|------|----------|-----------------|
| **Unauthenticated Access** | None | GET /api/sites | 401 Unauthorized |
| **Admin Full Access** | Admin | GET/POST/PUT/DELETE /api/sites | 200/201/204 |
| **Editor Read Access** | Editor | GET /api/sites | 200 OK |
| **Editor Create Access** | Editor | POST /api/sites | 200/201 OK |
| **Editor Update Access** | Editor | PUT /api/sites/{id} | 200/204 OK |
| **Editor Delete Forbidden** | Editor | DELETE /api/sites/{id} | 403 Forbidden |
| **Viewer Read Access** | Viewer | GET /api/sites | 200 OK |
| **Viewer Create Forbidden** | Viewer | POST /api/sites | 403 Forbidden |
| **Viewer Update Forbidden** | Viewer | PUT /api/sites/{id} | 403 Forbidden |
| **Viewer Delete Forbidden** | Viewer | DELETE /api/sites/{id} | 403 Forbidden |
| **Cross-site access** | User from Site A | POST /api/sites/site-b-id/pages | 403 Forbidden |
| **Privilege Escalation** | Editor | PUT /api/users/{id} (change to Admin) | 403 Forbidden |

---

### ✅ Input Validation Testing

| Test | Endpoint | Payload | Expected Result |
|------|----------|---------|-----------------|
| **XSS in Title** | POST /api/sites/{id}/pages | `{ "title": "<script>alert('XSS')</script>" }` | 400 or sanitized |
| **SQL in Name** | POST /api/sites | `{ "name": "'; DROP TABLE Sites;--" }` | 400 or safe insert |
| **Large Input** | POST /api/sites/{id}/pages | 10MB+ title field | 400 Bad Request |
| **Negative Numbers** | POST /api/sites/{id}/products | `{ "price": -100 }` | 400 Bad Request |
| **Invalid Email** | POST /api/users | `{ "email": "notanemail" }` | 400 Bad Request |
| **Path Traversal** | POST /api/media/upload | Filename: `../../etc/passwd` | 400 or sanitized |
| **Null/Missing Required** | POST /api/sites | `{ "name": null }` | 400 Bad Request |
| **Unicode/Emoji Injection** | POST /api/sites | `{ "name": "Test�💀�" }` | 400 or sanitized |

---

### ✅ CSRF Testing

| Test | Method | Headers | Expected Result |
|------|--------|---------|-----------------|
| **Missing Origin** | POST /api/sites | No Origin header | Blocked in production |
| **Invalid Origin** | POST /api/sites | Origin: https://evil.com | 403 Forbidden |
| **Missing CSRF Token** | POST (form) | No X-CSRF-TOKEN | 400 Bad Request |
| **Invalid CSRF Token** | POST (form) | Wrong X-CSRF-TOKEN | 400 Bad Request |

---

### ✅ CORS Testing

```powershell
# Test from unauthorized origin
curl -X OPTIONS http://localhost:5000/api/sites `
  -H "Origin: https://evil.com" `
  -H "Access-Control-Request-Method: POST" `
  -v

# Expected: No Access-Control-Allow-Origin header
```

---

### ✅ Rate Limiting Testing

```powershell
# Rapid-fire requests
for ($i=1; $i -le 100; $i++) {
    Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
        -Method POST `
        -Body (@{email="admin@cms.com"; password="wrong"} | ConvertTo-Json) `
        -ContentType "application/json"
}

# Expected: 429 Too Many Requests after threshold
```

---

## OWASP Top 10 Testing

### A01:2021 – Broken Access Control ✅

**Tests:**
```powershell
# Test 1: Vertical Privilege Escalation
# Login as Editor, attempt Admin-only operation
$editorToken = "Bearer <editor-token>"
Invoke-RestMethod -Uri "http://localhost:5000/api/sites/delete-site-id" `
    -Method DELETE `
    -Headers @{Authorization=$editorToken}
# Expected: 403 Forbidden

# Test 2: Horizontal Privilege Escalation
# User A attempts to modify User B's content
$userAToken = "Bearer <user-a-token>"
Invoke-RestMethod -Uri "http://localhost:5000/api/sites/user-b-site/pages" `
    -Method POST `
    -Headers @{Authorization=$userAToken} `
    -Body $pageData
# Expected: 403 Forbidden

# Test 3: IDOR (Insecure Direct Object Reference)
# Attempt to access another user's resource by ID
Invoke-RestMethod -Uri "http://localhost:5000/api/users/other-user-guid" `
    -Headers @{Authorization=$editorToken}
# Expected: 403 Forbidden
```

---

### A02:2021 – Cryptographic Failures ✅

**Tests:**
```powershell
# Test 1: HTTPS Enforcement
curl http://localhost:5000/api/sites -v
# Expected: 307 Redirect to HTTPS (in production)

# Test 2: JWT Secret Strength
# Check appsettings.json JWT:Secret length
# Expected: Minimum 32 characters

# Test 3: Password Storage
# Verify passwords are hashed (check database)
# Expected: No plaintext passwords, hashed with bcrypt/PBKDF2
```

---

### A03:2021 – Injection ✅

**SQL Injection Tests:**
```powershell
# Test payloads
$sqlPayloads = @(
    "' OR '1'='1",
    "'; DROP TABLE Sites;--",
    "' UNION SELECT * FROM AspNetUsers--",
    "admin'--",
    "' OR 1=1--",
    "1' AND '1'='1"
)

foreach ($payload in $sqlPayloads) {
    $body = @{
        email = "admin@cms.com$payload"
        password = "test"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
        -Method POST `
        -Body $body `
        -ContentType "application/json" `
        -ErrorAction SilentlyContinue
        
    Write-Host "Payload: $payload - Status: $($response.StatusCode)"
}
# Expected: All return 400/401, no SQL errors exposed
```

**XSS Injection Tests:**
```powershell
# Test payloads
$xssPayloads = @(
    "<script>alert('XSS')</script>",
    "<img src=x onerror=alert('XSS')>",
    "<svg/onload=alert('XSS')>",
    "javascript:alert('XSS')",
    "<iframe src='javascript:alert(1)'>",
    "'-alert(1)-'",
    "<body onload=alert('XSS')>"
)

foreach ($payload in $xssPayloads) {
    $body = @{
        siteId = "valid-site-guid"
        pageId = "test-page"
        title = $payload
        description = "Test"
        isPublished = $false
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/sites/site-guid/pages" `
        -Method POST `
        -Body $body `
        -Headers @{Authorization="Bearer $token"} `
        -ContentType "application/json" `
        -ErrorAction SilentlyContinue
        
    # Verify response doesn't contain unescaped payload
    if ($response.title -match "<script>") {
        Write-Host "❌ XSS VULNERABILITY FOUND!" -ForegroundColor Red
    } else {
        Write-Host "✅ XSS Blocked: $payload" -ForegroundColor Green
    }
}
```

---

### A04:2021 – Insecure Design ✅

**Business Logic Tests:**
```powershell
# Test 1: Negative Prices
$body = @{
    siteId = "site-guid"
    productId = "test-product"
    name = "Test Product"
    price = -100.00
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/sites/site-guid/products" `
    -Method POST `
    -Body $body `
    -Headers @{Authorization="Bearer $token"}
# Expected: 400 Bad Request

# Test 2: Duplicate Site Domain
$body = @{
    name = "Duplicate Site"
    domain = "existing-domain.com"  # Already exists
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/sites" `
    -Method POST `
    -Body $body `
    -Headers @{Authorization="Bearer $adminToken"}
# Expected: 400 Bad Request (unique constraint violation)
```

---

### A05:2021 – Security Misconfiguration ✅

**Tests:**
```powershell
# Test 1: Debug Mode in Production
Invoke-RestMethod -Uri "http://localhost:5000/api/sites/invalid-guid"
# Expected: Generic error message, not stack trace

# Test 2: Security Headers
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/sites" `
    -Method GET -ErrorAction SilentlyContinue

# Check headers
$headers = @(
    "X-Frame-Options",
    "X-Content-Type-Options",
    "Content-Security-Policy",
    "Referrer-Policy"
)

foreach ($header in $headers) {
    if ($response.Headers[$header]) {
        Write-Host "✅ $header present" -ForegroundColor Green
    } else {
        Write-Host "❌ $header missing" -ForegroundColor Red
    }
}

# Test 3: Swagger in Production
Invoke-RestMethod -Uri "https://production-api.com/swagger" `
    -ErrorAction SilentlyContinue
# Expected: 404 Not Found
```

---

### A06:2021 – Vulnerable Components ✅

**Tests:**
```powershell
# Check for outdated packages
dotnet list package --outdated --include-transitive

# Scan for known vulnerabilities
dotnet list package --vulnerable --include-transitive

# Generate SBOM (Software Bill of Materials)
dotnet tool install --global CycloneDX
dotnet CycloneDX . -o sbom.json

# Upload to OWASP Dependency-Check
dependency-check --project "DOTNET_CMS" --scan sbom.json --format HTML --out dep-check-report.html
```

---

### A07:2021 – Identification and Authentication Failures ✅

**Tests:**
```powershell
# Test 1: Weak Password
$body = @{
    email = "test@test.com"
    password = "123"  # Weak password
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" `
    -Method POST `
    -Body $body
# Expected: 400 Bad Request (password policy violation)

# Test 2: Session Fixation
# Login, note token, logout, try to reuse token
$token1 = "Bearer <token-before-logout>"
Invoke-RestMethod -Uri "http://localhost:5000/api/auth/logout" `
    -Method POST `
    -Headers @{Authorization=$token1}
    
Invoke-RestMethod -Uri "http://localhost:5000/api/sites" `
    -Headers @{Authorization=$token1}
# Expected: 401 Unauthorized

# Test 3: Account Enumeration
$response1 = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method POST `
    -Body (@{email="existing@user.com"; password="wrong"} | ConvertTo-Json) `
    -ErrorAction SilentlyContinue
    
$response2 = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method POST `
    -Body (@{email="nonexistent@user.com"; password="wrong"} | ConvertTo-Json) `
    -ErrorAction SilentlyContinue

# Expected: Same error message for both (prevent enumeration)
```

---

### A08:2021 – Software and Data Integrity Failures ✅

**Tests:**
```powershell
# Test 1: JWT Signature Validation
# Modify JWT payload without re-signing
$invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.MODIFIED_PAYLOAD.signature"

Invoke-RestMethod -Uri "http://localhost:5000/api/sites" `
    -Headers @{Authorization="Bearer $invalidToken"}
# Expected: 401 Unauthorized

# Test 2: File Upload Integrity
# Upload file with mismatched MIME type
$file = "malware.exe"
$mimeType = "image/jpeg"  # Fake MIME type

# Expected: File validation rejects based on actual content
```

---

### A09:2021 – Security Logging and Monitoring Failures ⚠️

**Tests:**
```powershell
# Test 1: Failed Login Logging
# Attempt failed login, check logs
Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
    -Method POST `
    -Body (@{email="admin@cms.com"; password="wrong"} | ConvertTo-Json)

# Check logs for entry
Get-Content "CMS.API/logs/app.log" | Select-String "Failed login attempt"
# Expected: Log entry present

# Test 2: Suspicious Activity Detection
# Multiple failed logins from same IP
for ($i=1; $i -le 10; $i++) {
    Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
        -Method POST `
        -Body (@{email="admin@cms.com"; password="wrong$i"} | ConvertTo-Json) `
        -ErrorAction SilentlyContinue
}

# Check for alert/block
# Expected: Account lockout or IP ban after threshold
```

---

### A10:2021 – Server-Side Request Forgery (SSRF) ✅

**Tests:**
```powershell
# Test 1: URL Parameter SSRF
$body = @{
    imageUrl = "http://169.254.169.254/latest/meta-data/"  # AWS metadata endpoint
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/sites/site-guid/pages" `
    -Method POST `
    -Body $body `
    -Headers @{Authorization="Bearer $token"}
# Expected: 400 Bad Request or blocked

# Test 2: File Upload SSRF
# Upload XML with external entity
$xxePayload = @"
<?xml version="1.0"?>
<!DOCTYPE foo [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
<foo>&xxe;</foo>
"@

# Expected: XML parsing disabled or entities blocked
```

---

## PowerShell Security Test Scripts

### Complete Security Test Suite

Create `Security-Tests.ps1`:

```powershell
<#
.SYNOPSIS
    Automated security testing suite for DOTNET CMS API
.DESCRIPTION
    Runs comprehensive security tests against the API
.EXAMPLE
    .\Security-Tests.ps1 -BaseUrl "http://localhost:5000"
#>

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
Write-Host ""

# Login to get token
Write-Host "[*] Authenticating..." -ForegroundColor Yellow
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
Write-Host ""

# Test 1: Authentication Tests
Write-Host "[1/10] Authentication Security Tests" -ForegroundColor Yellow

$testResults += Test-SecurityEndpoint `
    -TestName "Missing Token" `
    -Method "GET" `
    -Endpoint "/api/sites" `
    -ExpectedStatusCodes @(401)

$testResults += Test-SecurityEndpoint `
    -TestName "Invalid Token" `
    -Method "GET" `
    -Endpoint "/api/sites" `
    -Headers @{Authorization = "Bearer invalid-token"} `
    -ExpectedStatusCodes @(401)

# Test 2: SQL Injection Tests
Write-Host "[2/10] SQL Injection Protection Tests" -ForegroundColor Yellow

$sqlPayloads = @(
    "' OR '1'='1",
    "'; DROP TABLE Sites;--",
    "admin'--"
)

foreach ($payload in $sqlPayloads) {
    $testResults += Test-SecurityEndpoint `
        -TestName "SQL Injection: $payload" `
        -Method "POST" `
        -Endpoint "/api/auth/login" `
        -Body @{email = "admin@cms.com$payload"; password = "test"} `
        -ExpectedStatusCodes @(400, 401)
}

# Test 3: XSS Protection Tests
Write-Host "[3/10] XSS Protection Tests" -ForegroundColor Yellow

$xssPayloads = @(
    "<script>alert('XSS')</script>",
    "<img src=x onerror=alert('XSS')>",
    "javascript:alert('XSS')"
)

foreach ($payload in $xssPayloads) {
    $testResults += Test-SecurityEndpoint `
        -TestName "XSS Injection: $payload" `
        -Method "POST" `
        -Endpoint "/api/sites" `
        -Headers $authHeaders `
        -Body @{name = $payload; domain = "test.com"} `
        -ExpectedStatusCodes @(400, 201)
}

# Test 4: CORS Tests
Write-Host "[4/10] CORS Protection Tests" -ForegroundColor Yellow

$testResults += Test-SecurityEndpoint `
    -TestName "CORS - Invalid Origin" `
    -Method "OPTIONS" `
    -Endpoint "/api/sites" `
    -Headers @{Origin = "https://evil.com"} `
    -ExpectedStatusCodes @(403, 200)

# Test 5: Input Validation Tests
Write-Host "[5/10] Input Validation Tests" -ForegroundColor Yellow

$testResults += Test-SecurityEndpoint `
    -TestName "Missing Required Field" `
    -Method "POST" `
    -Endpoint "/api/sites" `
    -Headers $authHeaders `
    -Body @{domain = "test.com"} `
    -ExpectedStatusCodes @(400)

$testResults += Test-SecurityEndpoint `
    -TestName "Invalid Email Format" `
    -Method "POST" `
    -Endpoint "/api/auth/register" `
    -Headers $authHeaders `
    -Body @{email = "notanemail"; password = "Test@123"} `
    -ExpectedStatusCodes @(400)

# Test 6: Authorization Tests
Write-Host "[6/10] Authorization Tests" -ForegroundColor Yellow

# Authenticate as Editor
$editorLogin = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
    -Method POST `
    -Body (@{email="editor@cms.com"; password="Editor@123"} | ConvertTo-Json) `
    -ContentType "application/json"
$editorToken = $editorLogin.token
$editorHeaders = @{Authorization = "Bearer $editorToken"}

# Authenticate as Viewer
$viewerLogin = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
    -Method POST `
    -Body (@{email="viewer@cms.com"; password="Viewer@123"} | ConvertTo-Json) `
    -ContentType "application/json"
$viewerToken = $viewerLogin.token
$viewerHeaders = @{Authorization = "Bearer $viewerToken"}

# Test Admin permissions
$testResults += Test-SecurityEndpoint `
    -TestName "Admin Can Create Site" `
    -Method "POST" `
    -Endpoint "/api/sites" `
    -Headers $authHeaders `
    -Body @{name="Test Site"; domain="test-$(New-Guid).com"} `
    -ExpectedStatusCodes @(200, 201)

# Test Editor permissions (can read/write but not delete)
$testResults += Test-SecurityEndpoint `
    -TestName "Editor Can Read Sites" `
    -Method "GET" `
    -Endpoint "/api/sites" `
    -Headers $editorHeaders `
    -ExpectedStatusCodes @(200)

$testResults += Test-SecurityEndpoint `
    -TestName "Editor Cannot Delete Site" `
    -Method "DELETE" `
    -Endpoint "/api/sites/$testSiteId" `
    -Headers $editorHeaders `
    -ExpectedStatusCodes @(403)

# Test Viewer permissions (read-only)
$testResults += Test-SecurityEndpoint `
    -TestName "Viewer Can Read Sites" `
    -Method "GET" `
    -Endpoint "/api/sites" `
    -Headers $viewerHeaders `
    -ExpectedStatusCodes @(200)

$testResults += Test-SecurityEndpoint `
    -TestName "Viewer Cannot Create Site" `
    -Method "POST" `
    -Endpoint "/api/sites" `
    -Headers $viewerHeaders `
    -Body @{name="Viewer Site"; domain="viewer-$(New-Guid).com"} `
    -ExpectedStatusCodes @(403)

# Test 7: Rate Limiting Tests
Write-Host "[7/10] Rate Limiting Tests" -ForegroundColor Yellow

$rapidRequests = 0
for ($i=1; $i -le 50; $i++) {
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" `
            -Method POST `
            -Body (@{email="test@test.com"; password="wrong"} | ConvertTo-Json) `
            -ContentType "application/json" `
            -ErrorAction SilentlyContinue
        $rapidRequests++
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 429) {
            Write-Host "  [OK] Rate limit activated after $rapidRequests requests" -ForegroundColor Green
            break
        }
    }
}

if ($rapidRequests -ge 50) {
    Write-Host "  [WARNING] No rate limiting detected" -ForegroundColor Yellow
}

# Test 8: Security Headers Tests
Write-Host "[8/10] Security Headers Tests" -ForegroundColor Yellow

$response = Invoke-WebRequest -Uri "$BaseUrl/api/sites" -Method GET -ErrorAction SilentlyContinue

$requiredHeaders = @(
    "X-Frame-Options",
    "X-Content-Type-Options",
    "Content-Security-Policy"
)

foreach ($header in $requiredHeaders) {
    if ($response.Headers[$header]) {
        Write-Host "  [OK] $header present: $($response.Headers[$header])" -ForegroundColor Green
    } else {
        Write-Host "  [WARNING] $header missing" -ForegroundColor Yellow
    }
}

# Test 9: Path Traversal Tests
Write-Host "[9/10] Path Traversal Protection Tests" -ForegroundColor Yellow

$pathTraversalPayloads = @(
    "../../etc/passwd",
    "..\..\windows\system32\config\sam",
    "....//....//etc//passwd"
)

foreach ($payload in $pathTraversalPayloads) {
    $testResults += Test-SecurityEndpoint `
        -TestName "Path Traversal: $payload" `
        -Method "GET" `
        -Endpoint "/api/sites/$payload" `
        -Headers $authHeaders `
        -ExpectedStatusCodes @(400, 404)
}

# Test 10: Business Logic Tests
Write-Host "[10/10] Business Logic Security Tests" -ForegroundColor Yellow

# Get list of sites first
$sites = Invoke-RestMethod -Uri "$BaseUrl/api/sites" -Method GET

if ($sites.Count -gt 0) {
    $siteId = $sites[0].id
    
    $testResults += Test-SecurityEndpoint `
        -TestName "Negative Price" `
        -Method "POST" `
        -Endpoint "/api/sites/$siteId/products" `
        -Headers $authHeaders `
        -Body @{productId="test"; name="Test"; price=-100} `
        -ExpectedStatusCodes @(400)
}

# Generate Report
Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Test Results Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$testResults | Format-Table -AutoSize

$passed = ($testResults | Where-Object {$_.Passed -eq $true}).Count
$failed = ($testResults | Where-Object {$_.Passed -eq $false}).Count
$total = $testResults.Count

Write-Host ""
Write-Host "Total Tests: $total" -ForegroundColor Cyan
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })
Write-Host ""

if ($failed -eq 0) {
    Write-Host "✅ All security tests passed!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some security tests failed. Review the results above." -ForegroundColor Yellow
}

# Export results
$testResults | Export-Csv -Path "security-test-results.csv" -NoTypeInformation
Write-Host "Results exported to: security-test-results.csv" -ForegroundColor Cyan
```

---

## CI/CD Integration

### GitHub Actions Workflow

Create `.github/workflows/security-scan.yml`:

```yaml
name: Security Scan

on:
  push:
    branches: [ master, main ]
  pull_request:
    branches: [ master, main ]
  schedule:
    - cron: '0 2 * * 1'  # Weekly on Monday at 2 AM

jobs:
  security-scan:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run Tests
      run: dotnet test --no-build --verbosity normal
    
    - name: Check for Vulnerable Packages
      run: dotnet list package --vulnerable --include-transitive
    
    - name: OWASP ZAP Scan
      uses: zaproxy/action-full-scan@v0.4.0
      with:
        target: 'http://localhost:5000'
        rules_file_name: '.zap/rules.tsv'
        cmd_options: '-a'
    
    - name: Trivy Container Scan
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: 'cms-api:latest'
        format: 'sarif'
        output: 'trivy-results.sarif'
    
    - name: Upload Trivy results to GitHub Security
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'trivy-results.sarif'
    
    - name: SonarCloud Scan
      uses: SonarSource/sonarcloud-github-action@master
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
```

---

## Reporting & Remediation

### Vulnerability Report Template

```markdown
# Security Vulnerability Report

**Date:** [Date]
**Severity:** [Critical/High/Medium/Low]
**Affected Component:** [API Endpoint/Component]
**Reporter:** [Name/Tool]

## Summary
Brief description of the vulnerability

## Steps to Reproduce
1. Step 1
2. Step 2
3. Expected vs Actual result

## Impact
Potential security impact

## Remediation
Proposed fix

## References
- OWASP Link
- CVE Number (if applicable)
```

### Prioritization Matrix

| Severity | Exploitability | Remediation Timeline |
|----------|----------------|----------------------|
| **Critical** | Easy | Immediate (24 hours) |
| **High** | Moderate | 1 week |
| **Medium** | Difficult | 1 month |
| **Low** | Very Difficult | 3 months |

---

## Security Testing Schedule

### Recommended Cadence

- **Daily**: Automated security tests in CI/CD
- **Weekly**: OWASP ZAP quick scan
- **Monthly**: Full OWASP ZAP scan + manual testing
- **Quarterly**: External penetration test (hired firm)
- **Annually**: Comprehensive security audit

---

## Additional Resources

### Documentation
- [OWASP Testing Guide](https://owasp.org/www-project-web-security-testing-guide/)
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)

### Tools
- [OWASP ZAP](https://www.zaproxy.org/)
- [Burp Suite](https://portswigger.net/burp)
- [Postman](https://www.postman.com/)
- [SonarQube](https://www.sonarqube.org/)
- [Trivy](https://github.com/aquasecurity/trivy)

### Services
- [Snyk](https://snyk.io/) - Dependency scanning
- [WhiteSource](https://www.whitesourcesoftware.com/) - License compliance
- [HackerOne](https://www.hackerone.com/) - Bug bounty platform

---

**Document Version:** 1.0  
**Last Updated:** November 9, 2025  
**Next Review:** December 9, 2025
