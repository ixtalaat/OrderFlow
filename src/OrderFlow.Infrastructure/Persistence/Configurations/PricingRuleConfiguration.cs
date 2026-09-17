using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public sealed class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tier).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.DiscountPercentage).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.ValidFromUtc).IsRequired();
        builder.HasIndex(x => new { x.ProductId, x.Tier, x.ValidFromUtc });
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
