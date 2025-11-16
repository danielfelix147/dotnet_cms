# CMS Domain Design Overview

## Entity Relationship Diagram

```mermaid
erDiagram
    Site ||--o{ SiteUser : "has many"
    Site ||--o{ SitePlugin : "has many"
    Site ||--o{ Page : "manages"
    Site ||--o{ Product : "manages"
    Site ||--o{ Destination : "manages"
    
    User ||--o{ SiteUser : "has access to"
    Plugin ||--o{ SitePlugin : "enabled on"
    
    Page ||--o{ PageContent : "contains"
    Page ||--o{ Image : "has"
    Page ||--o{ File : "has"
    
    Product ||--o{ Image : "has"
    Product ||--o{ File : "has"
    
    Destination ||--o{ Tour : "offers"
    Destination ||--o{ Image : "has"
    Destination ||--o{ File : "has"
    
    Tour ||--o{ Image : "has"
    Tour ||--o{ File : "has"

    Site {
        Guid Id PK
        string Name
        string Domain UK
        string Description
        bool IsActive
        DateTime CreatedAt
        DateTime UpdatedAt
        string CreatedBy
        string UpdatedBy
        bool IsDeleted
    }

    User {
        Guid Id PK
        string Email UK
        string FirstName
        string LastName
        string Role
        bool IsActive
        DateTime CreatedAt
    }

    SiteUser {
        Guid Id PK
        Guid SiteId FK
        Guid UserId FK
        string Role
        DateTime CreatedAt
    }

    Plugin {
        Guid Id PK
        string Name
        string SystemName UK
        string Description
        bool IsActive
    }

    SitePlugin {
        Guid Id PK
        Guid SiteId FK
        Guid PluginId FK
        bool IsEnabled
        string Configuration
    }

    Page {
        Guid Id PK
        Guid SiteId FK
        string PageId
        string Title
        string Description
        bool IsPublished
    }

    PageContent {
        Guid Id PK
        Guid PageId FK
        string ContentId
        string Content
        int Order
    }

    Product {
        Guid Id PK
        Guid SiteId FK
        string ProductId
        string Name
        string Description
        decimal Price
        bool IsPublished
    }

    Destination {
        Guid Id PK
        Guid SiteId FK
        string DestinationId
        string Name
        string Description
        bool IsPublished
    }

    Tour {
        Guid Id PK
        Guid DestinationId FK
        string TourId
        string Name
        string Description
        decimal Price
        bool IsPublished
    }

    Image {
        Guid Id PK
        string ImageId
        string Location
        string AltText
        string Title
        long FileSize
        string MimeType
        Guid EntityId
        string EntityType
    }

    File {
        Guid Id PK
        string FileId
        string Location
        string Title
        long FileSize
        string MimeType
        Guid EntityId
        string EntityType
    }
```

---

## Architecture Layers

```mermaid
graph TB
    subgraph "Presentation Layer"
        API[CMS.API - REST API]
        UI[CMS.UI - Blazor]
    end
    
    subgraph "Application Layer"
        Commands[Commands - CQRS]
        Queries[Queries - CQRS]
        Handlers[Handlers - MediatR]
        DTOs[DTOs]
        Mappings[Extension Methods - Manual Mapping]
    end
    
    subgraph "Domain Layer"
        Entities[Domain Entities]
        Interfaces[Repository Interfaces]
        PluginInterfaces[Plugin Interfaces]
        BaseEntity[Base Entity - Audit]
    end
    
    subgraph "Infrastructure Layer"
        DbContext[CMSDbContext - EF Core]
        Repositories[Repository Implementations]
        Plugins[Plugin Implementations]
        UnitOfWork[Unit of Work]
    end
    
    subgraph "Database"
        PostgreSQL[(PostgreSQL Database)]
    end

    API --> Commands
    API --> Queries
    UI --> Commands
    UI --> Queries
    
    Commands --> Handlers
    Queries --> Handlers
    Handlers --> Mappings
    Handlers --> Interfaces
    Mappings --> DTOs
    Mappings --> Entities
    
    Interfaces --> Repositories
    Repositories --> DbContext
    DbContext --> PostgreSQL
    
    Repositories --> Entities
    Plugins --> PluginInterfaces
    Entities --> BaseEntity
    
    UnitOfWork --> DbContext

    style API fill:#e1f5ff
    style UI fill:#e1f5ff
    style Commands fill:#fff4e1
    style Queries fill:#fff4e1
    style Handlers fill:#fff4e1
    style Entities fill:#e8f5e9
    style DbContext fill:#f3e5f5
    style PostgreSQL fill:#ffebee
```

---

## Plugin System Architecture

```mermaid
graph LR
    subgraph "Plugin Manager"
        PM[PluginManager]
    end
    
    subgraph "Plugin Interfaces"
        IPlugin[IPlugin]
        IContentPlugin[IContentPlugin]
        BasePlugin[BasePlugin Abstract]
    end
    
    subgraph "Concrete Plugins"
        PagePlugin[PageManagementPlugin]
        ProductPlugin[ProductManagementPlugin]
        TravelPlugin[TravelManagementPlugin]
    end
    
    subgraph "Database Context"
        DB[(CMSDbContext)]
    end
    
    PM --> PagePlugin
    PM --> ProductPlugin
    PM --> TravelPlugin
    
    PagePlugin --> IContentPlugin
    ProductPlugin --> IContentPlugin
    TravelPlugin --> IContentPlugin
    
    IContentPlugin --> BasePlugin
    BasePlugin --> IPlugin
    
    PagePlugin --> DB
    ProductPlugin --> DB
    TravelPlugin --> DB

    style PM fill:#e1f5ff
    style IPlugin fill:#fff4e1
    style PagePlugin fill:#e8f5e9
    style ProductPlugin fill:#e8f5e9
    style TravelPlugin fill:#e8f5e9
    style DB fill:#ffebee
```

---

## CQRS Flow

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant MediatR
    participant Handler
    participant Repository
    participant UnitOfWork
    participant Database

    rect rgb(200, 230, 255)
        Note over Client,Database: Command Flow (Write)
        Client->>API: POST /api/sites (CreateSiteCommand)
        API->>MediatR: Send(command)
        MediatR->>Handler: CreateSiteCommandHandler
        Handler->>Repository: AddAsync(site)
        Handler->>UnitOfWork: SaveChangesAsync()
        UnitOfWork->>Database: INSERT INTO Sites
        Database-->>UnitOfWork: Success
        UnitOfWork-->>Handler: Result
        Handler-->>MediatR: SiteDto
        MediatR-->>API: SiteDto
        API-->>Client: 201 Created
    end

    rect rgb(200, 255, 200)
        Note over Client,Database: Query Flow (Read)
        Client->>API: GET /api/sites/{id} (GetSiteByIdQuery)
        API->>MediatR: Send(query)
        MediatR->>Handler: GetSiteByIdQueryHandler
        Handler->>Repository: GetByIdAsync(id)
        Repository->>Database: SELECT FROM Sites
        Database-->>Repository: Site Entity
        Repository-->>Handler: Site
        Handler->>Handler: Map to SiteDto
        Handler-->>MediatR: SiteDto
        MediatR-->>API: SiteDto
        API-->>Client: 200 OK
    end
```

---

## Manual Mapping Architecture

The CMS uses **manual mapping with extension methods** instead of AutoMapper for better performance and maintainability.

```mermaid
graph LR
    subgraph "Domain Entities"
        Site[Site Entity]
        Page[Page Entity]
        Product[Product Entity]
    end
    
    subgraph "Mapping Extensions"
        SiteMap[SiteMappings.ToDto]
        PageMap[PageMappings.ToDto]
        ProductMap[ProductMappings.ToDto]
    end
    
    subgraph "DTOs"
        SiteDto[SiteDto]
        PageDto[PageDto]
        ProductDto[ProductDto]
    end
    
    subgraph "Handlers"
        QueryHandler[Query Handler]
        CommandHandler[Command Handler]
    end
    
    Site --> SiteMap
    Page --> PageMap
    Product --> ProductMap
    
    SiteMap --> SiteDto
    PageMap --> PageDto
    ProductMap --> ProductDto
    
    QueryHandler --> SiteMap
    CommandHandler --> SiteMap
    
    style SiteMap fill:#e8f5e9
    style PageMap fill:#e8f5e9
    style ProductMap fill:#e8f5e9
```

### Mapping Benefits

| Aspect | Manual Mapping | AutoMapper |
|--------|---------------|------------|
| **Performance** | ✅ No reflection overhead | ❌ Uses reflection |
| **Compile-time safety** | ✅ Type-checked | ⚠️ Runtime errors possible |
| **Debugging** | ✅ Easy to trace | ❌ Complex stack traces |
| **Dependencies** | ✅ Zero dependencies | ❌ Package dependency |
| **Licensing** | ✅ No licensing costs | ⚠️ Commercial license required (v15+) |
| **Maintenance** | ✅ Explicit and clear | ⚠️ Magic conventions |

### Example Usage

```csharp
// Extension method in CMS.Application/Mappings/SiteMappings.cs
public static SiteDto ToDto(this Site site)
{
    return new SiteDto
    {
        Id = site.Id,
        Name = site.Name,
        Domain = site.Domain,
        Description = site.Description,
        IsActive = site.IsActive
    };
}

// Usage in handler
public async Task<SiteDto?> Handle(GetSiteByIdQuery request, CancellationToken cancellationToken)
{
    var site = await _siteRepository.GetByIdAsync(request.SiteId);
    return site?.ToDto(); // Clean, explicit mapping
}
```

---

## Content Export Flow

```mermaid
sequenceDiagram
    participant Client
    participant ContentController
    participant PluginManager
    participant Plugin
    participant DbContext
    participant Database

    Client->>ContentController: GET /api/content/site/{siteId}/pages
    ContentController->>PluginManager: GetPluginBySystemName("PageManagement")
    PluginManager-->>ContentController: IContentPlugin
    
    ContentController->>Plugin: GenerateContentJson(siteId)
    
    Plugin->>DbContext: Pages.Include(Contents).Where(siteId)
    DbContext->>Database: SELECT with JOIN
    Database-->>DbContext: Pages + PageContents
    DbContext-->>Plugin: Page Entities
    
    Plugin->>DbContext: Images.Where(EntityType="Page")
    DbContext->>Database: SELECT Images
    Database-->>DbContext: Image Entities
    DbContext-->>Plugin: Images
    
    Plugin->>DbContext: Files.Where(EntityType="Page")
    DbContext->>Database: SELECT Files
    Database-->>DbContext: File Entities
    DbContext-->>Plugin: Files
    
    Plugin->>Plugin: Build JSON Structure
    Plugin-->>ContentController: JSON string
    ContentController-->>Client: 200 OK (JSON)
```

---

## Multi-Tenancy Model

```mermaid
graph TD
    subgraph "Multi-Site Support"
        Site1[Site: example.com]
        Site2[Site: shop.com]
        Site3[Site: travel.com]
    end
    
    subgraph "Shared Users"
        User1[User: admin@cms.com]
        User2[User: editor@cms.com]
    end
    
    subgraph "Available Plugins"
        P1[Page Management]
        P2[Product Management]
        P3[Travel Management]
    end
    
    subgraph "Site 1 Content"
        Pages1[Pages]
    end
    
    subgraph "Site 2 Content"
        Pages2[Pages]
        Products2[Products]
    end
    
    subgraph "Site 3 Content"
        Destinations3[Destinations]
        Tours3[Tours]
    end
    
    User1 --> Site1
    User1 --> Site2
    User1 --> Site3
    User2 --> Site1
    User2 --> Site2
    
    Site1 --> P1
    Site2 --> P1
    Site2 --> P2
    Site3 --> P3
    
    Site1 --> Pages1
    Site2 --> Pages2
    Site2 --> Products2
    Site3 --> Destinations3
    Destinations3 --> Tours3

    style Site1 fill:#e3f2fd
    style Site2 fill:#f3e5f5
    style Site3 fill:#e8f5e9
    style User1 fill:#fff9c4
    style User2 fill:#fff9c4
```

---

## Entity Inheritance Structure

```mermaid
classDiagram
    class BaseEntity {
        <<abstract>>
        +Guid Id
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +string? CreatedBy
        +string? UpdatedBy
        +bool IsDeleted
    }

    class Site {
        +string Name
        +string Domain
        +string? Description
        +bool IsActive
        +ICollection~SiteUser~ SiteUsers
        +ICollection~SitePlugin~ SitePlugins
    }

    class User {
        +string Email
        +string FirstName
        +string LastName
        +string Role
        +bool IsActive
        +ICollection~SiteUser~ SiteUsers
    }

    class Page {
        +Guid SiteId
        +string PageId
        +string Title
        +string? Description
        +bool IsPublished
        +Site Site
        +ICollection~PageContent~ Contents
    }

    class PageContent {
        +Guid PageId
        +string ContentId
        +string Content
        +int Order
        +Page Page
    }

    class Product {
        +Guid SiteId
        +string ProductId
        +string Name
        +string? Description
        +decimal Price
        +bool IsPublished
        +Site Site
    }

    class Destination {
        +Guid SiteId
        +string DestinationId
        +string Name
        +string? Description
        +bool IsPublished
        +Site Site
        +ICollection~Tour~ Tours
    }

    class Tour {
        +Guid DestinationId
        +string TourId
        +string Name
        +string? Description
        +decimal Price
        +bool IsPublished
        +Destination Destination
    }

    class Plugin {
        +string Name
        +string SystemName
        +string? Description
        +bool IsActive
        +ICollection~SitePlugin~ SitePlugins
    }

    class SiteUser {
        +Guid SiteId
        +Guid UserId
        +string Role
        +Site Site
        +User User
    }

    class SitePlugin {
        +Guid SiteId
        +Guid PluginId
        +bool IsEnabled
        +string? Configuration
        +Site Site
        +Plugin Plugin
    }

    class Image {
        +string ImageId
        +string Location
        +string? AltText
        +string? Title
        +long FileSize
        +string? MimeType
        +Guid EntityId
        +string EntityType
    }

    class File {
        +string FileId
        +string Location
        +string? Title
        +long FileSize
        +string? MimeType
        +Guid EntityId
        +string EntityType
    }

    BaseEntity <|-- Site
    BaseEntity <|-- User
    BaseEntity <|-- Plugin
    BaseEntity <|-- Page
    BaseEntity <|-- PageContent
    BaseEntity <|-- Product
    BaseEntity <|-- Destination
    BaseEntity <|-- Tour
    BaseEntity <|-- SiteUser
    BaseEntity <|-- SitePlugin
    BaseEntity <|-- Image
    BaseEntity <|-- File
```

---

## Key Design Patterns

### 1. **Repository Pattern**
- Abstracts data access logic
- `IRepository<T>` generic interface
- Specific repositories: `ISiteRepository`, `IUserRepository`, etc.

### 2. **Unit of Work Pattern**
- Manages transactions across multiple repositories
- Ensures atomic operations
- `IUnitOfWork` interface with `SaveChangesAsync()`

### 3. **CQRS (Command Query Responsibility Segregation)**
- **Commands**: Modify state (CreateSiteCommand)
- **Queries**: Read state (GetAllSitesQuery, GetSiteByIdQuery)
- Handled by MediatR

### 4. **Plugin Pattern**
- Extensible plugin system
- `IPlugin` → `IContentPlugin` → `BasePlugin`
- Concrete implementations: Page, Product, Travel plugins
- Managed by `PluginManager`

### 5. **Clean Architecture**
- **Domain**: Entities, interfaces (no dependencies)
- **Application**: Business logic, CQRS handlers (depends on Domain)
- **Infrastructure**: Data access, EF Core (depends on Domain)
- **Presentation**: API, UI (depends on Application)

### 6. **Soft Delete Pattern**
- `IsDeleted` flag on `BaseEntity`
- Data never physically removed
- Audit trail preserved

### 7. **Audit Pattern**
- Automatic tracking via `BaseEntity`
- `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`
- Timestamps in UTC

### 8. **Multi-Tenancy Pattern**
- Site-based isolation
- Shared user pool across sites
- `SiteUser` junction table with roles
- Content scoped by `SiteId`

---

## Domain Rules & Constraints

### Unique Constraints
- `Site.Domain` - Each site must have unique domain
- `User.Email` - Each user must have unique email
- `Plugin.SystemName` - Each plugin must have unique system name

### Required Fields
- All entities require `Id` (Guid)
- All entities require `CreatedAt` (DateTime)
- Site requires `Name` and `Domain`
- User requires `Email`, `FirstName`, `LastName`, `Role`

### Business Rules
1. **Site Activation**: Sites can be activated/deactivated via `IsActive`
2. **Publishing**: Pages, Products, Destinations, Tours have `IsPublished` flag
3. **Ordering**: PageContent has `Order` field for sequencing
4. **Pricing**: Product and Tour use `decimal(18,2)` for price precision
5. **Media Polymorphism**: Images and Files use `EntityType` + `EntityId` for flexible associations
6. **Role-Based Access**: Users have different roles per site via `SiteUser.Role`
7. **Plugin Enablement**: Plugins can be enabled per site with custom configuration

---

## Database Indexes

Based on query patterns, the following indexes are recommended:

```sql
-- Primary Keys (automatic)
PK on all Id columns

-- Unique Indexes
UNIQUE INDEX on Sites.Domain
UNIQUE INDEX on Users.Email
UNIQUE INDEX on Plugins.SystemName

-- Foreign Key Indexes (automatic in most cases)
INDEX on SiteUsers.SiteId
INDEX on SiteUsers.UserId
INDEX on SitePlugins.SiteId
INDEX on SitePlugins.PluginId
INDEX on Pages.SiteId
INDEX on PageContents.PageId
INDEX on Products.SiteId
INDEX on Destinations.SiteId
INDEX on Tours.DestinationId

-- Query Optimization Indexes
INDEX on Images(EntityType, EntityId)
INDEX on Files(EntityType, EntityId)
INDEX on Pages(SiteId, IsPublished)
INDEX on Products(SiteId, IsPublished)
INDEX on Destinations(SiteId, IsPublished)
```

---

## Scalability Considerations

### Horizontal Scaling
- API layer is stateless (can run multiple instances)
- PostgreSQL read replicas for query scaling
- Redis caching for frequently accessed data

### Vertical Scaling
- Database connection pooling configured in `appsettings.json`
- Async operations throughout (async/await pattern)
- EF Core query optimization with `.AsNoTracking()` for read-only queries

### Content Delivery
- Static files (Images, Files) should be served from CDN
- JSON export cached with expiration policy
- Consider adding `ETag` headers for cache validation

### Multi-Tenancy at Scale
- Site-based data partitioning
- Separate database per site (future enhancement)
- Shared infrastructure with resource quotas

---

## Future Enhancements

1. **Versioning**: Page/Product version history
2. **Workflow**: Content approval workflow
3. **Localization**: Multi-language support per site
4. **Search**: Full-text search with Elasticsearch
5. **Analytics**: Track page views, product views
6. **Media Processing**: Image resizing, format conversion
7. **Webhooks**: Notify external systems on content changes
8. **GraphQL**: Alternative API layer
9. **Event Sourcing**: Audit log of all changes
10. **Real-time**: SignalR for live content updates

---

## Technology Stack Summary

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Domain** | C# Records/Classes | Pure business entities |
| **Application** | MediatR | CQRS implementation |
| **Application** | Extension Methods | Manual DTO mapping (no AutoMapper) |
| **Infrastructure** | EF Core 8 | ORM |
| **Infrastructure** | Npgsql | PostgreSQL provider |
| **Database** | PostgreSQL 16 | Relational database |
| **API** | ASP.NET Core 8 | REST API |
| **Authentication** | ASP.NET Identity + JWT | User authentication |
| **UI** | Blazor Server | Admin interface |
| **Containerization** | Docker | Deployment |
| **Testing** | xUnit + Testcontainers | Unit & integration tests |

