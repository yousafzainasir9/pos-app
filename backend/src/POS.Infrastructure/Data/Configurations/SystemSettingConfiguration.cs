using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities.Settings;

namespace POS.Infrastructure.Data.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Key)
            .IsUnique();

        builder.Property(e => e.Value)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Category)
            .HasMaxLength(50);

        builder.Property(e => e.DataType)
            .HasMaxLength(20);

        builder.Property(e => e.CreatedOn)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");
            
        builder.Property(e => e.ModifiedOn)
            .HasColumnType("datetime2");
            
        builder.Property(e => e.DeletedOn)
            .HasColumnType("datetime2");
    }
}
