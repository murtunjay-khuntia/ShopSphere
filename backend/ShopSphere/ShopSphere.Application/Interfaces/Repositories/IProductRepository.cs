using ShopSphere.Domain.Entities;

namespace ShopSphere.Application.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveProductsAsync(string? category = null, string? search = null, CancellationToken cancellationToken = default);
}
