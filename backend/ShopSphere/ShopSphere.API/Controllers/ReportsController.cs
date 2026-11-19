using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Application.DTOs.Reports;
using ShopSphere.Application.Interfaces.Services;

namespace ShopSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("sales-summary")]
    public async Task<ActionResult<SalesSummaryDto>> GetSalesSummary(CancellationToken cancellationToken)
        => Ok(await _reportingService.GetSalesSummaryAsync(cancellationToken));

    [HttpGet("top-products")]
    public async Task<ActionResult<IReadOnlyList<TopProductReportDto>>> GetTopProducts([FromQuery] int topCount = 5, CancellationToken cancellationToken = default)
        => Ok(await _reportingService.GetTopSellingProductsAsync(topCount, cancellationToken));
}
