using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public sealed class ProductInventoryConfiguration : IEntityTypeConfiguration<ProductInventory>
{
    public void Configure(EntityTypeBuilder<ProductInventory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AvailableQuantity).IsRequired();
        builder.HasIndex(x => x.ProductId).IsUnique();
        builder.HasOne(x => x.Product).WithOne(x => x.ProductInventory).HasForeignKey<ProductInventory>(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
