# Start Docker Services for CMS
param([switch]$Build, [switch]$Down, [switch]$Logs)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "CMS Docker Services Manager" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

if ($Down) {
    Write-Host "Stopping services..." -ForegroundColor Yellow
    docker-compose down
    exit 0
}

if ($Logs) {
    docker-compose logs -f
    exit 0
}

if ($Build) {
    docker-compose up --build -d
} else {
    docker-compose up -d
}

Start-Sleep -Seconds 5
Write-Host ""
Write-Host "Services Started!" -ForegroundColor Green
Write-Host ""
Write-Host "PostgreSQL: localhost:5432 (postgres/postgres)" -ForegroundColor Cyan
Write-Host "pgAdmin:    http://localhost:5050 (admin@cms.com/admin123)" -ForegroundColor Cyan
Write-Host "MailHog:    http://localhost:8025" -ForegroundColor Cyan
Write-Host ""
Write-Host "To connect in pgAdmin, use host: postgres" -ForegroundColor Yellow
