## CMS Project - .NET 8 with Clean Architecture

### Overview
A modular Content Management System built with .NET 8, PostgreSQL, and Blazor following Clean Architecture principles.

### Features
- **Modular Plugin System**: Page Management, Product Management, Travel Management
- **Automatic Plugin Seeding**: Plugins auto-registered from DI container to database
- **Multi-Site Support**: Manage multiple websites from one platform
- **User Authentication**: ASP.NET Core Identity with JWT tokens
- **Role-Based Access Control**: Admin, Editor, Viewer roles
- **JSON Export**: Generate structured JSON exports for each site's content
- **Media Management**: Upload and manage images and files with proper content type handling
- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and Presentation layers
- **CQRS Pattern**: Command/Query separation using MediatR
- **Soft Deletes**: All entities support soft delete with IsDeleted flag
- **Workflow Automation**: PowerShell scripts for complete test automation cycles

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

### API Endpoints (50 Total)

#### Authentication
- POST `/api/auth/login` - Login and get JWT token
- POST `/api/auth/register` - Register new user (Admin only)
- POST `/api/auth/refresh` - Refresh JWT token

#### Sites Management
- GET `/api/sites` - Get all sites
- GET `/api/sites/{id}` - Get site by ID
- POST `/api/sites` - Create new site (Admin only)
- PUT `/api/sites/{id}` - Update site (Admin only)
- DELETE `/api/sites/{id}` - Delete site (Admin only)

#### Plugins Management ✨ NEW
- GET `/api/plugins` - List available plugins from DI
- GET `/api/plugins/database` - List plugins from database with IDs
- POST `/api/plugins/site/{siteId}/enable/{pluginId}` - Enable plugin for site
- POST `/api/plugins/site/{siteId}/disable/{pluginId}` - Disable plugin for site
- PUT `/api/plugins/{id}` - Update plugin settings
- GET `/api/plugins/site/{siteId}` - Get enabled plugins for site

#### Pages Management (PageManagement Plugin)
- GET `/api/sites/{siteId}/pages` - Get all pages
- GET `/api/sites/{siteId}/pages/{id}` - Get page by ID
- POST `/api/sites/{siteId}/pages` - Create page (Admin, Editor)
- PUT `/api/sites/{siteId}/pages/{id}` - Update page (Admin, Editor)
- DELETE `/api/sites/{siteId}/pages/{id}` - Delete page (Admin, Editor)

#### Products Management (ProductManagement Plugin)
- GET `/api/sites/{siteId}/products` - Get all products
- GET `/api/sites/{siteId}/products/{id}` - Get product by ID
- POST `/api/sites/{siteId}/products` - Create product (Admin, Editor)
- PUT `/api/sites/{siteId}/products/{id}` - Update product (Admin, Editor)
- DELETE `/api/sites/{siteId}/products/{id}` - Delete product (Admin, Editor)

#### Travel Management (TravelManagement Plugin)
- GET `/api/sites/{siteId}/destinations` - Get all destinations
- GET `/api/sites/{siteId}/destinations/{id}` - Get destination by ID
- POST `/api/sites/{siteId}/destinations` - Create destination (Admin, Editor)
- PUT `/api/sites/{siteId}/destinations/{id}` - Update destination (Admin, Editor)
- DELETE `/api/sites/{siteId}/destinations/{id}` - Delete destination (Admin, Editor)
- GET `/api/sites/{siteId}/destinations/{destinationId}/tours` - Get tours
- POST `/api/sites/{siteId}/destinations/{destinationId}/tours` - Create tour
- PUT `/api/sites/{siteId}/destinations/{destinationId}/tours/{id}` - Update tour
- DELETE `/api/sites/{siteId}/destinations/{destinationId}/tours/{tourId}` - Delete tour

#### Media Management
- POST `/api/media/upload` - Upload image/file (multipart/form-data)
- GET `/api/media/site/{siteId}` - Get all media for site
- DELETE `/api/media/{id}?mediaType=Image|File` - Delete media

#### Content Export
- GET `/api/content/export/{siteId}` - Export complete site as structured JSON
- GET `/api/content/site/{siteId}` - Get raw site content JSON
- GET `/api/content/site/{siteId}/plugin/{pluginSystemName}` - Get plugin-specific content

#### User Management
- GET `/api/users` - Get all users (Admin only)
- GET `/api/users/{id}` - Get user by ID (Admin only)
- POST `/api/users` - Create user (Admin only)
- PUT `/api/users/{id}` - Update user (Admin only)
- DELETE `/api/users/{id}` - Delete user (Admin only)
- GET `/api/sites/{siteId}/users` - Get site users (Admin, Editor)
- POST `/api/sites/{siteId}/users` - Add user to site (Admin)
- DELETE `/api/sites/{siteId}/users/{userId}` - Remove user from site (Admin)

### Plugins
The system includes three built-in plugins with automatic database seeding:

1. **PageManagement** - Manage pages with HTML content, images, and files
2. **ProductManagement** - Manage products with pricing, images, and files  
3. **TravelManagement** - Manage destinations with tours, images, and files

**Plugin Features:**
- ✅ Automatic plugin seeding on startup from DI container
- ✅ Enable/disable plugins per site
- ✅ Plugin-specific configuration storage
- ✅ Database tracking with unique IDs
- ✅ RESTful API for plugin management

### Automation Scripts ✨ NEW

#### Run-CMS-Workflow.ps1
Complete workflow automation from empty database to JSON export:
```powershell
.\Run-CMS-Workflow.ps1 -BaseUrl "http://localhost:5055" `
                       -AdminEmail "admin@cms.com" `
                       -AdminPassword "Admin@123"
```

**Features:**
- Automated 12-step workflow
- Idempotent site creation (reuses existing sites)
- Creates sample content (pages, products, destinations, tours)
- Exports site configuration to JSON
- Proper error handling and progress indicators

**Steps:**
1. Admin login → JWT token
2. Check/create site
3. Fetch available plugins
4-6. Enable PageManagement, ProductManagement, TravelManagement plugins
7. Create sample page
8. Create sample product
9. Create sample destination
10. Create sample tour
11. Check media endpoint
12. Export site to JSON file

#### Cleanup-CMS-Data.ps1
Automated cleanup script that deletes all resources from an exported site:
```powershell
.\Cleanup-CMS-Data.ps1 -ExportFile "site_export_xyz.json" `
                       -BaseUrl "http://localhost:5055" `
                       -AdminEmail "admin@cms.com" `
                       -AdminPassword "Admin@123"
```

**Features:**
- Reads export JSON file
- Deletes resources in proper order (child → parent): Tours → Destinations → Products → Pages → Media → Site
- Graceful error handling (continues on 404s)
- Progress tracking with counters
- Perfect for test automation cycles

### Test Suite
**Total Tests**: 291 passing ✅
- Domain Tests: Full entity validation coverage
- Application Tests: CQRS handlers for all operations
- Infrastructure Tests: Repository and data access layer
- Integration Tests: End-to-end API workflow tests

**Test Coverage:**
- Sites CRUD operations
- Pages CRUD operations
- Products CRUD operations
- Destinations & Tours CRUD operations
- Plugin management
- Authentication & Authorization
- Content export functionality

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
