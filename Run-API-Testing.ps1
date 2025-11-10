# Run API with Testing Configuration
# This script starts the API with higher rate limits suitable for testing

param(
    [string]$Environment = "Testing"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Starting CMS API in Testing Mode" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Database: cms_test" -ForegroundColor Cyan
Write-Host "Rate Limits: RELAXED (100 login/min, 1000 general/min)" -ForegroundColor Green
Write-Host ""

# Check if test database exists
Write-Host "Checking test database..." -ForegroundColor Yellow
$checkDb = docker exec -i cms_postgres psql -U postgres -t -c "SELECT 1 FROM pg_database WHERE datname='cms_test';" 2>$null

if (-not ($checkDb -match "1")) {
    Write-Host "[WARNING] Test database 'cms_test' does not exist!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run the setup script first:" -ForegroundColor Yellow
    Write-Host "  .\Setup-Test-Database.ps1" -ForegroundColor White
    Write-Host ""
    $response = Read-Host "Do you want to run the setup now? (Y/n)"
    if ($response -ne "n" -and $response -ne "N") {
        & ".\Setup-Test-Database.ps1"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[ERROR] Database setup failed" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "[INFO] Exiting. Run setup script before starting API." -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "[OK] Test database found" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop the API" -ForegroundColor DarkGray
Write-Host ""

# Set environment variable
$env:ASPNETCORE_ENVIRONMENT = $Environment

# Navigate to API directory and run
Set-Location -Path "CMS.API"

# Use --no-launch-profile to ensure environment variable is respected
dotnet run --no-launch-profile
