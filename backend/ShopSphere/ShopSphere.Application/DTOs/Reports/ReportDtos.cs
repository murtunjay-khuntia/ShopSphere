namespace ShopSphere.Application.DTOs.Reports;

public class SalesSummaryDto
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class TopProductReportDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalUnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}
