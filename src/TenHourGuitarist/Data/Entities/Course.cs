using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenHourGuitarist.Data.Entities;

public class Course
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CoverImagePath { get; set; }

    [MaxLength(500)]
    public string? TrailerUrl { get; set; }

    public CourseLevel Level { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsFree { get; set; }

    public bool IsPublished { get; set; }

    [Required]
    public string InstructorId { get; set; } = string.Empty;

    // Navigation properties
    public ApplicationUser Instructor { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = [];
}
