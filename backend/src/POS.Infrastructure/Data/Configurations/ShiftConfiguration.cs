using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
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
