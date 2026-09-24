using Microsoft.EntityFrameworkCore;
using SyntheticApi.Data;
using SyntheticApi.DTOs;
using SyntheticApi.Models;

namespace SyntheticApi.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(int id);
    Task<(OrderDto? order, string? error)> CreateAsync(CreateOrderDto dto);
    Task<OrderDto?> UpdateStatusAsync(int id, OrderStatus status);
}

public class OrderService(AppDbContext db) : IOrderService
{
    public async Task<IEnumerable<OrderDto>> GetAllAsync() =>
        await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderedAt)
            .Select(o => ToDto(o))
            .ToListAsync();

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? null : ToDto(order);
    }

    public async Task<(OrderDto? order, string? error)> CreateAsync(CreateOrderDto dto)
    {
        var order = new Order
        {
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail
        };

        foreach (var itemDto in dto.Items)
        {
            var product = await db.Products.FindAsync(itemDto.ProductId);
            if (product is null)
                return (null, $"Product {itemDto.ProductId} not found.");
            if (product.Stock < itemDto.Quantity)
                return (null, $"Insufficient stock for '{product.Name}'. Available: {product.Stock}.");

            product.Stock -= itemDto.Quantity;
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
        return (ToDto(order), null);
    }

    public async Task<OrderDto?> UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return null;
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
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
