using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CMS.Domain.Entities;
using FileEntity = CMS.Domain.Entities.File;

namespace CMS.Infrastructure.Data;

public class CMSDbContext : IdentityDbContext<IdentityUser>
{
    public CMSDbContext(DbContextOptions<CMSDbContext> options) : base(options)
    {
    }

    public DbSet<Site> Sites => Set<Site>();
    // Removed custom User DbSet - using IdentityUser from IdentityDbContext instead
    public DbSet<Plugin> Plugins => Set<Plugin>();
    public DbSet<SiteUser> SiteUsers => Set<SiteUser>();
    public DbSet<SitePlugin> SitePlugins => Set<SitePlugin>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageContent> PageContents => Set<PageContent>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Destination> Destinations => Set<Destination>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure global query filter for soft delete on all entities with IsDeleted property
        modelBuilder.Entity<Site>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Page>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PageContent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Destination>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tour>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Image>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FileEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Plugin>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SiteUser>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SitePlugin>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PasswordResetToken>().HasQueryFilter(e => !e.IsDeleted);

        // Site configuration
        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Domain).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Domain).IsUnique();
            entity.HasIndex(e => e.IsDeleted); // For soft delete queries
            entity.HasIndex(e => e.CreatedAt); // For sorting
        });

        // Removed custom User entity configuration - using IdentityUser instead

        // SiteUser configuration
        modelBuilder.Entity<SiteUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Site)
                .WithMany(s => s.SiteUsers)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // UserId is a string that references IdentityUser.Id (not our custom User entity)
            // Configure it as a regular property without automatic FK inference
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450); // Standard length for IdentityUser.Id
                
            entity.HasIndex(e => new { e.SiteId, e.UserId }).IsUnique();
        });

        // Plugin configuration
        modelBuilder.Entity<Plugin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SystemName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.SystemName).IsUnique();
            entity.HasIndex(e => e.IsActive); // For filtering active plugins
            entity.HasIndex(e => e.IsDeleted); // For soft delete queries
        });

        // SitePlugin configuration
        modelBuilder.Entity<SitePlugin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Site)
                .WithMany(s => s.SitePlugins)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Plugin)
                .WithMany(p => p.SitePlugins)
                .HasForeignKey(e => e.PluginId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Page configuration
        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PageId).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.SiteId); // For filtering by site
            entity.HasIndex(e => e.PageId); // For filtering by PageId
            entity.HasIndex(e => new { e.SiteId, e.PageId }).IsUnique(); // Composite index
            entity.HasIndex(e => e.IsDeleted); // For soft delete queries
        });

        // PageContent configuration
        modelBuilder.Entity<PageContent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Page)
                .WithMany(p => p.Contents)
                .HasForeignKey(e => e.PageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.SiteId); // For filtering by site
            entity.HasIndex(e => e.ProductId); // For filtering by ProductId
            entity.HasIndex(e => new { e.SiteId, e.ProductId }).IsUnique(); // Composite index
            entity.HasIndex(e => e.IsDeleted); // For soft delete queries
        });

        // Destination configuration
        modelBuilder.Entity<Destination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DestinationId).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Site)
                .WithMany()
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.SiteId); // For filtering by site
            entity.HasIndex(e => e.DestinationId); // For filtering by DestinationId
            entity.HasIndex(e => new { e.SiteId, e.DestinationId }).IsUnique(); // Composite index
            entity.HasIndex(e => e.IsDeleted); // For soft delete queries
        });

        // Tour configuration
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TourId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Destination)
                .WithMany(d => d.Tours)
                .HasForeignKey(e => e.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Image configuration
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(500);
        });

        // File configuration
        modelBuilder.Entity<FileEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(500);
        });

        // PasswordResetToken configuration
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}
