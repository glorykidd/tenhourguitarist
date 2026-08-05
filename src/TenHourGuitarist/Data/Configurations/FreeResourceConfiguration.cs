using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data.Configurations;

public class FreeResourceConfiguration : IEntityTypeConfiguration<FreeResource>
{
    public void Configure(EntityTypeBuilder<FreeResource> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(f => f.Slug)
            .IsUnique();

        builder.Property(f => f.CoverImagePath)
            .HasMaxLength(500);

        builder.Property(f => f.VideoUrl)
            .HasMaxLength(500);

        builder.Property(f => f.AuthorId)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasOne(f => f.Author)
            .WithMany(u => u.FreeResources)
            .HasForeignKey(f => f.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
