using System.ComponentModel.DataAnnotations;

namespace TenHourGuitarist.Data.Entities;

public class Subject
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
