using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.DTOs.Reports;
using ShopSphere.Application.Interfaces.Services;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Services;

public class ReportingService : IReportingService
{
    private readonly AppDbContext _context;

    public ReportingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.Database
            .SqlQueryRaw<SalesSummaryDto>("EXEC dbo.sp_GetSalesSummary")
            .ToListAsync(cancellationToken);

        return results.FirstOrDefault() ?? new SalesSummaryDto();
    }

    public async Task<IReadOnlyList<TopProductReportDto>> GetTopSellingProductsAsync(int topCount = 5, CancellationToken cancellationToken = default)
    {
        return await _context.Database
            .SqlQuery<TopProductReportDto>($"EXEC dbo.sp_GetTopSellingProducts @TopCount = {topCount}")
            .ToListAsync(cancellationToken);
    }
}
