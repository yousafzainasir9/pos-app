using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Domain.Entities;

namespace POS.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.SKU)
            .HasMaxLength(50);

        builder.Property(e => e.Barcode)
            .HasMaxLength(50);

        builder.HasIndex(e => e.SKU)
            .IsUnique()
            .HasFilter("[SKU] IS NOT NULL");

        builder.HasIndex(e => e.Barcode)
            .IsUnique()
            .HasFilter("[Barcode] IS NOT NULL");

        builder.HasIndex(e => new { e.SubcategoryId, e.Slug })
            .IsUnique();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.PriceExGst)
            .HasPrecision(18, 2);

        builder.Property(e => e.GstAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.PriceIncGst)
            .HasPrecision(18, 2);

        builder.Property(e => e.Cost)
            .HasPrecision(18, 2);

        builder.Property(e => e.PackNotes)
            .HasMaxLength(200);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedOn)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("GETDATE()");
            
        builder.Property(e => e.ModifiedOn)
            .HasColumnType("datetime2");
            
        builder.Property(e => e.DeletedOn)
            .HasColumnType("datetime2");

        // Relationships
        builder.HasOne(e => e.Subcategory)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.SubcategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Supplier)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
