using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.OrderNumber)
            .IsUnique();

        builder.HasIndex(e => e.OrderDate);

        builder.HasIndex(e => new { e.StoreId, e.OrderDate });

        builder.Property(e => e.Status)
            .HasConversion<int>();

        builder.Property(e => e.OrderType)
            .HasConversion<int>();

        builder.Property(e => e.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.ChangeAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.TableNumber)
            .HasMaxLength(20);

        builder.Property(e => e.CancellationReason)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedOn)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(e => e.Customer)
            .WithMany(e => e.Orders)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Store)
            .WithMany(e => e.Orders)
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany(e => e.Orders)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Shift)
            .WithMany(e => e.Orders)
            .HasForeignKey(e => e.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.OrderItems)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Payments)
            .WithOne(e => e.Order)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
