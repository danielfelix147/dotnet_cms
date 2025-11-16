# Testing Configuration Guide

## Running Security Tests

### Step 1: Create Test Users
```powershell
# Run workflow script to create Editor and Viewer test users
.\Run-CMS-Workflow.ps1
```

This creates:
- **Admin**: `admin@cms.com` / `Admin@123` (pre-existing)
- **Editor**: `editor@cms.com` / `Editor@123` (created by workflow)
- **Viewer**: `viewer@cms.com` / `Viewer@123` (created by workflow)

### Step 2: Start API with Testing Configuration
```powershell
# Start API with relaxed rate limits (100 login/min)
.\Run-API-Testing.ps1
```

### Step 3: Run Security Tests
```powershell
# In another terminal, run comprehensive security tests
.\Security-Tests.ps1
```

The security test suite includes **13+ authorization tests** covering all three roles.

### Option: Quick Test Without Users
```powershell
# API with default settings
dotnet run --project CMS.API

# Run security tests (authorization tests will be skipped)
.\Security-Tests.ps1
```

## Rate Limit Configurations

### Production (`appsettings.Development.json`)
- **Login**: 5 attempts per minute
- **Registration**: 3 per hour
- **General API**: 100/min, 1000/hour
- **Purpose**: Prevent brute force attacks

### Testing (`appsettings.Testing.json`)
- **Login**: 100 attempts per minute
- **Registration**: 50 per hour
- **General API**: 1000/min, 10000/hour
- **Purpose**: Allow rapid automated testing

## Benefits

✅ **No test interference**: SQL injection tests won't hit rate limits
✅ **Faster test execution**: No delays needed between tests
✅ **Realistic rate limit testing**: Test #7 can still verify rate limiting works
✅ **Production safety**: Strict limits still apply in production
✅ **Flexible**: Easy to adjust limits per environment

## Rate Limit Test

To specifically test rate limiting (Test #7), you can:
1. Temporarily reduce limits in `appsettings.Testing.json`
2. Run the dedicated rate limit test
3. Restore higher limits for other tests

Example for rate limit testing:
```json
"RateLimiting": {
  "Login": {
    "Limit": 3  // Lower limit to test with fewer calls
  }
}
```
