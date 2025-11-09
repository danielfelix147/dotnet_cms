# DOTNET CMS Complete Workflow Script
# Executes the full workflow from login to JSON export

param(
    [string]$BaseUrl = "http://localhost:5055",
    [string]$AdminEmail = "admin@cms.com",
    [string]$AdminPassword = "Admin@123"
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "DOTNET CMS Workflow Automation" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Helper function to make API calls
function Invoke-CMSApi {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )
    
    $uri = "$BaseUrl$Endpoint"
    $params = @{
        Uri = $uri
        Method = $Method
        ContentType = "application/json"
        Headers = $Headers
    }
    
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 10)
    }
    
    try {
        return Invoke-RestMethod @params
    }
    catch {
        Write-Host "Error calling $Method $Endpoint" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        throw
    }
}

try {
    # Step 1: Login
    Write-Host "[1/12] Logging in as Admin..." -ForegroundColor Yellow
    $loginBody = @{
        email = $AdminEmail
        password = $AdminPassword
    }
    $loginResponse = Invoke-CMSApi -Method POST -Endpoint "/api/auth/login" -Body $loginBody
    $token = $loginResponse.token
    Write-Host "[OK] Login successful. Token obtained." -ForegroundColor Green
    Write-Host ""

    $authHeaders = @{
        Authorization = "Bearer $token"
    }

    # Step 2: Check/Create Site
    Write-Host "[2/12] Checking for existing site..." -ForegroundColor Yellow
    $siteDomain = "demo.example.com"
    $allSites = Invoke-CMSApi -Method GET -Endpoint "/api/sites" -Headers $authHeaders
    $existingSite = $allSites | Where-Object { $_.domain -eq $siteDomain }
    
    if ($existingSite) {
        $siteId = $existingSite.id
        $siteName = $existingSite.name
        Write-Host "[OK] Using existing site: $siteName" -ForegroundColor Green
        Write-Host "     Site ID: $siteId" -ForegroundColor Gray
    }
    else {
        Write-Host "[OK] Site not found, creating new site..." -ForegroundColor Yellow
        $siteBody = @{
            name = "My Demo Site"
            domain = $siteDomain
            description = "Site created via PowerShell workflow"
            isActive = $true
        }
        $siteResponse = Invoke-CMSApi -Method POST -Endpoint "/api/sites" -Body $siteBody -Headers $authHeaders
        $siteId = $siteResponse.id
        $siteName = $siteResponse.name
        Write-Host "[OK] Site created: $siteName" -ForegroundColor Green
        Write-Host "     Site ID: $siteId" -ForegroundColor Gray
    }
    Write-Host ""

    # Step 3: Get Database Plugins
    Write-Host "[3/12] Fetching available plugins..." -ForegroundColor Yellow
    $pluginsResponse = Invoke-CMSApi -Method GET -Endpoint "/api/plugins/database" -Headers $authHeaders
    
    $pagePlugin = $pluginsResponse | Where-Object { $_.systemName -eq "PageManagement" }
    $productPlugin = $pluginsResponse | Where-Object { $_.systemName -eq "ProductManagement" }
    $travelPlugin = $pluginsResponse | Where-Object { $_.systemName -eq "TravelManagement" }
    
    $pagePluginId = $pagePlugin.id
    $productPluginId = $productPlugin.id
    $travelPluginId = $travelPlugin.id
    
    Write-Host "[OK] Found plugins:" -ForegroundColor Green
    Write-Host "     - PageManagement: $pagePluginId" -ForegroundColor Gray
    Write-Host "     - ProductManagement: $productPluginId" -ForegroundColor Gray
    Write-Host "     - TravelManagement: $travelPluginId" -ForegroundColor Gray
    Write-Host ""

    # Step 4: Enable Page Plugin
    Write-Host "[4/12] Enabling PageManagement plugin..." -ForegroundColor Yellow
    $enablePageBody = @{
        configuration = ""
    }
    Invoke-CMSApi -Method POST -Endpoint "/api/plugins/site/$siteId/enable/$pagePluginId" -Body $enablePageBody -Headers $authHeaders | Out-Null
    Write-Host "[OK] PageManagement plugin enabled" -ForegroundColor Green
    Write-Host ""

    # Step 5: Enable Product Plugin
    Write-Host "[5/12] Enabling ProductManagement plugin..." -ForegroundColor Yellow
    $enableProductBody = @{
        configuration = ""
    }
    Invoke-CMSApi -Method POST -Endpoint "/api/plugins/site/$siteId/enable/$productPluginId" -Body $enableProductBody -Headers $authHeaders | Out-Null
    Write-Host "[OK] ProductManagement plugin enabled" -ForegroundColor Green
    Write-Host ""

    # Step 6: Enable Travel Plugin
    Write-Host "[6/12] Enabling TravelManagement plugin..." -ForegroundColor Yellow
    $enableTravelBody = @{
        configuration = ""
    }
    Invoke-CMSApi -Method POST -Endpoint "/api/plugins/site/$siteId/enable/$travelPluginId" -Body $enableTravelBody -Headers $authHeaders | Out-Null
    Write-Host "[OK] TravelManagement plugin enabled" -ForegroundColor Green
    Write-Host ""

    # Step 7: Create Page
    Write-Host "[7/12] Creating a page..." -ForegroundColor Yellow
    $pageBody = @{
        siteId = $siteId
        pageId = "welcome-page"
        title = "Welcome to Our Site"
        description = "This is the homepage"
        isPublished = $true
    }
    $pageResponse = Invoke-CMSApi -Method POST -Endpoint "/api/sites/$siteId/pages" -Body $pageBody -Headers $authHeaders
    $pageId = $pageResponse.id
    $pageTitle = $pageResponse.title
    Write-Host "[OK] Page created: $pageTitle" -ForegroundColor Green
    Write-Host "     Page ID: $pageId" -ForegroundColor Gray
    Write-Host ""

    # Step 8: Create Product
    Write-Host "[8/12] Creating a product..." -ForegroundColor Yellow
    $productBody = @{
        siteId = $siteId
        productId = "premium-widget"
        name = "Premium Widget"
        description = "Our best-selling widget"
        price = 99.99
        isPublished = $true
    }
    $productResponse = Invoke-CMSApi -Method POST -Endpoint "/api/sites/$siteId/products" -Body $productBody -Headers $authHeaders
    $productId = $productResponse.id
    $productName = $productResponse.name
    Write-Host "[OK] Product created: $productName" -ForegroundColor Green
    Write-Host "     Product ID: $productId" -ForegroundColor Gray
    Write-Host ""

    # Step 9: Create Destination
    Write-Host "[9/12] Creating a destination..." -ForegroundColor Yellow
    $destinationBody = @{
        siteId = $siteId
        destinationId = "paris"
        name = "Paris"
        description = "The City of Light"
        isPublished = $true
    }
    $destinationResponse = Invoke-CMSApi -Method POST -Endpoint "/api/sites/$siteId/destinations" -Body $destinationBody -Headers $authHeaders
    $destinationId = $destinationResponse.id
    $destinationName = $destinationResponse.name
    Write-Host "[OK] Destination created: $destinationName" -ForegroundColor Green
    Write-Host "     Destination ID: $destinationId" -ForegroundColor Gray
    Write-Host ""

    # Step 10: Create Tour
    Write-Host "[10/12] Creating a tour for the destination..." -ForegroundColor Yellow
    $tourBody = @{
        siteId = $siteId
        destinationId = $destinationId
        tourId = "paris-city-tour"
        name = "Paris City Tour"
        description = "Explore the best of Paris"
        price = 299.99
        isPublished = $true
    }
    $tourResponse = Invoke-CMSApi -Method POST -Endpoint "/api/sites/$siteId/destinations/$destinationId/tours" -Body $tourBody -Headers $authHeaders
    $tourId = $tourResponse.id
    $tourName = $tourResponse.name
    Write-Host "[OK] Tour created: $tourName" -ForegroundColor Green
    Write-Host "     Tour ID: $tourId" -ForegroundColor Gray
    Write-Host ""

    # Step 11: Get Media
    Write-Host "[11/12] Checking media..." -ForegroundColor Yellow
    $mediaResponse = Invoke-CMSApi -Method GET -Endpoint "/api/media/site/$siteId" -Headers $authHeaders
    $mediaCount = $mediaResponse.Count
    Write-Host "[OK] Media endpoint accessible. Media count: $mediaCount" -ForegroundColor Green
    Write-Host "     (Note: Image upload requires multipart form data - skipping in this script)" -ForegroundColor Gray
    Write-Host ""

    # Step 12: Export Site Configuration
    Write-Host "[12/12] Exporting site configuration to JSON..." -ForegroundColor Yellow
    $exportResponse = Invoke-CMSApi -Method GET -Endpoint "/api/content/export/$siteId" -Headers $authHeaders
    
    $exportFileName = "site_export_$siteId.json"
    $exportResponse | ConvertTo-Json -Depth 100 | Out-File -FilePath $exportFileName -Encoding UTF8
    
    Write-Host "[OK] Site configuration exported successfully!" -ForegroundColor Green
    Write-Host "     File saved: $exportFileName" -ForegroundColor Gray
    Write-Host ""

    # Summary
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "Workflow Completed Successfully!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Summary:" -ForegroundColor White
    Write-Host "  Site ID:        $siteId" -ForegroundColor Gray
    Write-Host "  Page ID:        $pageId" -ForegroundColor Gray
    Write-Host "  Product ID:     $productId" -ForegroundColor Gray
    Write-Host "  Destination ID: $destinationId" -ForegroundColor Gray
    Write-Host "  Tour ID:        $tourId" -ForegroundColor Gray
    Write-Host "  Export File:    $exportFileName" -ForegroundColor Gray
    Write-Host ""
    Write-Host "You can now review the exported JSON file!" -ForegroundColor Cyan

}
catch {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host "Workflow Failed!" -ForegroundColor Red
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    exit 1
}
