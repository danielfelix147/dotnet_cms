## CMS Project - .NET 8 with Clean Architecture

### Overview
A modular Content Management System built with .NET 8, PostgreSQL, and Blazor following Clean Architecture principles.

### Features
- **Modular Plugin System**: Page Management, Product Management, Travel Management
- **Multi-Site Support**: Manage multiple websites from one platform
- **User Authentication**: ASP.NET Core Identity with JWT tokens
- **Role-Based Access Control**: Admin, Editor, Viewer roles
- **JSON Export**: Generate JSON for each site's content
- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and Presentation layers

### Technology Stack
- .NET 8 / C#
- PostgreSQL
- Entity Framework Core
- ASP.NET Core Web API
- Blazor
- MediatR (CQRS pattern)
- Manual DTO Mapping (Extension Methods)
- Docker & Docker Compose

### Project Structure
```
DOTNET_CMS/
├── CMS.Domain/          # Domain entities, interfaces, and plugin abstractions
├── CMS.Application/     # Application layer with CQRS (Commands/Queries)
├── CMS.Infrastructure/  # Data access, repositories, plugin implementations
├── CMS.API/            # REST API with JWT authentication
├── CMS.UI/             # Blazor UI
├── Dockerfile          # API Docker configuration
└── docker-compose.yml  # Multi-container Docker setup
```

### Getting Started

#### Prerequisites
- .NET 8 SDK
- Docker Desktop
- PostgreSQL (if not using Docker)

#### Running with Docker
```powershell
# Build and start all services
docker-compose up --build

# API will be available at: http://localhost:5000
# pgAdmin will be available at: http://localhost:5050
```

#### Running Locally
```powershell
# Update connection string in appsettings.json
# Run migrations
cd CMS.API
dotnet ef migrations add InitialCreate --project ../CMS.Infrastructure
dotnet ef database update --project ../CMS.Infrastructure

# Run API
dotnet run
```

### Database Migrations
```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> --project CMS.Infrastructure --startup-project CMS.API

# Update database
dotnet ef database update --project CMS.Infrastructure --startup-project CMS.API
```

### API Endpoints

#### Sites
- GET `/api/sites` - Get all sites
- GET `/api/sites/{id}` - Get site by ID
- POST `/api/sites` - Create new site (Admin only)

#### Content Export
- GET `/api/content/site/{siteId}` - Get complete site JSON
- GET `/api/content/site/{siteId}/plugin/{pluginName}` - Get plugin-specific content

### Plugins
The system includes three built-in plugins:

1. **PageManagement** - Manage pages with HTML content, images, and files
2. **ProductManagement** - Manage products with pricing, images, and files  
3. **TravelManagement** - Manage destinations with tours, images, and files

### Configuration
Key settings in `appsettings.json`:
- **ConnectionStrings:DefaultConnection** - PostgreSQL connection
- **JwtSettings** - JWT token configuration

### Docker Services
- **postgres** - PostgreSQL 16 database (port 5432)
- **cms_api** - CMS API (port 5000/5001)
- **pgadmin** - Database management UI (port 5050)

### Future Enhancements
- Blazor admin UI implementation
- File upload handling
- Image optimization
- Plugin marketplace
- Multi-language support
- Content versioning
- Caching layer

### License
This project is provided as-is for development purposes.
