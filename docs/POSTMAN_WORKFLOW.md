# Postman Workflow - Empty Database to JSON Export

This document outlines the complete step-by-step flow to test the CMS from an empty database (with default seed data) to exporting a JSON configuration for a client.

## Prerequisites

- API running on `http://localhost:5055` (or your configured port)
- Database created and migrations applied
- Default seed data automatically created on startup includes:
  - **Admin user**: `admin@cms.com` / `Admin@123`
  - **Roles**: Admin, Editor, Viewer
  - **Plugins**: PageManagement, ProductManagement, TravelManagement (automatically seeded from DI)

## Environment Variables Setup

Create these Postman environment variables:

```json
{
  "baseUrl": "http://localhost:5055",
  "adminEmail": "admin@cms.com",
  "adminPassword": "Admin@123",
  "token": "",
  "siteId": "",
  "pageId": "",
  "productId": "",
  "destinationId": "",
  "tourId": "",
  "pluginId": ""
}
```

---

## Complete Workflow Steps

### **1. Authentication**

#### 1.1 Login as Admin
```http
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "email": "{{adminEmail}}",
  "password": "{{adminPassword}}"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "refresh-token-guid",
  "email": "admin@cms.com",
  "roles": ["Admin"]
}
```

**Postman Test Script:**
```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.environment.set("token", jsonData.token);
}
```

---

### **2. Create Site**

#### 2.1 Create a New Site
```http
POST {{baseUrl}}/api/sites
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "name": "My Demo Site",
  "domain": "demo.example.com",
  "description": "Demo site for testing JSON export",
  "isActive": true
}
```

**Response:**
```json
{
  "id": "site-guid",
  "name": "My Demo Site",
  "domain": "demo.example.com",
  "description": "Demo site for testing JSON export",
  "isActive": true
}
```

**Postman Test Script:**
```javascript
if (pm.response.code === 201) {
    var jsonData = pm.response.json();
    pm.environment.set("siteId", jsonData.id);
}
```

---

### **3. Manage Plugins** ✨ NEW

Plugins are now automatically seeded into the database on application startup!

#### 3.1 Get All Database Plugins
```http
GET {{baseUrl}}/api/plugins/database
Authorization: Bearer {{token}}
```

**Response:**
```json
[
  {
    "id": "plugin-guid-1",
    "name": "Page Management",
    "systemName": "PageManagement",
    "description": "Manage website pages with content, images, and files",
    "isActive": true
  },
  {
    "id": "plugin-guid-2",
    "name": "Product Management",
    "systemName": "ProductManagement",
    "description": "Manage products with descriptions, pricing, and media",
    "isActive": true
  },
  {
    "id": "plugin-guid-3",
    "name": "Travel Management",
    "systemName": "TravelManagement",
    "description": "Manage destinations and tours",
    "isActive": true
  }
]
```

**Postman Test Script:**
```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    if (jsonData.length > 0) {
        // Save first plugin ID
        pm.environment.set("pluginId", jsonData[0].id);
        
        // Save plugin IDs by system name for convenience
        jsonData.forEach(plugin => {
            if (plugin.systemName === "PageManagement") {
                pm.environment.set("pagePluginId", plugin.id);
            }
            if (plugin.systemName === "ProductManagement") {
                pm.environment.set("productPluginId", plugin.id);
            }
            if (plugin.systemName === "TravelManagement") {
                pm.environment.set("travelPluginId", plugin.id);
            }
        });
    }
}
```

#### 3.2 Enable PageManagement Plugin for Site
```http
POST {{baseUrl}}/api/plugins/site/{{siteId}}/enable/{{pagePluginId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "configuration": null
}
```

#### 3.3 Enable ProductManagement Plugin for Site
```http
POST {{baseUrl}}/api/plugins/site/{{siteId}}/enable/{{productPluginId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "configuration": null
}
```

#### 3.4 Enable TravelManagement Plugin for Site
```http
POST {{baseUrl}}/api/plugins/site/{{siteId}}/enable/{{travelPluginId}}
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "configuration": null
}
```

#### 3.5 Verify Enabled Plugins
```http
GET {{baseUrl}}/api/plugins/site/{{siteId}}
Authorization: Bearer {{token}}
```

**Response:**
```json
[
  {
    "id": "site-plugin-guid-1",
    "pluginId": "plugin-guid-1",
    "pluginName": "Page Management",
    "systemName": "PageManagement",
    "isEnabled": true,
    "configuration": null
  },
  {
    "id": "site-plugin-guid-2",
    "pluginId": "plugin-guid-2",
    "pluginName": "Product Management",
    "systemName": "ProductManagement",
    "isEnabled": true,
    "configuration": null
  },
  {
    "id": "site-plugin-guid-3",
    "pluginId": "plugin-guid-3",
    "pluginName": "Travel Management",
    "systemName": "TravelManagement",
    "isEnabled": true,
    "configuration": null
  }
]
```

---

### **4. Create Content - Pages**

#### 5.1 Create a Page
```http
POST {{baseUrl}}/api/sites/{{siteId}}/pages
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "siteId": "{{siteId}}",
  "title": "Home Page",
  "description": "Welcome to our website",
  "isPublished": true
}
```

**Postman Test Script:**
```javascript
if (pm.response.code === 201) {
    var jsonData = pm.response.json();
    pm.environment.set("pageId", jsonData.id);
}
```

#### 5.2 Add Content to Page
```http
POST {{baseUrl}}/api/sites/{{siteId}}/pages/{{pageId}}/contents
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "pageId": "{{pageId}}",
  "content": "<h1>Welcome</h1><p>This is our homepage content</p>",
  "order": 1
}
```

#### 5.3 Create Another Page
```http
POST {{baseUrl}}/api/sites/{{siteId}}/pages
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "siteId": "{{siteId}}",
  "title": "About Us",
  "description": "Learn more about our company",
  "isPublished": true
}
```

---

### **6. Create Content - Products**

#### 6.1 Create a Product
```http
POST {{baseUrl}}/api/sites/{{siteId}}/products
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "siteId": "{{siteId}}",
  "name": "Premium Widget",
  "description": "High-quality widget with advanced features",
  "price": 99.99,
  "isPublished": true
}
```

**Postman Test Script:**
```javascript
if (pm.response.code === 201) {
    var jsonData = pm.response.json();
    pm.environment.set("productId", jsonData.id);
}
```

#### 6.2 Create Another Product
```http
POST {{baseUrl}}/api/sites/{{siteId}}/products
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "siteId": "{{siteId}}",
  "name": "Starter Kit",
  "description": "Everything you need to get started",
  "price": 49.99,
  "isPublished": true
}
```

---

### **7. Create Content - Destinations & Tours**

#### 7.1 Create a Destination
```http
POST {{baseUrl}}/api/sites/{{siteId}}/destinations
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "siteId": "{{siteId}}",
  "name": "Paris",
  "description": "The City of Light",
  "isPublished": true
}
```

**Postman Test Script:**
```javascript
if (pm.response.code === 201) {
    var jsonData = pm.response.json();
    pm.environment.set("destinationId", jsonData.id);
}
```

#### 7.2 Create a Tour for the Destination
```http
POST {{baseUrl}}/api/sites/{{siteId}}/destinations/{{destinationId}}/tours
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "destinationId": "{{destinationId}}",
  "name": "Eiffel Tower Experience",
  "description": "Visit the iconic Eiffel Tower with a guided tour",
  "price": 79.99,
  "isPublished": true
}
```

**Postman Test Script:**
```javascript
if (pm.response.code === 201) {
    var jsonData = pm.response.json();
    pm.environment.set("tourId", jsonData.id);
}
```

#### 7.3 Create Another Destination
```http
POST {{baseUrl}}/api/sites/{{siteId}}/destinations
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "siteId": "{{siteId}}",
  "name": "Tokyo",
  "description": "Experience modern Japan",
  "isPublished": true
}
```

---

### **8. Add Media (Optional)**

#### 8.1 Upload Image for Page
```http
POST {{baseUrl}}/api/media/images
Authorization: Bearer {{token}}
Content-Type: multipart/form-data

{
  "entityId": "{{pageId}}",
  "entityType": "Page",
  "file": [image file],
  "title": "Homepage Banner",
  "altText": "Welcome banner image"
}
```

*Note: File upload endpoints may need to be created*

---

### **9. Verify Content**

#### 9.1 Get All Pages
```http
GET {{baseUrl}}/api/sites/{{siteId}}/pages
Authorization: Bearer {{token}}
```

#### 9.2 Get All Products
```http
GET {{baseUrl}}/api/sites/{{siteId}}/products
Authorization: Bearer {{token}}
```

#### 9.3 Get All Destinations
```http
GET {{baseUrl}}/api/sites/{{siteId}}/destinations
Authorization: Bearer {{token}}
```

---

### **10. Export JSON Configuration**

#### 10.1 Get Complete Site JSON (Plugin Manager)
```http
GET {{baseUrl}}/api/content/site/{{siteId}}
```

**Response Example:**
```json
{
  "PageManagement": [
    {
      "id": "page-guid",
      "title": "Home Page",
      "description": "Welcome to our website",
      "contents": [
        {
          "content": "<h1>Welcome</h1><p>This is our homepage content</p>",
          "order": 1
        }
      ],
      "images": [],
      "files": []
    }
  ],
  "ProductManagement": [
    {
      "id": "product-guid",
      "name": "Premium Widget",
      "description": "High-quality widget with advanced features",
      "price": 99.99,
      "images": [],
      "files": []
    }
  ],
  "TravelManagement": [
    {
      "id": "destination-guid",
      "name": "Paris",
      "description": "The City of Light",
      "tours": [
        {
          "id": "tour-guid",
          "name": "Eiffel Tower Experience",
          "description": "Visit the iconic Eiffel Tower with a guided tour",
          "price": 79.99
        }
      ],
      "images": []
    }
  ]
}
```

#### 10.2 Get Structured Export (Alternative)
```http
GET {{baseUrl}}/api/content/export/{{siteId}}
```

**Response Example:**
```json
{
  "siteId": "site-guid",
  "name": "My Demo Site",
  "domain": "demo.example.com",
  "exportedAt": "2025-11-09T14:30:00Z",
  "pages": [...],
  "products": [...],
  "destinations": [...],
  "tours": [...],
  "media": [...]
}
```

#### 10.3 Get Plugin-Specific JSON
```http
GET {{baseUrl}}/api/content/site/{{siteId}}/plugin/PageManagement
```

```http
GET {{baseUrl}}/api/content/site/{{siteId}}/plugin/ProductManagement
```

```http
GET {{baseUrl}}/api/content/site/{{siteId}}/plugin/TravelManagement
```

---

## Summary Flow Chart

```
1. Login (Admin) → Get JWT Token
2. Create Site → Get Site ID
3. Get Database Plugins (auto-seeded on startup)
4. Enable Plugins for Site (PageManagement, ProductManagement, TravelManagement)
5. Create Pages
6. Create Products
7. Create Destinations → Add Tours
8. (Optional) Upload Media via MediaController
9. Verify Content via GET endpoints
10. Export JSON:
    - GET /api/content/site/{siteId} → Complete plugin-based JSON
    - GET /api/content/export/{siteId} → Structured export
    - GET /api/content/site/{siteId}/plugin/{pluginName} → Plugin-specific JSON
```

---

## What's New ✨

### Automatic Plugin Seeding
- Plugins are now automatically seeded into the database on application startup
- No manual SQL required!
- Plugins sync from the DI container registration

### PluginsController Endpoints
All plugin management is now fully automated through REST API:

1. **GET /api/plugins** - List registered plugins from DI
2. **GET /api/plugins/database** - List plugins from database with IDs
3. **GET /api/plugins/{systemName}** - Get specific plugin details
4. **GET /api/plugins/site/{siteId}** - Get plugins for a site
5. **POST /api/plugins/site/{siteId}/enable/{pluginId}** - Enable plugin (Admin)
6. **POST /api/plugins/site/{siteId}/disable/{pluginId}** - Disable plugin (Admin)
7. **PUT /api/plugins/site/{siteId}/plugin/{pluginId}/config** - Update configuration (Admin)
8. **POST /api/plugins/sync** - Manual sync utility (Admin)

### MediaController
File upload endpoints already exist:
- **POST /api/media/upload** - Upload images/files with multipart/form-data
- **GET /api/media/site/{siteId}** - Get all media for a site
- **DELETE /api/media/{id}** - Delete media file

---

## Recommendations for Future Enhancements

### Missing Endpoints to Consider:

1. **PageContentsController**
   - `POST /api/sites/{siteId}/pages/{pageId}/contents` - Add page content section
   - `PUT /api/sites/{siteId}/pages/{pageId}/contents/{contentId}` - Update content
   - `DELETE /api/sites/{siteId}/pages/{pageId}/contents/{contentId}` - Delete content
   - `GET /api/sites/{siteId}/pages/{pageId}/contents` - List page contents

2. **Enhanced Plugin Configuration**
   - Plugin-specific configuration schemas
   - Validation for plugin configurations
   - Configuration UI metadata

---

## Client Integration

The exported JSON can be consumed by:

1. **Mobile Apps** - Flutter, React Native apps using the JSON to render dynamic content
2. **Static Site Generators** - Next.js, Gatsby consuming the JSON at build time
3. **SPAs** - React, Vue, Angular apps fetching JSON for dynamic rendering
4. **External Systems** - Third-party integrations consuming structured content

Example client fetch:
```javascript
const response = await fetch('http://localhost:5055/api/content/site/{siteId}');
const siteData = await response.json();

// Render pages
siteData.PageManagement.forEach(page => {
  renderPage(page);
});

// Render products
siteData.ProductManagement.forEach(product => {
  renderProduct(product);
});

// Render destinations
siteData.TravelManagement.forEach(destination => {
  renderDestination(destination);
});
```

---

## Quick Start Guide

### 1. Start the API
```bash
cd CMS.API
dotnet run
```

The API will automatically:
- Create database tables (via migrations)
- Seed Admin user (admin@cms.com / Admin@123)
- Seed roles (Admin, Editor, Viewer)
- Seed plugins (PageManagement, ProductManagement, TravelManagement)

### 2. Import Postman Collection
- Import `DOTNET_CMS.postman_collection.json`
- Set up environment variables
- Run the "Login" request first

### 3. Basic Workflow
1. Login → Get Token
2. Create Site
3. Get Database Plugins → Save Plugin IDs
4. Enable all 3 plugins for your site
5. Create content (pages, products, destinations)
6. Export JSON via `/api/content/site/{siteId}`

---

## Notes

- All authenticated requests require `Authorization: Bearer {{token}}` header
- Plugin registration happens automatically on startup
- MediaController supports multipart/form-data for file uploads
- The system supports multi-tenancy through `siteId` scoping
- All endpoints follow RESTful conventions
- Soft delete is implemented - deleted items remain in database with `IsDeleted = true`
