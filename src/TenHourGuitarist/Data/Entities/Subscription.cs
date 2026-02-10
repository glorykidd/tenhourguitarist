using System.ComponentModel.DataAnnotations;

namespace TenHourGuitarist.Data.Entities;

public class Subscription
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int PackageId { get; set; }

    [MaxLength(200)]
    public string? StripeSubscriptionId { get; set; }

    public SubscriptionStatus Status { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Package Package { get; set; } = null!;
}
