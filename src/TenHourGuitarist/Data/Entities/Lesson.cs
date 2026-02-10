using System.ComponentModel.DataAnnotations;

namespace TenHourGuitarist.Data.Entities;

public class Lesson
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }

    [MaxLength(500)]
    public string? CoverImagePath { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    [MaxLength(500)]
    public string? FreeVideoUrl { get; set; }

    public int DurationMinutes { get; set; }

    public int DurationSeconds { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Course Course { get; set; } = null!;
    public ICollection<LessonHistory> LessonHistories { get; set; } = [];
}
