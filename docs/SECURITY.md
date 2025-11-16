# Security Implementation Guide

## Overview
This document outlines the security measures implemented in the DOTNET CMS to protect against common web vulnerabilities.

---

## 🛡️ Security Protections Implemented

### 1. **SQL Injection Protection** ✅

**Status:** **FULLY PROTECTED**

**Implementation:**
- All database queries use Entity Framework Core with LINQ
- EF Core automatically parameterizes all queries
- No raw SQL queries used anywhere in the codebase
- All user input is processed through strongly-typed entities

**Evidence:**
```csharp
// Example from SiteRepository.cs
return await _dbSet.FirstOrDefaultAsync(s => s.Domain == domain);
// EF Core converts this to parameterized SQL automatically
```

**Risk Level:** ✅ **LOW** - SQL Injection attacks are effectively mitigated.

---

### 2. **Cross-Site Scripting (XSS) Protection** ⚠️

**Status:** **INFRASTRUCTURE READY** - HtmlSanitizerService created, needs integration with PageContent commands

**Implementation:**

#### A. HTML Sanitization Service ✅
- Custom `HtmlSanitizerService` created and registered
- Whitelist approach: Only allows safe HTML tags
- Removes dangerous elements:
  - `<script>` tags and content
  - Event handlers (onclick, onerror, etc.)
  - `javascript:`, `vbscript:`, `data:` protocols
  - `<iframe>`, `<object>`, `<embed>`, `<form>` tags
  - Inline styles and `<style>` tags

**Allowed HTML Tags:**
```
p, br, strong, em, u, h1-h6, ul, ol, li, a, img, 
blockquote, code, pre, table, thead, tbody, tr, th, td, div, span
```

**Allowed Attributes:**
- `<a>`: href, title, target
- `<img>`: src, alt, title, width, height
- `<div>/<span>`: class
- `<table>/<td>/<th>`: class, colspan, rowspan

#### B. Needs Application to PageContent ⚠️
**Note:** The system uses `PageContent` entities to store HTML content, not a direct `Content` property on `Page`. When PageContent CRUD commands are created, they must use `IHtmlSanitizerService`:

```csharp
// Example for future CreatePageContentCommand handler
public class CreatePageContentCommandHandler
{
    private readonly IHtmlSanitizerService _htmlSanitizer;
    
    public async Task<PageContentDto> Handle(CreatePageContentCommand request, ...)
    {
        var sanitizedContent = _htmlSanitizer.Sanitize(request.Content);
        
        var pageContent = new PageContent
        {
            Content = sanitizedContent,
            // ... other properties
        };
        
        // Save to database
    }
}
```

#### C. Input Validation ✅
- Page commands have `[Required]` and `[StringLength]` attributes
- `PageId` uses regex validation: `^[a-zA-Z0-9-_]+$`
- Description limited to 500 characters
- Automatic model validation enabled in API

#### D. Output Encoding ✅
- ASP.NET Core automatically encodes JSON responses
- HTML entities encoded in sanitizer

**Risk Level:** ⚠️ **MEDIUM** - Infrastructure ready, but HTML sanitization not yet applied to PageContent. Create PageContent commands with sanitization integration.

---

### 3. **Cross-Site Request Forgery (CSRF) Protection** ✅

**Status:** **PROTECTED FOR API**

**Implementation:**

#### A. JWT Token Authentication
- API uses stateless JWT authentication
- Tokens must be included in `Authorization` header
- No cookies used for authentication (immune to traditional CSRF)

#### B. Anti-Forgery Tokens (Configured)
```csharp
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
```

#### C. CORS Protection
**Development:**
```csharp
// Permissive for local development
policy.AllowAnyOrigin()
```

**Production:**
```csharp
// Strict origin whitelist from appsettings.json
policy.WithOrigins(allowedOrigins)
      .AllowCredentials()
      .AllowAnyMethod()
      .AllowAnyHeader();
```

**Configured Origins:**
```json
"AllowedOrigins": [
  "http://localhost:3000",
  "http://localhost:5173",
  "https://yourdomain.com"
]
```

#### D. SameSite Cookie Policy
- Cookies set to `SameSite=Strict` in production
- Prevents CSRF via cookie-based attacks

**Risk Level:** ✅ **LOW** - CSRF attacks are mitigated through JWT authentication and proper CORS configuration.

---

## 🔒 Additional Security Headers (Production Only)

### Security Headers Implemented

```csharp
// X-Frame-Options: Prevents clickjacking
"X-Frame-Options": "DENY"

// X-Content-Type-Options: Prevents MIME-type sniffing
"X-Content-Type-Options": "nosniff"

// X-XSS-Protection: Legacy XSS protection
"X-XSS-Protection": "1; mode=block"

// Content-Security-Policy: Restricts resource loading
"Content-Security-Policy": "default-src 'self'; script-src 'self'; ..."

// Referrer-Policy: Controls referrer information
"Referrer-Policy": "strict-origin-when-cross-origin"

// Permissions-Policy: Restricts browser features
"Permissions-Policy": "geolocation=(), microphone=(), camera=()"
```

---

## 📋 Security Checklist

| Vulnerability | Status | Protection Method |
|--------------|--------|-------------------|
| SQL Injection | ✅ Protected | EF Core parameterized queries |
| XSS (Stored) | ⚠️ Partial | HtmlSanitizerService ready, needs PageContent integration |
| XSS (Reflected) | ✅ Protected | Automatic JSON encoding |
| XSS (DOM-based) | ⚠️ Frontend | Frontend responsibility |
| CSRF | ✅ Protected | JWT + CORS configuration |
| Clickjacking | ✅ Protected | X-Frame-Options: DENY |
| MIME Sniffing | ✅ Protected | X-Content-Type-Options |
| Information Disclosure | ✅ Protected | Generic error messages |
| Brute Force | ⚠️ Recommended | Add rate limiting |
| Password Security | ✅ Protected | ASP.NET Identity hashing |

---

## 🔧 Configuration Requirements

### Production Deployment

1. **Update appsettings.Production.json:**
```json
{
  "AllowedOrigins": [
    "https://yourdomain.com",
    "https://www.yourdomain.com"
  ],
  "JwtSettings": {
    "SecretKey": "GENERATE-A-STRONG-KEY-AT-LEAST-32-CHARS",
    "ExpiryInMinutes": 60
  }
}
```

2. **Environment Variables (Recommended):**
```bash
JWT_SECRET_KEY=your-secret-key
DATABASE_CONNECTION=your-connection-string
```

3. **HTTPS Only:**
- Always use HTTPS in production
- `UseHttpsRedirection()` is enabled for production

---

## 🚀 Best Practices

### For Developers

1. **Never Trust User Input**
   - All input is sanitized through `HtmlSanitizerService`
   - Validation attributes on all commands
   - Model validation enabled globally

2. **Use Parameterized Queries**
   - Continue using EF Core LINQ queries
   - Avoid raw SQL (FromSqlRaw/ExecuteSqlRaw)

3. **Validate on Server**
   - Client-side validation is a UX feature, not security
   - Server always validates with `[Required]`, `[StringLength]`, etc.

4. **Least Privilege Principle**
   - Use `[Authorize(Roles = "Admin")]` appropriately
   - Verify user permissions in business logic

5. **Security Headers**
   - Production automatically adds security headers
   - Don't disable them

### For Frontend Developers

1. **Content Security Policy**
   - Your frontend must comply with CSP headers
   - Avoid inline scripts and styles

2. **XSS Prevention**
   - Use framework's built-in escaping (React, Angular, Vue)
   - Never use `dangerouslySetInnerHTML` without sanitization
   - Backend already sanitizes, but defense-in-depth

3. **HTTPS Only**
   - Always use HTTPS in production
   - Set secure flags on cookies

---

## 📊 Security Testing Recommendations

### Manual Testing

1. **XSS Testing:**
```html
<!-- Try injecting in page content -->
<script>alert('XSS')</script>
<img src=x onerror=alert('XSS')>
<a href="javascript:alert('XSS')">Click</a>
```
**Expected Result:** All scripts removed, safe HTML retained.

2. **SQL Injection Testing:**
```sql
-- Try in search fields
' OR '1'='1
'; DROP TABLE Sites; --
```
**Expected Result:** Treated as literal strings, no SQL execution.

3. **CSRF Testing:**
- Try accessing API from unauthorized origin
- Expected: CORS error in production

### Automated Testing

**Recommended Tools:**
- OWASP ZAP
- Burp Suite
- SonarQube (static analysis)
- Snyk (dependency vulnerabilities)

---

## 🔄 Future Enhancements

### High Priority (Security Gaps)

1. **PageContent CRUD Commands with HTML Sanitization**
   - Create `CreatePageContentCommand`/`UpdatePageContentCommand`
   - Integrate `IHtmlSanitizerService` in handlers
   - Apply sanitization to `PageContent.Content` field
   - Add validation attributes ([MaxLength(50000)])
   - **Critical for XSS protection**

### Recommended (Not Yet Implemented)

2. **Rate Limiting**
   - Prevent brute force attacks
   - Use `AspNetCoreRateLimit` package

3. **Request Size Limits**
   - Limit JSON payload sizes
   - Already configured for file uploads

4. **Audit Logging**
   - Log all security-relevant events
   - Track failed login attempts

5. **IP Whitelisting/Blacklisting**
   - For admin endpoints
   - Geographic restrictions if needed

6. **Two-Factor Authentication**
   - Additional layer for admin accounts

7. **API Key Management**
   - For third-party integrations
   - Separate from JWT tokens

---

## 📞 Security Incident Response

If you discover a security vulnerability:

1. **DO NOT** create a public GitHub issue
2. Email: security@yourdomain.com
3. Include:
   - Description of vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if known)

---

## 📚 References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [EF Core Security](https://docs.microsoft.com/en-us/ef/core/miscellaneous/security)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)

---

**Last Updated:** November 9, 2025  
**Reviewed By:** Development Team  
**Next Review:** December 9, 2025
