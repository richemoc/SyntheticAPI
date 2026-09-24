using SyntheticApi.Models;

namespace SyntheticApi.DTOs;

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record OrderDto(
    int Id,
    string CustomerName,
    string CustomerEmail,
    OrderStatus Status,
    DateTime OrderedAt,
    decimal Total,
    IEnumerable<OrderItemDto> Items
);

public record CreateOrderDto(
    string CustomerName,
    string CustomerEmail,
    IEnumerable<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(int ProductId, int Quantity);
