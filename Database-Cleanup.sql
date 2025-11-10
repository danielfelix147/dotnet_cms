-- ============================================================================
-- PostgreSQL Database Cleanup Script for CMS
-- ============================================================================
-- Purpose: Clean up all user-created data while preserving seed data
-- 
-- What is DELETED:
--   - All Sites and related content (Pages, Products, Destinations, Tours)
--   - All Images and Files
--   - All SiteUsers relationships
--   - All PasswordResetTokens
--   - All user-created AspNetUsers (except admin@cms.com)
--   - All user roles (except those of admin@cms.com)
--
-- What is PRESERVED:
--   - Admin user (admin@cms.com)
--   - Admin's role assignments
--   - All Roles (Admin, Editor, Viewer)
--   - All Plugins (PageManagement, ProductManagement, TravelManagement)
--
-- Usage:
--   1. Connect to your database
--   2. Run this script
--   3. Review the output in the "Messages" tab
--   4. If correct, change ROLLBACK to COMMIT and run again
-- ============================================================================

-- Start transaction for safety
BEGIN;

-- ============================================================================
-- Display current counts BEFORE cleanup
-- ============================================================================

DO $$
DECLARE
    sites_count INT;
    pages_count INT;
    products_count INT;
    destinations_count INT;
    tours_count INT;
    images_count INT;
    files_count INT;
    site_users_count INT;
    password_tokens_count INT;
    users_count INT;
    user_roles_count INT;
    roles_count INT;
    plugins_count INT;
BEGIN
    SELECT COUNT(*) INTO sites_count FROM "Sites";
    SELECT COUNT(*) INTO pages_count FROM "Pages";
    SELECT COUNT(*) INTO products_count FROM "Products";
    SELECT COUNT(*) INTO destinations_count FROM "Destinations";
    SELECT COUNT(*) INTO tours_count FROM "Tours";
    SELECT COUNT(*) INTO images_count FROM "Images";
    SELECT COUNT(*) INTO files_count FROM "Files";
    SELECT COUNT(*) INTO site_users_count FROM "SiteUsers";
    SELECT COUNT(*) INTO password_tokens_count FROM "PasswordResetTokens";
    SELECT COUNT(*) INTO users_count FROM "AspNetUsers";
    SELECT COUNT(*) INTO user_roles_count FROM "AspNetUserRoles";
    SELECT COUNT(*) INTO roles_count FROM "AspNetRoles";
    SELECT COUNT(*) INTO plugins_count FROM "Plugins";
    
    RAISE NOTICE '========================================';
    RAISE NOTICE 'BEFORE CLEANUP - Record Counts';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Sites: %', sites_count;
    RAISE NOTICE 'Pages: %', pages_count;
    RAISE NOTICE 'Products: %', products_count;
    RAISE NOTICE 'Destinations: %', destinations_count;
    RAISE NOTICE 'Tours: %', tours_count;
    RAISE NOTICE 'Images: %', images_count;
    RAISE NOTICE 'Files: %', files_count;
    RAISE NOTICE 'SiteUsers: %', site_users_count;
    RAISE NOTICE 'PasswordResetTokens: %', password_tokens_count;
    RAISE NOTICE 'Users: %', users_count;
    RAISE NOTICE 'UserRoles: %', user_roles_count;
    RAISE NOTICE 'Roles: %', roles_count;
    RAISE NOTICE 'Plugins: %', plugins_count;
    RAISE NOTICE '========================================';
END $$;

-- ============================================================================
-- Step 1: Delete all content entities
-- ============================================================================

-- Delete Tours (depends on Destinations)
DELETE FROM "Tours";

-- Delete PageContents (depends on Pages)
DELETE FROM "PageContents";

-- Delete Destinations (depends on Sites)
DELETE FROM "Destinations";

-- Delete Products (depends on Sites)
DELETE FROM "Products";

-- Delete Pages (depends on Sites)
DELETE FROM "Pages";

-- Delete SitePlugins (depends on Sites and Plugins)
DELETE FROM "SitePlugins";

-- Delete SiteUsers (depends on Sites)
DELETE FROM "SiteUsers";

-- Delete Images (can be attached to Sites)
DELETE FROM "Images";

-- Delete Files (can be attached to Sites)
DELETE FROM "Files";

-- Delete all Sites
DELETE FROM "Sites";

-- Delete all PasswordResetTokens
DELETE FROM "PasswordResetTokens";

-- ============================================================================
-- Step 2: Delete user data (preserve admin user)
-- ============================================================================

DO $$ 
DECLARE 
    admin_user_id TEXT;
BEGIN
    -- Find admin user ID
    SELECT "Id" INTO admin_user_id 
    FROM "AspNetUsers" 
    WHERE "Email" = 'admin@cms.com';

    IF admin_user_id IS NOT NULL THEN
        -- Delete user claims for non-admin users
        DELETE FROM "AspNetUserClaims" 
        WHERE "UserId" != admin_user_id;

        -- Delete user logins for non-admin users
        DELETE FROM "AspNetUserLogins" 
        WHERE "UserId" != admin_user_id;

        -- Delete user tokens for non-admin users
        DELETE FROM "AspNetUserTokens" 
        WHERE "UserId" != admin_user_id;

        -- Delete user roles for non-admin users
        DELETE FROM "AspNetUserRoles" 
        WHERE "UserId" != admin_user_id;

        -- Delete all users except admin
        DELETE FROM "AspNetUsers" 
        WHERE "Email" != 'admin@cms.com';

        RAISE NOTICE 'Preserved admin user: admin@cms.com (ID: %)', admin_user_id;
    ELSE
        RAISE WARNING 'Admin user admin@cms.com not found!';
    END IF;
END $$;

-- ============================================================================
-- Display counts AFTER cleanup and verify preserved data
-- ============================================================================

DO $$
DECLARE
    sites_count INT;
    pages_count INT;
    products_count INT;
    destinations_count INT;
    tours_count INT;
    images_count INT;
    files_count INT;
    site_users_count INT;
    password_tokens_count INT;
    users_count INT;
    user_roles_count INT;
    roles_count INT;
    plugins_count INT;
    admin_email TEXT;
    admin_confirmed BOOLEAN;
    admin_role TEXT;
    role_list TEXT;
    plugin_list TEXT;
BEGIN
    -- Get counts
    SELECT COUNT(*) INTO sites_count FROM "Sites";
    SELECT COUNT(*) INTO pages_count FROM "Pages";
    SELECT COUNT(*) INTO products_count FROM "Products";
    SELECT COUNT(*) INTO destinations_count FROM "Destinations";
    SELECT COUNT(*) INTO tours_count FROM "Tours";
    SELECT COUNT(*) INTO images_count FROM "Images";
    SELECT COUNT(*) INTO files_count FROM "Files";
    SELECT COUNT(*) INTO site_users_count FROM "SiteUsers";
    SELECT COUNT(*) INTO password_tokens_count FROM "PasswordResetTokens";
    SELECT COUNT(*) INTO users_count FROM "AspNetUsers";
    SELECT COUNT(*) INTO user_roles_count FROM "AspNetUserRoles";
    SELECT COUNT(*) INTO roles_count FROM "AspNetRoles";
    SELECT COUNT(*) INTO plugins_count FROM "Plugins";
    
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'AFTER CLEANUP - Record Counts';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Sites: %', sites_count;
    RAISE NOTICE 'Pages: %', pages_count;
    RAISE NOTICE 'Products: %', products_count;
    RAISE NOTICE 'Destinations: %', destinations_count;
    RAISE NOTICE 'Tours: %', tours_count;
    RAISE NOTICE 'Images: %', images_count;
    RAISE NOTICE 'Files: %', files_count;
    RAISE NOTICE 'SiteUsers: %', site_users_count;
    RAISE NOTICE 'PasswordResetTokens: %', password_tokens_count;
    RAISE NOTICE 'Users: %', users_count;
    RAISE NOTICE 'UserRoles: %', user_roles_count;
    RAISE NOTICE 'Roles: %', roles_count;
    RAISE NOTICE 'Plugins: %', plugins_count;
    RAISE NOTICE '========================================';
    
    -- Verify admin user
    SELECT u."Email", u."EmailConfirmed", r."Name" 
    INTO admin_email, admin_confirmed, admin_role
    FROM "AspNetUsers" u
    LEFT JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
    LEFT JOIN "AspNetRoles" r ON ur."RoleId" = r."Id"
    WHERE u."Email" = 'admin@cms.com'
    LIMIT 1;
    
    RAISE NOTICE '';
    RAISE NOTICE 'Admin User Verification:';
    IF admin_email IS NOT NULL THEN
        RAISE NOTICE '  ✓ Email: %', admin_email;
        RAISE NOTICE '  ✓ Email Confirmed: %', admin_confirmed;
        RAISE NOTICE '  ✓ Role: %', COALESCE(admin_role, 'NO ROLE ASSIGNED');
    ELSE
        RAISE WARNING '  ✗ Admin user NOT FOUND!';
    END IF;
    
    -- List all roles
    SELECT string_agg("Name", ', ' ORDER BY "Name") 
    INTO role_list 
    FROM "AspNetRoles";
    
    RAISE NOTICE '';
    RAISE NOTICE 'Available Roles (%): %', roles_count, COALESCE(role_list, 'NONE');
    
    -- List all plugins
    SELECT string_agg("Name", ', ' ORDER BY "SystemName") 
    INTO plugin_list 
    FROM "Plugins";
    
    RAISE NOTICE 'Available Plugins (%): %', plugins_count, COALESCE(plugin_list, 'NONE');
    RAISE NOTICE '========================================';
    
    -- Final summary
    RAISE NOTICE '';
    IF sites_count = 0 AND pages_count = 0 AND users_count = 1 AND 
       roles_count = 3 AND plugins_count = 3 THEN
        RAISE NOTICE '✓ CLEANUP SUCCESSFUL!';
        RAISE NOTICE '  All content deleted, seed data preserved.';
    ELSE
        RAISE WARNING '⚠ UNEXPECTED RESULTS!';
        RAISE WARNING '  Expected: 0 sites, 0 pages, 1 user, 3 roles, 3 plugins';
        RAISE WARNING '  Got: % sites, % pages, % users, % roles, % plugins', 
                     sites_count, pages_count, users_count, roles_count, plugins_count;
    END IF;
    RAISE NOTICE '';
END $$;

-- ============================================================================
-- Commit or Rollback
-- ============================================================================

-- Review the results above in the "Messages" tab.
-- If everything looks correct, COMMIT the transaction.
-- If something went wrong, ROLLBACK instead.

-- COMMIT;  -- Uncomment this line to apply changes
ROLLBACK;  -- Comment this line and uncomment COMMIT above to apply changes

-- ============================================================================
-- Expected Output in Messages Tab:
--
-- BEFORE CLEANUP:
--   Shows current record counts
--
-- AFTER CLEANUP:
--   Sites: 0
--   Pages: 0, Products: 0, Destinations: 0, Tours: 0
--   Images: 0, Files: 0
--   SiteUsers: 0, PasswordResetTokens: 0
--   Users: 1
--   UserRoles: 1
--   Roles: 3 (Admin, Editor, Viewer)
--   Plugins: 3 (Page Management, Product Management, Travel Management)
--
-- Admin User Verification:
--   ✓ Email: admin@cms.com
--   ✓ Email Confirmed: true
--   ✓ Role: Admin
--
-- ✓ CLEANUP SUCCESSFUL!
-- ============================================================================

-- SAFETY NOTE:
-- By default, this script uses ROLLBACK to prevent accidental data loss.
-- To actually apply the cleanup:
--   1. Review the output in the "Messages" tab (not "Data Output")
--   2. If correct, comment out the ROLLBACK line
--   3. Uncomment the COMMIT line
--   4. Run the script again
