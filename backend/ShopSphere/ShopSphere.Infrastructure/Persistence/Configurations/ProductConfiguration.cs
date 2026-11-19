using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Entities;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        // Optimistic Concurrency Token
        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Performance & Uniqueness Indexes
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => new { p.Category, p.IsActive });
    }
}
