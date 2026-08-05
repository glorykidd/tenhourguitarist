using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TenHourGuitarist.Data.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfileImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }

    // Navigation properties
    public ICollection<Course> Courses { get; set; } = [];
    public ICollection<BlogPost> BlogPosts { get; set; } = [];
    public ICollection<Podcast> Podcasts { get; set; } = [];
    public ICollection<FreeResource> FreeResources { get; set; } = [];
    public ICollection<LessonHistory> LessonHistories { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
