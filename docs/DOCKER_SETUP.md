# Docker Services Setup Guide

## Quick Start

### Start All Services
```powershell
.\Start-Docker-Services.ps1
```

### Stop All Services
```powershell
.\Start-Docker-Services.ps1 -Down
```

### View Logs
```powershell
.\Start-Docker-Services.ps1 -Logs
```

### Rebuild Services
```powershell
.\Start-Docker-Services.ps1 -Build
```

## Included Services

### 1. PostgreSQL Database
- **Host**: `localhost`
- **Port**: `5432`
- **Database**: `CMS_DB`
- **Username**: `postgres`
- **Password**: `postgres`

**Connection String**:
```
Host=localhost;Port=5432;Database=CMS_DB;Username=postgres;Password=postgres
```

### 2. pgAdmin (PostgreSQL Admin Tool)
- **URL**: http://localhost:5050
- **Login Email**: `admin@cms.com`
- **Login Password**: `admin123`

#### How to Connect to Database in pgAdmin:

1. Open http://localhost:5050 in your browser
2. Login with the credentials above
3. Click **"Add New Server"** (or right-click "Servers" → "Register" → "Server")
4. **General Tab**:
   - Name: `CMS Database` (or any name you prefer)
5. **Connection Tab**:
   - Host: `postgres` (the Docker container name)
   - Port: `5432`
   - Maintenance database: `CMS_DB`
   - Username: `postgres`
   - Password: `postgres`
   - Save password: ✅ (optional)
6. Click **Save**

You can now browse tables, run queries, and manage your database through the web interface!

### 3. MailHog (Email Testing)
- **Web UI**: http://localhost:8025
- **SMTP Server**: `localhost:1025`

All emails sent from the application will be caught by MailHog and displayed in the web interface.

### 4. CMS API (Optional)
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger**: http://localhost:5000/swagger

## Common Tasks

### Check Service Status
```powershell
docker-compose ps
```

### View Service Logs
```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f postgres
docker-compose logs -f pgadmin
docker-compose logs -f mailhog
```

### Restart a Service
```powershell
docker-compose restart postgres
docker-compose restart pgadmin
```

### Remove All Containers and Volumes
```powershell
docker-compose down -v
```
⚠️ **Warning**: This will delete all database data!

### Access PostgreSQL CLI
```powershell
docker exec -it cms_postgres psql -U postgres -d CMS_DB
```

Common PostgreSQL commands:
```sql
\l              -- List all databases
\c CMS_DB       -- Connect to CMS_DB
\dt             -- List all tables
\d Sites        -- Describe Sites table
\q              -- Quit
```

## Troubleshooting

### Port Already in Use
If you get a "port already allocated" error:

1. **Check what's using the port**:
   ```powershell
   netstat -ano | findstr :5432
   netstat -ano | findstr :5050
   ```

2. **Stop the conflicting service** or change the port in `docker-compose.yml`

### pgAdmin Can't Connect to Database
- Make sure you use **`postgres`** as the hostname (not `localhost`)
- The hostname is the Docker service name, not the local machine

### Reset pgAdmin Configuration
```powershell
docker-compose down
docker volume rm dotnet_cms_pgadmin_data
.\Start-Docker-Services.ps1
```

### Database Connection Issues
```powershell
# Check if PostgreSQL is running
docker-compose ps postgres

# View PostgreSQL logs
docker-compose logs postgres

# Test connection
docker exec cms_postgres pg_isready -U postgres
```

## Data Persistence

All data is persisted in Docker volumes:
- `postgres_data`: Database files
- `pgadmin_data`: pgAdmin configuration and server definitions

Even if you stop and remove containers, your data will be preserved unless you use `docker-compose down -v`.

## Production Considerations

For production deployment, you should:
1. ✅ Change default passwords in `docker-compose.yml`
2. ✅ Use environment variables or secrets management
3. ✅ Enable SSL/TLS for database connections
4. ✅ Configure proper backup strategies
5. ✅ Use a reverse proxy (nginx/traefik) for HTTPS
6. ✅ Implement monitoring and logging
7. ✅ Restrict pgAdmin access or disable it in production
