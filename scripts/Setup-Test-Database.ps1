# Setup Test Database for CMS
# This script creates a separate test database and applies migrations

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Setting Up Test Database" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Database configuration
$dbHost = "localhost"
$dbPort = "5432"
$dbUser = "postgres"
$dbPassword = "postgres"
$dbName = "cms_test"

Write-Host "Creating test database: $dbName" -ForegroundColor Yellow

# Create database using psql
$createDbCommand = "CREATE DATABASE $dbName;"

try {
    # Check if database exists
    $checkDb = "SELECT 1 FROM pg_database WHERE datname='$dbName';"
    $exists = docker exec -i cms_postgres psql -U $dbUser -t -c $checkDb 2>$null
    
    if ($exists -match "1") {
        Write-Host "[INFO] Database '$dbName' already exists" -ForegroundColor DarkGray
        
        $response = Read-Host "Do you want to drop and recreate it? (y/N)"
        if ($response -eq "y" -or $response -eq "Y") {
            Write-Host "Dropping existing database..." -ForegroundColor Yellow
            docker exec -i cms_postgres psql -U $dbUser -c "DROP DATABASE $dbName;"
            docker exec -i cms_postgres psql -U $dbUser -c $createDbCommand
            Write-Host "[OK] Database recreated" -ForegroundColor Green
        } else {
            Write-Host "[INFO] Using existing database" -ForegroundColor DarkGray
        }
    } else {
        docker exec -i cms_postgres psql -U $dbUser -c $createDbCommand
        Write-Host "[OK] Database created" -ForegroundColor Green
    }
}
catch {
    Write-Host "[ERROR] Failed to create database: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "[INFO] Make sure PostgreSQL is running (docker-compose up -d)" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Applying migrations to test database..." -ForegroundColor Yellow

# Set environment to Testing so EF uses the correct connection string
$env:ASPNETCORE_ENVIRONMENT = "Testing"

# Navigate to Infrastructure project and apply migrations
Set-Location -Path "CMS.Infrastructure"

try {
    # Apply migrations
    dotnet ef database update --startup-project ../CMS.API --context CMSDbContext
    Write-Host "[OK] Migrations applied successfully" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Failed to apply migrations: $($_.Exception.Message)" -ForegroundColor Red
    Set-Location -Path ".."
    exit 1
}

# Return to root directory
Set-Location -Path ".."

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Test Database Setup Complete!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Database: $dbName" -ForegroundColor Cyan
Write-Host "Host: ${dbHost}:${dbPort}" -ForegroundColor Cyan
Write-Host "User: $dbUser" -ForegroundColor Cyan
Write-Host ""
Write-Host "The test database is ready for use with:" -ForegroundColor Yellow
Write-Host "  .\Run-API-Testing.ps1" -ForegroundColor White
Write-Host ""
