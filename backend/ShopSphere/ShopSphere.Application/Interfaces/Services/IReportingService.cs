using ShopSphere.Application.DTOs.Reports;

namespace ShopSphere.Application.Interfaces.Services;

public interface IReportingService
{
    Task<SalesSummaryDto> GetSalesSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopProductReportDto>> GetTopSellingProductsAsync(int topCount = 5, CancellationToken cancellationToken = default);
}
