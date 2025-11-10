# Check SonarQube Status
# This script checks if SonarQube is ready to accept analysis

param(
    [string]$SonarQubeUrl = "http://localhost:9000",
    [int]$MaxWaitSeconds = 120
)

Write-Host "Checking SonarQube status at $SonarQubeUrl..." -ForegroundColor Yellow
Write-Host ""

$elapsed = 0
$interval = 5

while ($elapsed -lt $MaxWaitSeconds) {
    try {
        $response = Invoke-RestMethod -Uri "$SonarQubeUrl/api/system/status" -ErrorAction Stop
        $status = $response.status
        
        if ($status -eq "UP") {
            Write-Host "✅ SonarQube is UP and ready!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Access SonarQube at: $SonarQubeUrl" -ForegroundColor Cyan
            Write-Host "Default login: admin / admin" -ForegroundColor Gray
            Write-Host ""
            Write-Host "Next steps:" -ForegroundColor Yellow
            Write-Host "1. Open http://localhost:9000" -ForegroundColor White
            Write-Host "2. Login with admin/admin" -ForegroundColor White
            Write-Host "3. Change the default password" -ForegroundColor White
            Write-Host "4. Generate an authentication token" -ForegroundColor White
            Write-Host "5. Run: .\Run-SonarQube-Analysis.ps1 -Token 'your-token'" -ForegroundColor White
            Write-Host ""
            
            # Try to open browser
            Start-Process $SonarQubeUrl
            
            exit 0
        }
        elseif ($status -eq "STARTING") {
            Write-Host "⏳ SonarQube is starting... (elapsed: $elapsed seconds)" -ForegroundColor Yellow
        }
        else {
            Write-Host "⚠️  SonarQube status: $status (elapsed: $elapsed seconds)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "⏳ Waiting for SonarQube to start... (elapsed: $elapsed seconds)" -ForegroundColor Yellow
    }
    
    Start-Sleep -Seconds $interval
    $elapsed += $interval
}

Write-Host ""
Write-Host "❌ Timeout: SonarQube did not start within $MaxWaitSeconds seconds" -ForegroundColor Red
Write-Host ""
Write-Host "Troubleshooting steps:" -ForegroundColor Yellow
Write-Host "1. Check if container is running: docker-compose ps" -ForegroundColor White
Write-Host "2. Check logs: docker-compose logs sonarqube --tail 50" -ForegroundColor White
Write-Host "3. Ensure Docker has enough memory (at least 2GB for SonarQube)" -ForegroundColor White
Write-Host "4. Try restarting: docker-compose restart sonarqube" -ForegroundColor White
Write-Host ""

exit 1
