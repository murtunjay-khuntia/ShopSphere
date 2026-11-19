using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE dbo.sp_GetSalesSummary
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        COUNT(*) AS TotalOrders,
                        ISNULL(SUM(CASE WHEN Status <> 'Cancelled' THEN 1 ELSE 0 END), 0) AS CompletedOrders,
                        ISNULL(SUM(CASE WHEN Status = 'Cancelled' THEN 1 ELSE 0 END), 0) AS CancelledOrders,
                        CAST(ISNULL(SUM(CASE WHEN Status <> 'Cancelled' THEN TotalAmount ELSE 0 END), 0) AS DECIMAL(18,2)) AS TotalRevenue,
                        CAST(ISNULL(AVG(CASE WHEN Status <> 'Cancelled' THEN TotalAmount ELSE NULL END), 0) AS DECIMAL(18,2)) AS AverageOrderValue
                    FROM dbo.Orders;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE dbo.sp_GetTopSellingProducts
                    @TopCount INT = 5
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT TOP (@TopCount)
                        p.Id AS ProductId,
                        p.Name AS ProductName,
                        p.Category,
                        ISNULL(SUM(oi.Quantity), 0) AS TotalUnitsSold,
                        CAST(ISNULL(SUM(oi.Quantity * oi.UnitPrice), 0) AS DECIMAL(18,2)) AS TotalRevenue
                    FROM dbo.OrderItems oi
                    INNER JOIN dbo.Orders o ON oi.OrderId = o.Id
                    INNER JOIN dbo.Products p ON oi.ProductId = p.Id
                    WHERE o.Status <> 'Cancelled'
                    GROUP BY p.Id, p.Name, p.Category
                    ORDER BY TotalUnitsSold DESC, TotalRevenue DESC;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_GetSalesSummary;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_GetTopSellingProducts;");
        }
    }
}
