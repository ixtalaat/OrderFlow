using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Ignore(x => x.LineTotal);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
