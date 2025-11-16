# Configuration and Secrets Management

## Overview
This project uses environment-specific configuration management to keep sensitive data secure.

## Configuration Files

### `appsettings.json`
- Checked into source control
- Contains NO sensitive data
- Contains default/development-safe values

### `appsettings.template.json`
- Template showing all required configuration keys
- Use this to create your local configuration

### `appsettings.Development.json` (optional)
- Not checked into source control (in .gitignore)
- Override settings for local development

### `appsettings.Production.json` (optional)
- Not checked into source control (in .gitignore)
- Production-specific settings

## Setting Up Secrets

### Option 1: User Secrets (Development - Recommended)

For local development, use .NET User Secrets:

```powershell
# Navigate to the API project
cd src/CMS.API

# Initialize user secrets
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=CMS_DB;Username=postgres;Password=YourPassword"

# Set JWT secret (minimum 32 characters)
dotnet user-secrets set "JwtSettings:SecretKey" "YourSuperSecretKeyMinimum32CharactersLong!"

# List all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"

# Clear all secrets
dotnet user-secrets clear
```

### Option 2: Environment Variables

Set environment variables in your system or Docker:

#### Windows PowerShell
```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=CMS_DB;Username=postgres;Password=YourPassword"
$env:JwtSettings__SecretKey = "YourSuperSecretKeyMinimum32CharactersLong!"
```

#### Linux/Mac
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=CMS_DB;Username=postgres;Password=YourPassword"
export JwtSettings__SecretKey="YourSuperSecretKeyMinimum32CharactersLong!"
```

#### Docker Compose
```yaml
services:
  cms_api:
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=CMS_DB;Username=postgres;Password=postgres
      - JwtSettings__SecretKey=YourSuperSecretKeyForJWTTokenGeneration123!
```

### Option 3: appsettings.Development.json (Local File)

Create `src/CMS.API/appsettings.Development.json` (already in .gitignore):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=CMS_DB;Username=postgres;Password=YourPassword"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyMinimum32CharactersLong!"
  }
}
```

## Required Secrets

### 1. Database Connection String
```
ConnectionStrings:DefaultConnection
```
Example: `Host=localhost;Port=5432;Database=CMS_DB;Username=postgres;Password=YourPassword`

### 2. JWT Secret Key
```
JwtSettings:SecretKey
```
**Requirements:**
- Minimum 32 characters
- Use strong random string
- Never commit to source control

**Generate a strong key:**
```powershell
# PowerShell
-join ((65..90) + (97..122) + (48..57) + (33..47) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

```bash
# Linux/Mac
openssl rand -base64 64
```

### 3. Email Settings (Optional)
```
EmailSettings:Username
EmailSettings:Password
```
Only needed if using real SMTP (not MailHog for dev).

## Production Deployment

### Azure App Service
Use Application Settings in the portal or via CLI:
```bash
az webapp config appsettings set --name your-app-name --resource-group your-rg \
  --settings ConnectionStrings__DefaultConnection="your-connection-string" \
              JwtSettings__SecretKey="your-secret-key"
```

### AWS Elastic Beanstalk
Use environment properties in the console or `.ebextensions` config files.

### Kubernetes
Use Kubernetes Secrets:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: cms-secrets
type: Opaque
stringData:
  connection-string: "Host=postgres;Port=5432;Database=CMS_DB;Username=postgres;Password=YourPassword"
  jwt-secret: "YourSuperSecretKeyMinimum32CharactersLong!"
```

### Docker
Use Docker secrets or environment variables passed at runtime:
```bash
docker run -e "ConnectionStrings__DefaultConnection=..." -e "JwtSettings__SecretKey=..." cms_api
```

## Configuration Priority

.NET loads configuration in this order (later sources override earlier ones):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments

## Security Best Practices

✅ **DO:**
- Use User Secrets for local development
- Use environment variables or secret managers for production
- Generate strong random secrets (minimum 32 characters)
- Rotate secrets regularly
- Use different secrets for each environment
- Keep `appsettings.json` clean of any secrets

❌ **DON'T:**
- Commit secrets to source control
- Share secrets via email or chat
- Use weak or guessable secrets
- Reuse secrets across environments
- Log secrets in application logs

## Verifying Configuration

To verify your configuration is loaded correctly without exposing secrets:

```csharp
// In Program.cs or Startup
var isDatabaseConfigured = !string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection"));
var isJwtConfigured = !string.IsNullOrEmpty(builder.Configuration["JwtSettings:SecretKey"]);

if (!isDatabaseConfigured)
    throw new InvalidOperationException("Database connection string is not configured!");

if (!isJwtConfigured)
    throw new InvalidOperationException("JWT secret key is not configured!");
```

## Troubleshooting

### "Connection string is empty"
Make sure you've set the connection string using one of the methods above.

### "User secrets not working"
1. Verify you're in the correct project directory
2. Check that `UserSecretsId` exists in `CMS.API.csproj`
3. Run `dotnet user-secrets list` to see stored secrets

### "Environment variables not loaded"
1. Check the naming format (use `__` double underscore, not `:`)
2. Restart your IDE/terminal after setting environment variables
3. Verify with `echo $env:VariableName` (PowerShell) or `echo $VariableName` (bash)

## References

- [Safe storage of app secrets in development in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
