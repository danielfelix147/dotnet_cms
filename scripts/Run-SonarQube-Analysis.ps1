# SonarQube Analysis Script for DOTNET CMS
# This script runs code quality and security analysis using SonarQube

param(
    [string]$SonarQubeUrl = "http://localhost:9000",
    [string]$ProjectKey = "DOTNET_CMS",
    [string]$ProjectName = "DOTNET CMS",
    [string]$Token = $null
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SonarQube Analysis - DOTNET CMS" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if SonarQube is running
Write-Host "[1/6] Checking SonarQube availability..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$SonarQubeUrl/api/system/status" -UseBasicParsing -TimeoutSec 5
    $status = ($response.Content | ConvertFrom-Json).status
    
    if ($status -ne "UP") {
        Write-Host "  [ERROR] SonarQube is not ready. Status: $status" -ForegroundColor Red
        Write-Host "  Please wait for SonarQube to fully start (can take 1-2 minutes)" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "  [OK] SonarQube is running at $SonarQubeUrl" -ForegroundColor Green
}
catch {
    Write-Host "  [ERROR] Cannot connect to SonarQube at $SonarQubeUrl" -ForegroundColor Red
    Write-Host "  Make sure SonarQube container is running: docker-compose up -d sonarqube" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Check if dotnet-sonarscanner is installed
Write-Host "[2/6] Checking SonarScanner for .NET..." -ForegroundColor Yellow
try {
    $scannerVersion = dotnet tool list --global | Select-String "dotnet-sonarscanner"
    
    if (-not $scannerVersion) {
        Write-Host "  [INFO] Installing SonarScanner for .NET..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-sonarscanner
        Write-Host "  [OK] SonarScanner installed successfully" -ForegroundColor Green
    }
    else {
        Write-Host "  [OK] SonarScanner is already installed" -ForegroundColor Green
    }
}
catch {
    Write-Host "  [ERROR] Failed to check/install SonarScanner" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Get or create authentication token
Write-Host "[3/6] Setting up authentication..." -ForegroundColor Yellow
if (-not $Token) {
    Write-Host "  [INFO] First-time setup instructions:" -ForegroundColor Cyan
    Write-Host "  1. Open http://localhost:9000 in your browser" -ForegroundColor Gray
    Write-Host "  2. Login with: admin / admin" -ForegroundColor Gray
    Write-Host "  3. Change the default password when prompted" -ForegroundColor Gray
    Write-Host "  4. Go to: My Account > Security > Generate Token" -ForegroundColor Gray
    Write-Host "  5. Name: 'DOTNET_CMS_Analysis'" -ForegroundColor Gray
    Write-Host "  6. Type: 'Global Analysis Token'" -ForegroundColor Gray
    Write-Host "  7. Copy the token and run this script again with -Token parameter" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Example: .\Run-SonarQube-Analysis.ps1 -Token 'your-token-here'" -ForegroundColor Yellow
    Write-Host ""
    
    $useDefault = Read-Host "  Do you want to try with default credentials (admin/admin)? (y/n)"
    
    if ($useDefault -eq 'y') {
        Write-Host "  [WARNING] Using default credentials (not recommended for production)" -ForegroundColor Yellow
        $Token = "admin"
    }
    else {
        Write-Host "  [INFO] Please run the script again with -Token parameter" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "  [OK] Authentication configured" -ForegroundColor Green
Write-Host ""

# Begin SonarQube analysis
Write-Host "[4/6] Starting SonarQube analysis..." -ForegroundColor Yellow
try {
    $beginArgs = @(
        "begin",
        "/k:$ProjectKey",
        "/n:$ProjectName",
        "/d:sonar.host.url=$SonarQubeUrl",
        "/d:sonar.token=$Token",
        "/d:sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml",
        "/d:sonar.cs.vstest.reportsPaths=**/*.trx"
    )
    
    dotnet sonarscanner @beginArgs
    Write-Host "  [OK] Analysis initialized" -ForegroundColor Green
}
catch {
    Write-Host "  [ERROR] Failed to initialize SonarQube analysis" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Build the project
Write-Host "[5/6] Building project..." -ForegroundColor Yellow
try {
    dotnet build --configuration Release --no-incremental
    Write-Host "  [OK] Build completed" -ForegroundColor Green
}
catch {
    Write-Host "  [ERROR] Build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# End SonarQube analysis and upload results
Write-Host "[6/6] Uploading analysis results to SonarQube..." -ForegroundColor Yellow
try {
    dotnet sonarscanner end /d:sonar.token=$Token
    Write-Host "  [OK] Analysis completed and uploaded" -ForegroundColor Green
}
catch {
    Write-Host "  [ERROR] Failed to complete analysis" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Analysis Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "View results at: $SonarQubeUrl/dashboard?id=$ProjectKey" -ForegroundColor Cyan
Write-Host ""
Write-Host "SonarQube will analyze:" -ForegroundColor White
Write-Host "  - Code Quality Issues" -ForegroundColor Gray
Write-Host "  - Security Vulnerabilities" -ForegroundColor Gray
Write-Host "  - Code Smells" -ForegroundColor Gray
Write-Host "  - Test Coverage" -ForegroundColor Gray
Write-Host "  - Code Duplication" -ForegroundColor Gray
Write-Host "  - Technical Debt" -ForegroundColor Gray
Write-Host ""
