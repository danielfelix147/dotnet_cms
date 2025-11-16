# DOTNET CMS Cleanup Script
# Deletes all resources from an exported site JSON file

param(
    [Parameter(Mandatory=$true)]
    [string]$ExportFile,
    [string]$BaseUrl = "http://localhost:5000",
    [string]$AdminEmail = "admin@cms.com",
    [string]$AdminPassword = "Admin@123"
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "DOTNET CMS Cleanup Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if export file exists (support both relative and absolute paths, and exports folder)
$exportFilePath = $ExportFile
if (-not (Test-Path $exportFilePath)) {
    # Try exports folder
    $exportFilePath = Join-Path "exports" $ExportFile
    if (-not (Test-Path $exportFilePath)) {
        Write-Host "Error: Export file not found: $ExportFile" -ForegroundColor Red
        Write-Host "Tried locations:" -ForegroundColor Yellow
        Write-Host "  - $ExportFile" -ForegroundColor Gray
        Write-Host "  - exports\$ExportFile" -ForegroundColor Gray
        exit 1
    }
}

Write-Host "Using export file: $exportFilePath" -ForegroundColor Gray
Write-Host ""

# Helper function to make API calls
function Invoke-CMSApi {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = @{},
        [bool]$ThrowOnError = $false
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
        $response = Invoke-RestMethod @params
        return @{ success = $true; data = $response }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($ThrowOnError) {
            throw
        }
        
        # 204 No Content means success (for DELETE operations)
        if ($statusCode -eq 204) {
            return @{ success = $true; noContent = $true }
        }
        
        # 404 means already deleted, which is fine
        if ($statusCode -eq 404) {
            Write-Host "  [SKIP] Resource not found (already deleted?)" -ForegroundColor DarkGray
            return @{ skipped = $true }
        }
        
        Write-Host "  [ERROR] $Method $Endpoint - Status: $statusCode" -ForegroundColor Red
        Write-Host "  [ERROR] $($_.Exception.Message)" -ForegroundColor Red
        return @{ error = $true; statusCode = $statusCode }
    }
}

try {
    # Load the export file
    Write-Host "Loading export file: $exportFilePath" -ForegroundColor Yellow
    $export = Get-Content $exportFilePath -Raw | ConvertFrom-Json
    Write-Host "[OK] Export file loaded" -ForegroundColor Green
    Write-Host ""

    # Login
    Write-Host "[1/7] Logging in as Admin..." -ForegroundColor Yellow
    $loginBody = @{
        email = $AdminEmail
        password = $AdminPassword
    }
    $loginResponse = Invoke-CMSApi -Method POST -Endpoint "/api/auth/login" -Body $loginBody -ThrowOnError $true
    if (-not $loginResponse.success -or -not $loginResponse.data.token) {
        throw "Login failed - no token received"
    }
    $token = $loginResponse.data.token
    Write-Host "[OK] Login successful" -ForegroundColor Green
    Write-Host ""

    $authHeaders = @{
        Authorization = "Bearer $token"
    }

    $siteId = $export.siteId
    Write-Host "Site ID: $siteId" -ForegroundColor Cyan
    Write-Host "Site Name: $($export.name)" -ForegroundColor Cyan
    Write-Host "Site Domain: $($export.domain)" -ForegroundColor Cyan
    Write-Host ""

    # Delete Tours (child of Destinations)
    if ($export.destinations -and $export.destinations.Count -gt 0) {
        Write-Host "[2/7] Deleting Tours..." -ForegroundColor Yellow
        $tourCount = 0
        foreach ($destination in $export.destinations) {
            if ($destination.tours -and $destination.tours.Count -gt 0) {
                foreach ($tour in $destination.tours) {
                    $destId = $destination.id
                    $tourId = $tour.id
                    Write-Host "  Deleting tour: $($tour.name) (ID: $tourId)" -ForegroundColor DarkGray
                    $result = Invoke-CMSApi -Method DELETE -Endpoint "/api/sites/$siteId/destinations/$destId/tours/$tourId" -Headers $authHeaders
                    if ($result.success -or $result.noContent) {
                        $tourCount++
                    }
                }
            }
        }
        Write-Host "[OK] Deleted $tourCount tours" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host "[2/7] No tours to delete" -ForegroundColor DarkGray
        Write-Host ""
    }

    # Delete Destinations
    if ($export.destinations -and $export.destinations.Count -gt 0) {
        Write-Host "[3/7] Deleting Destinations..." -ForegroundColor Yellow
        $destCount = 0
        foreach ($destination in $export.destinations) {
            $destId = $destination.id
            Write-Host "  Deleting destination: $($destination.name) (ID: $destId)" -ForegroundColor DarkGray
            $result = Invoke-CMSApi -Method DELETE -Endpoint "/api/sites/$siteId/destinations/$destId" -Headers $authHeaders
            if ($result.success -or $result.noContent) {
                $destCount++
            }
        }
        Write-Host "[OK] Deleted $destCount destinations" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host "[3/7] No destinations to delete" -ForegroundColor DarkGray
        Write-Host ""
    }

    # Delete Products
    if ($export.products -and $export.products.Count -gt 0) {
        Write-Host "[4/7] Deleting Products..." -ForegroundColor Yellow
        $productCount = 0
        foreach ($product in $export.products) {
            $productId = $product.id
            Write-Host "  Deleting product: $($product.name) (ID: $productId)" -ForegroundColor DarkGray
            $result = Invoke-CMSApi -Method DELETE -Endpoint "/api/sites/$siteId/products/$productId" -Headers $authHeaders
            if ($result.success -or $result.noContent) {
                $productCount++
            }
        }
        Write-Host "[OK] Deleted $productCount products" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host "[4/7] No products to delete" -ForegroundColor DarkGray
        Write-Host ""
    }

    # Delete Pages
    if ($export.pages -and $export.pages.Count -gt 0) {
        Write-Host "[5/7] Deleting Pages..." -ForegroundColor Yellow
        $pageCount = 0
        foreach ($page in $export.pages) {
            $pageId = $page.id
            Write-Host "  Deleting page: $($page.title) (ID: $pageId)" -ForegroundColor DarkGray
            $result = Invoke-CMSApi -Method DELETE -Endpoint "/api/sites/$siteId/pages/$pageId" -Headers $authHeaders
            if ($result.success -or $result.noContent) {
                $pageCount++
            }
        }
        Write-Host "[OK] Deleted $pageCount pages" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host "[5/7] No pages to delete" -ForegroundColor DarkGray
        Write-Host ""
    }

    # Delete Media
    if ($export.media -and $export.media.Count -gt 0) {
        Write-Host "[6/7] Deleting Media..." -ForegroundColor Yellow
        $mediaCount = 0
        foreach ($media in $export.media) {
            $mediaId = $media.id
            $mediaType = if ($media.mimeType -like "image/*") { "Image" } else { "File" }
            Write-Host "  Deleting media: $($media.title) (ID: $mediaId, Type: $mediaType)" -ForegroundColor DarkGray
            $result = Invoke-CMSApi -Method DELETE -Endpoint "/api/media/$mediaId?mediaType=$mediaType" -Headers $authHeaders
            if ($result.success -or $result.noContent) {
                $mediaCount++
            }
        }
        Write-Host "[OK] Deleted $mediaCount media items" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Host "[6/7] No media to delete" -ForegroundColor DarkGray
        Write-Host ""
    }

    # Delete Site (this should cascade delete any remaining associations)
    Write-Host "[7/7] Deleting Site..." -ForegroundColor Yellow
    Write-Host "  Deleting site: $($export.name) (ID: $siteId)" -ForegroundColor DarkGray
    $result = Invoke-CMSApi -Method DELETE -Endpoint "/api/sites/$siteId" -Headers $authHeaders -ThrowOnError $true
    Write-Host "[OK] Site deleted: $($export.name)" -ForegroundColor Green
    Write-Host ""

    # Summary
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "Cleanup Completed Successfully!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "All resources from the export file have been deleted." -ForegroundColor White
    Write-Host ""

}
catch {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host "Cleanup Failed!" -ForegroundColor Red
    Write-Host "=====================================" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    exit 1
}
