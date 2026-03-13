using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(l => l.Slug)
            .IsUnique();

        builder.Property(l => l.Subject)
            .HasMaxLength(200);

        builder.Property(l => l.CoverImagePath)
            .HasMaxLength(500);

        builder.Property(l => l.VideoUrl)
            .HasMaxLength(500);

        builder.Property(l => l.FreeVideoUrl)
            .HasMaxLength(500);

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasOne(l => l.Course)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.LessonHistories)
            .WithOne(lh => lh.Lesson)
            .HasForeignKey(lh => lh.LessonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
