using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SyntheticApi.Data;
using SyntheticApi.DTOs;
using SyntheticApi.Models;
using SyntheticApi.Services;

namespace SyntheticApi.Tests;

public class OrderServiceSecurityTests
{
    [Fact]
    public async Task CreateAsync_DoesNotOversellStock()
    {
        await using var connection = await OpenDatabaseAsync();
        await using var db = CreateContext(connection);
        db.Products.Add(new Product
        {
            Name = "Limited product",
            Description = "Test product",
            Price = 10m,
            Stock = 1,
            Category = "Test"
        });
        await db.SaveChangesAsync();

        var productId = await db.Products
            .Where(product => product.Name == "Limited product")
            .Select(product => product.Id)
            .SingleAsync();
        var service = CreateService(db);
        var request = CreateOrder(productId);

        var first = await service.CreateAsync(request);
        var second = await service.CreateAsync(request);

        Assert.NotNull(first.order);
        Assert.Null(second.order);
        Assert.Equal("Insufficient stock for this product.", second.error);
        Assert.Equal(0, await db.Products
            .Where(product => product.Id == productId)
            .Select(product => product.Stock)
            .SingleAsync());
    }

    [Fact]
    public async Task UpdateStatusAsync_RestoresStockWhenOrderIsCancelled()
    {
        await using var connection = await OpenDatabaseAsync();
        await using var db = CreateContext(connection);
        var product = new Product
        {
            Name = "Cancelable product",
            Description = "Test product",
            Price = 10m,
            Stock = 0,
            Category = "Test"
        };
        var order = new Order();
        order.Items.Add(new OrderItem { Product = product, ProductId = product.Id, Quantity = 2, UnitPrice = product.Price });
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var updated = await service.UpdateStatusAsync(order.Id, OrderStatus.Cancelled);

        Assert.Equal(OrderStatus.Cancelled, updated?.Status);
        Assert.Equal(2, await db.Products.Select(savedProduct => savedProduct.Stock).SingleAsync());
    }

    [Fact]
    public async Task UpdateStatusAsync_RejectsBackwardTransitions()
    {
        await using var connection = await OpenDatabaseAsync();
        await using var db = CreateContext(connection);
        var order = new Order { Status = OrderStatus.Delivered };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var updated = await service.UpdateStatusAsync(order.Id, OrderStatus.Pending);

        Assert.Null(updated);
        Assert.Equal(OrderStatus.Delivered, await db.Orders.Select(savedOrder => savedOrder.Status).SingleAsync());
    }

    private static CreateOrderDto CreateOrder(int productId) => new(
        "Test customer",
        "customer@example.com",
        [new CreateOrderItemDto(productId, 1)]);

    private static OrderService CreateService(AppDbContext db) =>
        new(db, NullLogger<OrderService>.Instance);

    private static AppDbContext CreateContext(SqliteConnection connection) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options);

    private static async Task<SqliteConnection> OpenDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();
        return connection;
    }
}
