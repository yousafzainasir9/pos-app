using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(e => e.CreatedOn)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");
            
        builder.Property(e => e.ModifiedOn)
            .HasColumnType("datetime2");
            
        builder.Property(e => e.DeletedOn)
            .HasColumnType("datetime2");

        builder.Property(e => e.TotalPurchases)
            .HasPrecision(18, 2);
    }
}
