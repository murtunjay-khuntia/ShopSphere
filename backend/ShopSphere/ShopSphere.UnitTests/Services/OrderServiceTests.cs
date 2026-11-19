using FluentAssertions;
using Moq;
using ShopSphere.Application.DTOs.Orders;
using ShopSphere.Application.Interfaces.Repositories;
using ShopSphere.Application.Services;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();

        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _uowMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _uowMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);

        _sut = new OrderService(_uowMock.Object);
    }

    [Fact]
    public async Task PlaceOrderAsync_WithValidStock_ShouldDeductStockAndCommitTransaction()
    {
        var user = new User { FullName = "Ali", Email = "ali@test.com" };
        var product = new Product { Name = "Headphones", Price = 100m, StockQuantity = 10, IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var request = new CreateOrderRequestDto(
            new List<OrderItemRequestDto> { new(product.Id, 3) },
            "123 Main Street"
        );

        var result = await _sut.PlaceOrderAsync(user.Id, request);

        result.TotalAmount.Should().Be(300m);
        result.Status.Should().Be("Confirmed");
        product.StockQuantity.Should().Be(7);
        _uowMock.Verify(u => u.BeginTransactionAsync(default), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_WithDuplicateProductLines_ShouldConsolidateQuantities()
    {
        var user = new User { FullName = "Ali", Email = "ali@test.com" };
        var product = new Product { Name = "Mouse", Price = 50m, StockQuantity = 5, IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var request = new CreateOrderRequestDto(
            new List<OrderItemRequestDto> { new(product.Id, 2), new(product.Id, 2) },
            "Hyderabad, India"
        );

        var result = await _sut.PlaceOrderAsync(user.Id, request);

        result.TotalAmount.Should().Be(200m);
        product.StockQuantity.Should().Be(1);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenInsufficientStock_ShouldThrowBadRequestAndRollback()
    {
        var user = new User { FullName = "Ali", Email = "ali@test.com" };
        var product = new Product { Name = "Monitor", Price = 300m, StockQuantity = 2, IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var request = new CreateOrderRequestDto(
            new List<OrderItemRequestDto> { new(product.Id, 5) },
            "Address"
        );

        Func<Task> act = () => _sut.PlaceOrderAsync(user.Id, request);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Insufficient stock*");
        _uowMock.Verify(u => u.RollbackTransactionAsync(default), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(default), Times.Never);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenProductInactive_ShouldThrowBadRequestAndRollback()
    {
        var user = new User { FullName = "Ali", Email = "ali@test.com" };
        var product = new Product { Name = "Old Item", Price = 20m, StockQuantity = 10, IsActive = false };

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var request = new CreateOrderRequestDto(new List<OrderItemRequestDto> { new(product.Id, 1) }, "Addr");

        Func<Task> act = () => _sut.PlaceOrderAsync(user.Id, request);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*no longer available*");
        _uowMock.Verify(u => u.RollbackTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenConcurrencyConflictOccurs_ShouldPropagateAndRollback()
    {
        var user = new User { FullName = "Ali", Email = "ali@test.com" };
        var product = new Product { Name = "LED Bar", Price = 45m, StockQuantity = 1, IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ThrowsAsync(new ConcurrencyConflictException("Conflict"));

        var request = new CreateOrderRequestDto(new List<OrderItemRequestDto> { new(product.Id, 1) }, "Addr");

        Func<Task> act = () => _sut.PlaceOrderAsync(user.Id, request);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
        _uowMock.Verify(u => u.RollbackTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        var request = new CreateOrderRequestDto(new List<OrderItemRequestDto> { new(Guid.NewGuid(), 1) }, "Addr");

        Func<Task> act = () => _sut.PlaceOrderAsync(userId, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenCustomerRequestsAnotherUsersOrder_ShouldThrowUnauthorized()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var order = new Order { UserId = ownerId, OrderNumber = "ORD-1" };
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        Func<Task> act = () => _sut.GetOrderByIdAsync(order.Id, otherUserId, isAdmin: false);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenAdminRequestsAnyOrder_ShouldSucceed()
    {
        var order = new Order { UserId = Guid.NewGuid(), OrderNumber = "ORD-1", TotalAmount = 99m };
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.GetOrderByIdAsync(order.Id, Guid.NewGuid(), isAdmin: true);

        result.OrderNumber.Should().Be("ORD-1");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WhenCancelled_ShouldRestoreStockAndCommit()
    {
        var product = new Product { Name = "Keyboard", StockQuantity = 5 };
        var order = new Order
        {
            Status = OrderStatus.Confirmed,
            OrderItems = new List<OrderItem> { new() { ProductId = product.Id, Product = product, Quantity = 3, UnitPrice = 50m } }
        };
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _sut.UpdateOrderStatusAsync(order.Id, OrderStatus.Cancelled);

        result.Status.Should().Be("Cancelled");
        product.StockQuantity.Should().Be(8);
        _uowMock.Verify(u => u.CommitTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WhenAlreadyCancelled_ShouldThrowBadRequest()
    {
        var order = new Order { Status = OrderStatus.Cancelled };
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        Func<Task> act = () => _sut.UpdateOrderStatusAsync(order.Id, OrderStatus.Shipped);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WhenDeliveredAndAttemptingCancel_ShouldThrowBadRequest()
    {
        var order = new Order { Status = OrderStatus.Delivered };
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id, default)).ReturnsAsync(order);

        Func<Task> act = () => _sut.UpdateOrderStatusAsync(order.Id, OrderStatus.Cancelled);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
