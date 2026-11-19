using ShopSphere.Application.DTOs.Orders;
using ShopSphere.Domain.Enums;

namespace ShopSphere.Application.Interfaces.Services;

public interface IOrderService
{
    Task<OrderResponseDto> PlaceOrderAsync(Guid userId, CreateOrderRequestDto request, CancellationToken cancellationToken = default);
    Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId, Guid requesterId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponseDto>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponseDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, CancellationToken cancellationToken = default);
}
