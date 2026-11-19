using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Application.DTOs.Products;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    string Sku,
    decimal Price,
    int StockQuantity,
    string? ImageUrl,
    bool IsActive
);

public record CreateProductDto(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string Description,
    [Required, MaxLength(100)] string Category,
    [Required, MaxLength(50)] string Sku,
    [Range(0.01, 1000000)] decimal Price,
    [Range(0, 100000)] int StockQuantity,
    string? ImageUrl
);

public record UpdateProductDto(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string Description,
    [Required, MaxLength(100)] string Category,
    [Range(0.01, 1000000)] decimal Price,
    [Range(0, 100000)] int StockQuantity,
    string? ImageUrl,
    bool IsActive
);
