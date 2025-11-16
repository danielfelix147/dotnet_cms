# Database Cleanup Guide

## Overview

The `Database-Cleanup.sql` script safely cleans up all user-created data while preserving the seed data needed for the application to function.

## What Gets Deleted

- ✅ All Sites and related content
- ✅ All Pages, Products, Destinations, Tours
- ✅ All Images and Files
- ✅ All SiteUsers relationships
- ✅ All PasswordResetTokens
- ✅ All AspNetUsers (except admin@cms.com)
- ✅ All user role assignments (except admin's)

## What Gets Preserved

- ✅ Admin user: `admin@cms.com` / `Admin@123`
- ✅ Admin's role assignments
- ✅ All Roles: Admin, Editor, Viewer
- ✅ All Plugins: PageManagement, ProductManagement, TravelManagement

## How to Run

### Method 1: Using pgAdmin (Recommended)

1. **Open pgAdmin** at http://localhost:5050
   - Login: `admin@cms.com` / `admin123`

2. **Connect to the database** (if not already connected)
   - Right-click "Servers" → "Register" → "Server"
   - General: Name = `CMS Database`
   - Connection: Host = `postgres`, Port = `5432`, Database = `CMS_DB`, Username = `postgres`, Password = `postgres`

3. **Open Query Tool**
   - Navigate to: Servers → CMS Database → Databases → CMS_DB
   - Right-click on `CMS_DB` → "Query Tool" (or press Alt+Shift+Q)

4. **Load the script**
   - Click "Open File" icon (📁) or press Ctrl+O
   - Select `Database-Cleanup.sql`

5. **Run in test mode first** (default behavior)
   - Click "Execute" (▶) or press F5
   - Review the output:
     - "BEFORE CLEANUP" counts
     - "AFTER CLEANUP" counts
     - Admin user verification
     - Available roles and plugins

6. **If results look correct, commit the changes**
   - In the script, find these lines at the bottom:
     ```sql
     -- COMMIT;  -- Uncomment this line to apply changes
     ROLLBACK;  -- Comment this line and uncomment COMMIT above
     ```
   - Change to:
     ```sql
     COMMIT;  -- Uncomment this line to apply changes
     -- ROLLBACK;  -- Comment this line and uncomment COMMIT above
     ```
   - Run the script again (F5)

7. **Verify the cleanup**
   - Check the output tables to confirm:
     - Users: 1 (admin@cms.com)
     - Roles: 3 (Admin, Editor, Viewer)
     - Plugins: 3

### Method 2: Using PostgreSQL Command Line

1. **Connect to the database**
   ```bash
   # If using Docker
   docker exec -it cms_postgres psql -U postgres -d CMS_DB

   # If PostgreSQL is installed locally
   psql -h localhost -U postgres -d CMS_DB
   ```

2. **Run the script**
   ```sql
   \i /path/to/Database-Cleanup.sql
   ```
   OR if inside the container:
   ```bash
   docker cp Database-Cleanup.sql cms_postgres:/tmp/cleanup.sql
   docker exec -it cms_postgres psql -U postgres -d CMS_DB -f /tmp/cleanup.sql
   ```

3. **Review the output** and follow step 6 above to commit if correct

### Method 3: Using PowerShell Script

```powershell
# Run from the project root directory
$script = Get-Content ".\Database-Cleanup.sql" -Raw

# Connect and execute (requires npgsql or similar)
docker exec -i cms_postgres psql -U postgres -d CMS_DB < Database-Cleanup.sql
```

## Safety Features

1. **Transaction-based**: All operations wrapped in BEGIN/ROLLBACK by default
2. **Test mode**: Default behavior shows results without committing
3. **Verification queries**: Shows before/after counts and preserved data
4. **Explicit commit required**: Must manually change ROLLBACK to COMMIT

## Expected Output

After successful cleanup, you should see:

```
AFTER CLEANUP - Record Counts:
┌───────┬───────┬──────────┬──────────────┬───────┬────────┬───────┬────────────┬─────────────────┬───────┬────────────┬───────┬─────────┐
│ sites │ pages │ products │ destinations │ tours │ images │ files │ site_users │ password_tokens │ users │ user_roles │ roles │ plugins │
├───────┼───────┼──────────┼──────────────┼───────┼────────┼───────┼────────────┼─────────────────┼───────┼────────────┼───────┼─────────┤
│     0 │     0 │        0 │            0 │     0 │      0 │     0 │          0 │               0 │     1 │          1 │     3 │       3 │
└───────┴───────┴──────────┴──────────────┴───────┴────────┴───────┴────────────┴─────────────────┴───────┴────────────┴───────┴─────────┘

Admin User Verification:
┌──────────────────────────────────────┬──────────────────┬────────────────┬───────┐
│                  Id                  │      Email       │ EmailConfirmed │ role  │
├──────────────────────────────────────┼──────────────────┼────────────────┼───────┤
│ <guid>                               │ admin@cms.com    │ true           │ Admin │
└──────────────────────────────────────┴──────────────────┴────────────────┴───────┘

Available Roles:
┌──────────────────────────────────────┬─────────┐
│                  Id                  │  Name   │
├──────────────────────────────────────┼─────────┤
│ <guid>                               │ Admin   │
│ <guid>                               │ Editor  │
│ <guid>                               │ Viewer  │
└──────────────────────────────────────┴─────────┘

Available Plugins:
┌──────────────────────────────────────┬───────────────────────┬───────────────────────┬──────────┐
│                  Id                  │      SystemName       │         Name          │ IsActive │
├──────────────────────────────────────┼───────────────────────┼───────────────────────┼──────────┤
│ <guid>                               │ PageManagement        │ Page Management       │ true     │
│ <guid>                               │ ProductManagement     │ Product Management    │ true     │
│ <guid>                               │ TravelManagement      │ Travel Management     │ true     │
└──────────────────────────────────────┴───────────────────────┴───────────────────────┴──────────┘
```

## Common Use Cases

### 1. Testing/Development
Run the cleanup script between test runs to reset to a clean state.

### 2. Demo Preparation
Clean up all test data before a demo, ensuring only the admin user exists.

### 3. Starting Fresh
Remove all content while keeping the application functional with seed data.

### 4. Before Migration
Clean up development data before applying to a fresh database.

## Troubleshooting

### "Admin user not found"
If you see this warning, the admin user doesn't exist. Run the API to re-seed:
```bash
cd CMS.API
dotnet run
```

### "Cannot truncate a table referenced in a foreign key constraint"
This shouldn't happen with DELETE statements, but if it does, the order matters. The script already handles dependencies correctly.

### "Transaction is aborted"
Check for constraint violations or syntax errors in the output.

### To completely reset everything (including seed data)
```sql
-- Drop all tables and recreate from migrations
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
-- Then run: dotnet ef database update --project CMS.Infrastructure --startup-project CMS.API
```

## Quick Verification Queries

After cleanup, you can run these to verify:

```sql
-- Check user count (should be 1)
SELECT COUNT(*) FROM "AspNetUsers";

-- Check admin user exists
SELECT * FROM "AspNetUsers" WHERE "Email" = 'admin@cms.com';

-- Check roles (should be 3)
SELECT * FROM "AspNetRoles" ORDER BY "Name";

-- Check plugins (should be 3)
SELECT * FROM "Plugins" ORDER BY "SystemName";

-- Check all content tables are empty
SELECT 
    (SELECT COUNT(*) FROM "Sites") AS sites,
    (SELECT COUNT(*) FROM "Pages") AS pages,
    (SELECT COUNT(*) FROM "Products") AS products;
```

## Automation

To integrate into your workflow:

```powershell
# Add to your test/demo preparation script
docker exec -i cms_postgres psql -U postgres -d CMS_DB < Database-Cleanup.sql
```

Or in your CI/CD pipeline:
```yaml
- name: Cleanup Database
  run: docker exec -i cms_postgres psql -U postgres -d CMS_DB < Database-Cleanup.sql
```
