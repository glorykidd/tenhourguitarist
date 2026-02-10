using System.ComponentModel.DataAnnotations;

namespace TenHourGuitarist.Data.Entities;

public class BlogCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    // Navigation properties
    public BlogCategory? Parent { get; set; }
    public ICollection<BlogCategory> Children { get; set; } = [];
    public ICollection<BlogPost> BlogPosts { get; set; } = [];
}
