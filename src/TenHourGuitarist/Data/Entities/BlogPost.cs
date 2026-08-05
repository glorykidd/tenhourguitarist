using System.ComponentModel.DataAnnotations;

namespace TenHourGuitarist.Data.Entities;

public class BlogPost
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? FeaturedImagePath { get; set; }

    public int? CategoryId { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    public DateTime? PublishedDate { get; set; }

    public bool IsPublished { get; set; }

    // Navigation properties
    public BlogCategory? Category { get; set; }
    public ApplicationUser Author { get; set; } = null!;
}
