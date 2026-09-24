using System.ComponentModel.DataAnnotations;
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
    [property: Required, StringLength(100)] string CustomerName,
    [property: Required, EmailAddress, StringLength(254)] string CustomerEmail,
    [property: Required, MinLength(1)] IEnumerable<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(
    [property: Range(1, int.MaxValue)] int ProductId,
    [property: Range(1, 1000)] int Quantity
);
