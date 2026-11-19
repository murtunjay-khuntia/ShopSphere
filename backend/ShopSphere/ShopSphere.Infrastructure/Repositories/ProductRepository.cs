using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Interfaces.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetActiveProductsAsync(
        string? category = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.ToLower() == category.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }
}
