using CafePos.Models;
using Microsoft.EntityFrameworkCore;

namespace CafePos.Data;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "cafepos.db3");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.Property(p => p.SKU).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Category).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.HasIndex(o => o.OrderDate);
            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(o => o.Subtotal).HasPrecision(18, 2);
            entity.Property(o => o.Tax).HasPrecision(18, 2);
            entity.Property(o => o.GrandTotal).HasPrecision(18, 2);

            entity.HasMany(o => o.Items)
                  .WithOne(i => i.Order)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(i => i.Product)
                  .WithMany()
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
