using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(e => e.CreatedOn)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");
            
        builder.Property(e => e.ModifiedOn)
            .HasColumnType("datetime2");
            
        builder.Property(e => e.DeletedOn)
            .HasColumnType("datetime2");

        builder.Property(e => e.UnitPriceExGst)
            .HasPrecision(18, 2);

        builder.Property(e => e.UnitGstAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.UnitPriceIncGst)
            .HasPrecision(18, 2);

        builder.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(e => e.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);
    }
}
