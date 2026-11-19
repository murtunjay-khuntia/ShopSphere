using ShopSphere.Application.DTOs.Products;
using ShopSphere.Application.Interfaces.Repositories;
using ShopSphere.Application.Interfaces.Services;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(string? category = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetActiveProductsAsync(category, search, cancellationToken);
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID '{id}' was not found.");

        return MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedSku = dto.Sku.Trim().ToUpper();
        var existing = await _unitOfWork.Products.GetBySkuAsync(normalizedSku, cancellationToken);
        if (existing is not null)
        {
            throw new BadRequestException($"A product with SKU '{normalizedSku}' already exists.");
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Category = dto.Category.Trim(),
            Sku = normalizedSku,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrl?.Trim(),
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID '{id}' was not found.");

        product.Name = dto.Name.Trim();
        product.Description = dto.Description.Trim();
        product.Category = dto.Category.Trim();
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.ImageUrl = dto.ImageUrl?.Trim();
        product.IsActive = dto.IsActive;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID '{id}' was not found.");

        product.IsActive = false;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ProductDto MapToDto(Product p) =>
        new(p.Id, p.Name, p.Description, p.Category, p.Sku, p.Price, p.StockQuantity, p.ImageUrl, p.IsActive);
}
