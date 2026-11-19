using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.DTOs.Orders;
using ShopSphere.Application.Interfaces.Services;

namespace ShopSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> PlaceOrder([FromBody] CreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        var order = await _orderService.PlaceOrderAsync(GetCurrentUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetMyOrders(CancellationToken cancellationToken)
        => Ok(await _orderService.GetMyOrdersAsync(GetCurrentUserId(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        return Ok(await _orderService.GetOrderByIdAsync(id, GetCurrentUserId(), isAdmin, cancellationToken));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetAllOrders(CancellationToken cancellationToken)
        => Ok(await _orderService.GetAllOrdersAsync(cancellationToken));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken cancellationToken)
        => Ok(await _orderService.UpdateOrderStatusAsync(id, dto.Status, cancellationToken));
}
