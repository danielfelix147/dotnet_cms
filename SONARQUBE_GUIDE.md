# SonarQube Setup & Usage Guide

## What is SonarQube?

SonarQube is an automatic code review tool to detect bugs, vulnerabilities, and code smells in your code. It provides:
- **Security Vulnerabilities** - OWASP Top 10, CWE vulnerabilities
- **Code Quality** - Code smells, technical debt
- **Test Coverage** - Percentage of code covered by tests
- **Code Duplication** - Identifies duplicate code blocks
- **Complexity Metrics** - Cyclomatic complexity analysis

---

## Quick Start

### 1. Start SonarQube
```powershell
# Start SonarQube container
docker-compose up -d sonarqube

# Check if it's running (wait 1-2 minutes for startup)
docker-compose logs sonarqube --tail 50
```

### 2. Access SonarQube Web Interface
- **URL**: http://localhost:9000
- **Default Credentials**: 
  - Username: `admin`
  - Password: `admin`
- **Important**: Change the password on first login!

### 3. Create Authentication Token
1. Login to http://localhost:9000
2. Go to: **My Account** (top-right) → **Security**
3. Generate Token:
   - Name: `DOTNET_CMS_Analysis`
   - Type: `Global Analysis Token`
   - Expiration: `No expiration` (or set as needed)
4. Copy the token (you won't see it again!)

### 4. Run Code Analysis
```powershell
# Run analysis with your token
.\Run-SonarQube-Analysis.ps1 -Token "your-token-here"

# Or try with default credentials (first time only)
.\Run-SonarQube-Analysis.ps1
```

---

## What Gets Analyzed

### Security Issues
- **SQL Injection vulnerabilities**
- **XSS (Cross-Site Scripting) risks**
- **Authentication weaknesses**
- **Cryptographic failures**
- **Hard-coded credentials**
- **LDAP/Command injection**
- **Path traversal vulnerabilities**
- **Weak cryptography usage**

### Code Quality
- **Code Smells** - Maintainability issues
- **Bugs** - Potential runtime errors
- **Technical Debt** - Time to fix all issues
- **Complexity** - Methods/classes that are too complex
- **Duplications** - Copy-pasted code

### Best Practices
- **Naming conventions**
- **Exception handling**
- **Resource management**
- **LINQ usage**
- **Null handling**
- **Async/await patterns**

---

## Understanding the Dashboard

### Quality Gate
- **Passed** ✅ - Code meets quality standards
- **Failed** ❌ - Issues need attention

### Metrics
- **Reliability** - Bug count (A-E rating)
- **Security** - Vulnerability count (A-E rating)
- **Security Review** - Security hotspots to review
- **Maintainability** - Code smell count (A-E rating)
- **Coverage** - % of code covered by tests
- **Duplications** - % of duplicated code

### Issue Severity
- **Blocker** 🔴 - Critical issue, must fix immediately
- **Critical** 🟠 - Major issue, high priority
- **Major** 🟡 - Important issue
- **Minor** 🔵 - Minor issue
- **Info** ℹ️ - Suggestion/best practice

---

## Docker Management

### View Logs
```powershell
# Follow logs in real-time
docker-compose logs -f sonarqube

# View last 50 lines
docker-compose logs sonarqube --tail 50
```

### Restart SonarQube
```powershell
docker-compose restart sonarqube
```

### Stop SonarQube
```powershell
docker-compose stop sonarqube
```

### Remove SonarQube (keeps data)
```powershell
docker-compose down sonarqube
```

### Remove SonarQube + Data (clean slate)
```powershell
docker-compose down -v sonarqube
docker volume rm dotnet_cms_sonarqube_data
docker volume rm dotnet_cms_sonarqube_logs
docker volume rm dotnet_cms_sonarqube_extensions
```

---

## Integration with CI/CD

### GitHub Actions Example
```yaml
name: SonarQube Analysis

on:
  push:
    branches: [ master, main ]
  pull_request:
    branches: [ master, main ]

jobs:
  sonarqube:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0  # Shallow clones should be disabled
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Cache SonarQube packages
        uses: actions/cache@v3
        with:
          path: ~\.sonar\cache
          key: ${{ runner.os }}-sonar
      
      - name: Install SonarScanner
        run: dotnet tool install --global dotnet-sonarscanner
      
      - name: Begin SonarQube Analysis
        run: |
          dotnet sonarscanner begin \
            /k:"DOTNET_CMS" \
            /d:sonar.host.url="${{ secrets.SONAR_HOST_URL }}" \
            /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
      
      - name: Build
        run: dotnet build --configuration Release
      
      - name: End SonarQube Analysis
        run: dotnet sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
```

---

## Troubleshooting

### SonarQube Won't Start
```powershell
# Check if port 9000 is already in use
netstat -ano | findstr :9000

# Check logs for errors
docker-compose logs sonarqube

# Increase Docker memory (Docker Desktop → Settings → Resources)
# SonarQube needs at least 2GB RAM
```

### Analysis Fails
```powershell
# Ensure SonarQube is fully started
# Check: http://localhost:9000/api/system/status
# Should return: {"status":"UP"}

# Clear .sonarqube cache
Remove-Item -Recurse -Force .sonarqube

# Rebuild project
dotnet clean
dotnet build
```

### Token Authentication Issues
```powershell
# Generate new token in SonarQube UI
# Go to: My Account → Security → Generate Token

# Test token
curl -u "your-token:" http://localhost:9000/api/authentication/validate
```

### Out of Memory Error
```powershell
# Edit docker-compose.yml, add to sonarqube service:
environment:
  - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true
  - ES_JAVA_OPTS=-Xms512m -Xmx2g
```

---

## Best Practices

### Before Analysis
1. ✅ Ensure all code compiles
2. ✅ Run unit tests
3. ✅ Commit your changes
4. ✅ Use meaningful commit messages

### After Analysis
1. 📊 Review the dashboard
2. 🔴 Fix Blocker/Critical issues first
3. 🧪 Verify fixes don't break tests
4. 📈 Track progress over time
5. 🎯 Set quality gate targets

### Regular Scans
- **Daily**: Automated CI/CD scans
- **Weekly**: Manual review of new issues
- **Monthly**: Technical debt review
- **Quarterly**: Security hotspot review

---

## Key Metrics to Track

| Metric | Target | Why It Matters |
|--------|--------|----------------|
| **Security Rating** | A | No vulnerabilities |
| **Reliability Rating** | A | No bugs |
| **Coverage** | >80% | Good test coverage |
| **Duplications** | <3% | DRY principle |
| **Technical Debt** | <5% | Maintainability |

---

## Additional Resources

### Documentation
- [SonarQube Docs](https://docs.sonarqube.org/)
- [SonarQube for .NET](https://docs.sonarqube.org/latest/analyzing-source-code/scanners/sonarscanner-for-dotnet/)
- [Quality Gate Configuration](https://docs.sonarqube.org/latest/user-guide/quality-gates/)

### Rules
- [C# Rules](https://rules.sonarsource.com/csharp/)
- [Security Rules](https://rules.sonarsource.com/csharp/tag/security/)

---

## Quick Commands Reference

```powershell
# Start SonarQube
docker-compose up -d sonarqube

# Check status
docker-compose ps sonarqube
docker-compose logs sonarqube --tail 20

# Run analysis
.\Run-SonarQube-Analysis.ps1 -Token "your-token"

# View results
start http://localhost:9000

# Stop SonarQube
docker-compose stop sonarqube

# View all containers
docker-compose ps
```

---

**Document Version:** 1.0  
**Last Updated:** November 10, 2025  
**SonarQube Version:** Community Edition (Latest)
