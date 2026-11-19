using ShopSphere.Application.DTOs.Products;

namespace ShopSphere.Application.Interfaces.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(string? category = null, string? search = null, CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
