using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data.Configurations;

public class PodcastConfiguration : IEntityTypeConfiguration<Podcast>
{
    public void Configure(EntityTypeBuilder<Podcast> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.AudioFileUrl)
            .HasMaxLength(500);

        builder.Property(p => p.AuthorId)
            .IsRequired();

        builder.HasOne(p => p.Author)
            .WithMany(u => u.Podcasts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
