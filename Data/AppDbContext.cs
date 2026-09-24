using Microsoft.EntityFrameworkCore;
using SyntheticApi.Models;

namespace SyntheticApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.HasIndex(p => p.Category);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
            e.HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId);
            e.HasOne(i => i.Product).WithMany(p => p.OrderItems).HasForeignKey(i => i.ProductId);
        });

        // Seed data
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop Pro 15", Description = "High-performance laptop", Price = 1299.99m, Stock = 50, Category = "Electronics" },
            new Product { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 29.99m, Stock = 200, Category = "Accessories" },
            new Product { Id = 3, Name = "USB-C Hub", Description = "7-in-1 USB-C hub", Price = 49.99m, Stock = 150, Category = "Accessories" },
            new Product { Id = 4, Name = "Mechanical Keyboard", Description = "TKL mechanical keyboard", Price = 89.99m, Stock = 75, Category = "Accessories" },
            new Product { Id = 5, Name = "4K Monitor", Description = "27-inch 4K IPS display", Price = 449.99m, Stock = 30, Category = "Electronics" }
        );
    }
}
