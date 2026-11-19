using ShopSphere.Application.DTOs.Orders;
using ShopSphere.Application.Interfaces.Repositories;
using ShopSphere.Application.Interfaces.Services;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto> PlaceOrderAsync(Guid userId, CreateOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User account not found.");

        // Consolidate duplicate product IDs in the request
        var groupedItems = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0m;

            foreach (var item in groupedItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken)
                    ?? throw new NotFoundException($"Product '{item.ProductId}' was not found.");

                if (!product.IsActive)
                {
                    throw new BadRequestException($"Product '{product.Name}' is no longer available.");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new BadRequestException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}.");
                }

                // Deduct stock (RowVersion concurrency check happens on SaveChangesAsync)
                product.StockQuantity -= item.Quantity;
                _unitOfWork.Products.Update(product);

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                totalAmount += orderItem.SubTotal;
                orderItems.Add(orderItem);
            }

            var order = new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                UserId = user.Id,
                User = user,
                Status = OrderStatus.Confirmed,
                TotalAmount = totalAmount,
                ShippingAddress = request.ShippingAddress.Trim(),
                OrderItems = orderItems
            };

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return MapToDto(order);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId, Guid requesterId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId, cancellationToken)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        if (!isAdmin && order.UserId != requesterId)
        {
            throw new UnauthorizedException("You do not have permission to view this order.");
        }

        return MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId, cancellationToken);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllOrdersWithDetailsAsync(cancellationToken);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId, cancellationToken)
            ?? throw new NotFoundException($"Order '{orderId}' was not found.");

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new BadRequestException("Cancelled orders cannot be modified.");
        }

        if (order.Status == OrderStatus.Delivered && newStatus == OrderStatus.Cancelled)
        {
            throw new BadRequestException("Delivered orders cannot be cancelled.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Restore stock if cancelling an active order
            if (newStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Product.StockQuantity += item.Quantity;
                    _unitOfWork.Products.Update(item.Product);
                }
            }

            order.Status = newStatus;
            _unitOfWork.Orders.Update(order);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return MapToDto(order);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static OrderResponseDto MapToDto(Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.User?.FullName ?? "Customer",
            order.User?.Email ?? string.Empty,
            order.Status.ToString(),
            order.TotalAmount,
            order.ShippingAddress,
            order.CreatedAt,
            order.OrderItems.Select(oi => new OrderItemResponseDto(
                oi.ProductId,
                oi.Product?.Name ?? "Product",
                oi.Quantity,
                oi.UnitPrice,
                oi.SubTotal
            )).ToList()
        );
}
