using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.Property(e => e.StartTime)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(e => e.EndTime)
            .HasColumnType("datetime2");

        builder.Property(e => e.CreatedOn)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");
            
        builder.Property(e => e.ModifiedOn)
            .HasColumnType("datetime2");
            
        builder.Property(e => e.DeletedOn)
            .HasColumnType("datetime2");

        builder.Property(e => e.StartingCash)
            .HasPrecision(18, 2);

        builder.Property(e => e.EndingCash)
            .HasPrecision(18, 2);

        builder.Property(e => e.CashSales)
            .HasPrecision(18, 2);

        builder.Property(e => e.CardSales)
            .HasPrecision(18, 2);

        builder.Property(e => e.OtherSales)
            .HasPrecision(18, 2);

        builder.Property(e => e.TotalSales)
            .HasPrecision(18, 2);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Shifts)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ClosedByUser)
            .WithMany()
            .HasForeignKey(s => s.ClosedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
