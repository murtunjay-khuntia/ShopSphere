using FluentAssertions;
using Moq;
using ShopSphere.Application.DTOs.Products;
using ShopSphere.Application.Interfaces.Repositories;
using ShopSphere.Application.Services;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _uowMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _sut = new ProductService(_uowMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedProducts()
    {
        var products = new List<Product>
        {
            new() { Name = "Laptop", Category = "Tech", Sku = "SKU-1", Price = 1000m, StockQuantity = 5 },
            new() { Name = "Mouse", Category = "Tech", Sku = "SKU-2", Price = 50m, StockQuantity = 20 }
        };
        _productRepoMock.Setup(r => r.GetActiveProductsAsync(null, null, default)).ReturnsAsync(products);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetAllAsync_WithFilters_ShouldPassFiltersToRepository()
    {
        _productRepoMock.Setup(r => r.GetActiveProductsAsync("Audio", "Headphones", default))
            .ReturnsAsync(new List<Product>());

        var result = await _sut.GetAllAsync("Audio", "Headphones");

        result.Should().BeEmpty();
        _productRepoMock.Verify(r => r.GetActiveProductsAsync("Audio", "Headphones", default), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnProductDto()
    {
        var product = new Product { Name = "Keyboard", Sku = "KB-1", Price = 99m, StockQuantity = 10 };
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        result.Id.Should().Be(product.Id);
        result.Sku.Should().Be("KB-1");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Product?)null);

        Func<Task> act = () => _sut.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithUniqueSku_ShouldCreateAndNormalizeSku()
    {
        var dto = new CreateProductDto("Monitor", "4K Display", "Electronics", "mon-01", 300m, 15, null);
        _productRepoMock.Setup(r => r.GetBySkuAsync("MON-01", default)).ReturnsAsync((Product?)null);

        var result = await _sut.CreateAsync(dto);

        result.Sku.Should().Be("MON-01");
        result.Price.Should().Be(300m);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSku_ShouldThrowBadRequestException()
    {
        var dto = new CreateProductDto("Monitor", "Desc", "Electronics", "MON-01", 300m, 15, null);
        _productRepoMock.Setup(r => r.GetBySkuAsync("MON-01", default)).ReturnsAsync(new Product { Sku = "MON-01" });

        Func<Task> act = () => _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*MON-01*");
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_ShouldUpdateFieldsAndSave()
    {
        var product = new Product { Name = "Old", Category = "Cat", Sku = "S1", Price = 10m, StockQuantity = 5 };
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        var dto = new UpdateProductDto("New Name", "New Desc", "New Cat", 25m, 12, "http://img", true);

        var result = await _sut.UpdateAsync(product.Id, dto);

        result.Name.Should().Be("New Name");
        result.Price.Should().Be(25m);
        result.StockQuantity.Should().Be(12);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Product?)null);

        Func<Task> act = () => _sut.UpdateAsync(id, new UpdateProductDto("N", "D", "C", 10m, 1, null, true));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ShouldSoftDeleteProduct()
    {
        var product = new Product { Name = "Item", IsActive = true };
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        await _sut.DeleteAsync(product.Id);

        product.IsActive.Should().BeFalse();
        _productRepoMock.Verify(r => r.Update(product), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
