using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data.Configurations;

public class LessonHistoryConfiguration : IEntityTypeConfiguration<LessonHistory>
{
    public void Configure(EntityTypeBuilder<LessonHistory> builder)
    {
        builder.HasKey(lh => lh.Id);

        builder.Property(lh => lh.UserId)
            .IsRequired();

        builder.Property(lh => lh.ViewedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(lh => lh.User)
            .WithMany(u => u.LessonHistories)
            .HasForeignKey(lh => lh.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lh => lh.Lesson)
            .WithMany(l => l.LessonHistories)
            .HasForeignKey(lh => lh.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lh => lh.Course)
            .WithMany()
            .HasForeignKey(lh => lh.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
