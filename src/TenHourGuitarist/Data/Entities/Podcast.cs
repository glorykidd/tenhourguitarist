using System.ComponentModel.DataAnnotations;

namespace TenHourGuitarist.Data.Entities;

public class Podcast
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
    public string? AudioFileUrl { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    public DateTime? PublishedDate { get; set; }

    public bool IsPublished { get; set; }

    // Navigation properties
    public ApplicationUser Author { get; set; } = null!;
}
