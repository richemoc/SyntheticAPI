using Microsoft.EntityFrameworkCore;
using SyntheticApi.Data;
using SyntheticApi.DTOs;
using SyntheticApi.Models;

namespace SyntheticApi.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<OrderDto?> GetByIdAsync(int id);
    Task<(OrderDto? order, string? error)> CreateAsync(CreateOrderDto dto);
    Task<OrderDto?> UpdateStatusAsync(int id, OrderStatus status);
}

public class OrderService(AppDbContext db, ILogger<OrderService> logger) : IOrderService
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = new() { OrderStatus.Confirmed, OrderStatus.Cancelled },
        [OrderStatus.Confirmed] = new() { OrderStatus.Shipped, OrderStatus.Cancelled },
        [OrderStatus.Shipped] = new() { OrderStatus.Delivered },
        [OrderStatus.Delivered] = new(),
        [OrderStatus.Cancelled] = new()
    };

    public async Task<IEnumerable<OrderDto>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => ToDto(o))
            .ToListAsync();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? null : ToDto(order);
    }

    public async Task<(OrderDto? order, string? error)> CreateAsync(CreateOrderDto dto)
    {
        if (dto.Items is null || !dto.Items.Any())
            return (null, "Order must contain at least one item.");

        await using var transaction = await db.Database.BeginTransactionAsync();
        var order = new Order
        {
            CustomerName = dto.CustomerName.Trim(),
            CustomerEmail = dto.CustomerEmail.Trim()
        };

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantity <= 0)
                return (null, "Item quantity must be greater than zero.");

            var product = await db.Products.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == itemDto.ProductId && p.IsActive);

            if (product is null)
                return (null, "Product not found.");

            var affectedRows = await db.Products
                .Where(p => p.Id == itemDto.ProductId && p.IsActive && p.Stock >= itemDto.Quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - itemDto.Quantity));

            if (affectedRows == 0)
                return (null, "Insufficient stock for this product.");

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            });
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        logger.LogInformation("Order {OrderId} created for {CustomerEmail}", order.Id, order.CustomerEmail);
        return (ToDto(order), null);
    }

    public async Task<OrderDto?> UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return null;
        if (!AllowedTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(status))
            return null;

        if (status == OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                var product = await db.Products.FindAsync(item.ProductId);
                if (product is not null)
                    product.Stock += item.Quantity;
            }
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation("Order {OrderId} status changed to {Status}", id, status);
        return ToDto(order);
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id,
        o.CustomerName,
        o.CustomerEmail,
        o.Status,
        o.OrderedAt,
        o.Total,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.Product?.Name ?? "", i.Quantity, i.UnitPrice))
    );
}
