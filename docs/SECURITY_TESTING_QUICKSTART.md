# Security Testing Quick Reference

## 🚀 Quick Start

### Run Automated Security Tests
```powershell
# Basic run
.\Security-Tests.ps1

# Custom API URL
.\Security-Tests.ps1 -BaseUrl "http://localhost:5055"

# Custom credentials
.\Security-Tests.ps1 -AdminEmail "admin@cms.com" -AdminPassword "Admin@123"
```

**Output:**
- Console report with color-coded results
- `security-test-results-YYYYMMDD-HHmmss.csv` - CSV export
- `security-test-results-YYYYMMDD-HHmmss.html` - HTML report (open in browser)

---

## 🔧 Free Tools Setup

### 1. OWASP ZAP (Recommended)
```powershell
# Install via Chocolatey
choco install zap

# Or download: https://www.zaproxy.org/download/

# Quick scan
zap-cli quick-scan --spider http://localhost:5055
zap-cli report -o zap-report.html -f html
```

### 2. Burp Suite Community
- Download: https://portswigger.net/burp/communitydownload
- Configure browser proxy: 127.0.0.1:8080
- Import API: http://localhost:5055/swagger/v1/swagger.json

### 3. Postman Security Tests
Import your existing collection and add test scripts:
```javascript
// SQL Injection Test
pm.test("SQL Injection Protection", function() {
    pm.expect(pm.response.code).to.be.oneOf([400, 401]);
});

// XSS Test
pm.test("XSS Protection", function() {
    pm.expect(pm.response.text()).to.not.include("<script>");
});
```

---

## ✅ 10-Point Security Checklist

Before deploying to production:

- [ ] **Run automated tests**: `.\Security-Tests.ps1` (all pass)
- [ ] **OWASP ZAP scan**: No high/critical vulnerabilities
- [ ] **Check packages**: `dotnet list package --vulnerable` (none found)
- [ ] **Security headers**: All present in production
- [ ] **HTTPS enforced**: HTTP redirects to HTTPS
- [ ] **CORS configured**: Only allowed origins in whitelist
- [ ] **JWT secret**: Strong (32+ characters)
- [ ] **Rate limiting**: Implemented (TODO)
- [ ] **Input validation**: All commands validated
- [ ] **HTML sanitization**: Applied to content fields

---

## 🎯 Test Coverage

### Current: 291 Automated Tests
- Domain layer tests
- Application layer tests  
- Infrastructure tests
- Integration tests

### Security-Specific: 40+ Tests
- Authentication (3 tests)
- SQL Injection (5 payloads)
- XSS Protection (6 payloads)
- CORS (2 tests)
- Input Validation (4 tests)
- Authorization (varies)
- Rate Limiting (50 requests)
- Security Headers (4 headers)
- Path Traversal (5 payloads)
- Business Logic (3 tests)

---

## 🔍 Manual Testing Examples

### Test SQL Injection
```powershell
$body = @{
    email = "admin@cms.com' OR '1'='1"
    password = "anything"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5055/api/auth/login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

# Expected: 401 Unauthorized (not SQL error)
```

### Test XSS
```powershell
$body = @{
    name = "<script>alert('XSS')</script>"
    domain = "test.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5055/api/sites" `
    -Method POST `
    -Body $body `
    -Headers @{Authorization="Bearer $token"} `
    -ContentType "application/json"

# Expected: 400 Bad Request OR sanitized in response
```

### Test Authorization
```powershell
# Without token
Invoke-RestMethod -Uri "http://localhost:5055/api/sites" `
    -Method DELETE
    
# Expected: 401 Unauthorized
```

### Check Security Headers
```powershell
$response = Invoke-WebRequest -Uri "http://localhost:5055/api/sites"
$response.Headers["X-Frame-Options"]
$response.Headers["Content-Security-Policy"]

# Expected: Headers present with proper values
```

---

## 📊 Vulnerability Severity

| Level | Action | Timeline |
|-------|--------|----------|
| **Critical** | Stop deployment, fix immediately | 24 hours |
| **High** | Fix before next release | 1 week |
| **Medium** | Add to backlog, prioritize | 1 month |
| **Low** | Document, fix when convenient | 3 months |

---

## 🛡️ Security Best Practices

### ✅ IMPLEMENTED
- SQL Injection protection (EF Core)
- CSRF protection (anti-forgery tokens)
- Secure CORS policy (whitelist)
- Security headers (production)
- JWT authentication
- Role-based authorization
- HTML sanitizer service (created)
- Input validation (partial)

### ⚠️ TODO
- **High Priority:**
  - Apply HTML sanitization to PageContent
  - Complete input validation on all commands
  - Implement rate limiting
  - Add audit logging

- **Medium Priority:**
  - Content Security Policy refinement
  - API key authentication (alternative to JWT)
  - Request throttling per user
  - Failed login attempt tracking

---

## 📞 Reporting Security Issues

**DO NOT** create public GitHub issues for security vulnerabilities!

Instead:
1. Email: security@yourdomain.com
2. Include:
   - Vulnerability description
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

---

## 📚 Resources

### Documentation
- Full Guide: `API_SECURITY_TESTING_GUIDE.md`
- Implementation: `SECURITY_IMPLEMENTATION_SUMMARY.md`
- General Security: `SECURITY.md`

### Tools
- **OWASP ZAP**: https://www.zaproxy.org/
- **Burp Suite**: https://portswigger.net/burp
- **Trivy**: https://github.com/aquasecurity/trivy
- **SonarQube**: https://www.sonarqube.org/

### Learning
- **OWASP Top 10**: https://owasp.org/www-project-top-ten/
- **API Security**: https://owasp.org/www-project-api-security/
- **ASP.NET Security**: https://docs.microsoft.com/en-us/aspnet/core/security/

---

## 🚦 Testing Schedule

- **Daily**: CI/CD automated tests
- **Weekly**: Run `.\Security-Tests.ps1`
- **Monthly**: OWASP ZAP full scan
- **Quarterly**: External penetration test
- **Annually**: Comprehensive security audit

---

**Version**: 1.0  
**Last Updated**: November 9, 2025  
**For**: DOTNET CMS API v1.0
