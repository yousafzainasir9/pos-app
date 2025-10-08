using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Audit;

namespace POS.Infrastructure.Data.Configurations;

public class SecurityLogConfiguration : IEntityTypeConfiguration<SecurityLog>
{
    public void Configure(EntityTypeBuilder<SecurityLog> builder)
    {
        builder.ToTable("SecurityLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Timestamp)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");

        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.Severity);
        builder.HasIndex(e => e.UserId);

        builder.Property(e => e.EventType)
            .HasConversion<int>();

        builder.Property(e => e.Severity)
            .HasConversion<int>();

        builder.Property(e => e.UserName)
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Store)
            .WithMany()
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
