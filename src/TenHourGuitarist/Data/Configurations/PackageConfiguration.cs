using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Subtitle)
            .HasMaxLength(300);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Property(p => p.DurationUnit)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.StripePriceId)
            .HasMaxLength(200);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);
    }
}
