using System.ComponentModel.DataAnnotations;
using ShopSphere.Domain.Enums;

namespace ShopSphere.Application.DTOs.Orders;

public record OrderItemRequestDto(
    [Required] Guid ProductId,
    [Range(1, 100)] int Quantity
);

public record CreateOrderRequestDto(
    [Required, MinLength(1)] List<OrderItemRequestDto> Items,
    [Required, MaxLength(500)] string ShippingAddress
);

public record UpdateOrderStatusDto(
    [Required] OrderStatus Status
);

public record OrderItemResponseDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal
);

public record OrderResponseDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    string ShippingAddress,
    DateTime CreatedAt,
    List<OrderItemResponseDto> Items
);
