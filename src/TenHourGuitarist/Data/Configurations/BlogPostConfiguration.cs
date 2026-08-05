using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(b => b.Slug)
            .IsUnique();

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(b => b.FeaturedImagePath)
            .HasMaxLength(500);

        builder.Property(b => b.AuthorId)
            .IsRequired();

        builder.HasOne(b => b.Category)
            .WithMany(c => c.BlogPosts)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Author)
            .WithMany(u => u.BlogPosts)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
