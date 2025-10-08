using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Username)
            .IsUnique();

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Phone)
            .HasMaxLength(20);

        builder.Property(e => e.Pin)
            .HasMaxLength(10);

        builder.HasIndex(e => e.Pin)
            .HasFilter("[Pin] IS NOT NULL");

        builder.Property(e => e.Role)
            .HasConversion<int>();

        builder.Property(e => e.RefreshToken)
            .HasMaxLength(500);

        builder.Property(e => e.LastLoginAt)
            .HasColumnType("datetime2");

        builder.Property(e => e.RefreshTokenExpiryTime)
            .HasColumnType("datetime2");

        builder.Property(e => e.CreatedOn)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");
            
        builder.Property(e => e.ModifiedOn)
            .HasColumnType("datetime2");
            
        builder.Property(e => e.DeletedOn)
            .HasColumnType("datetime2");

        // Computed column
        builder.Ignore(e => e.FullName);

        // Relationships
        builder.HasOne(e => e.Store)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.StoreId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
