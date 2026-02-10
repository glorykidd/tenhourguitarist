using System.ComponentModel.DataAnnotations;

namespace TenHourGuitarist.Data.Entities;

public class LessonHistory
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int LessonId { get; set; }

    public int CourseId { get; set; }

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
