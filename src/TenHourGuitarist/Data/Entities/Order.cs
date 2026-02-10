using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenHourGuitarist.Data.Entities;

public class Order
{
    public int Id { get; set; }

    public Guid OrderRefId { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int PackageId { get; set; }

    [MaxLength(200)]
    public string? StripeSessionId { get; set; }

    [MaxLength(200)]
    public string? StripePaymentIntentId { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaymentDate { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Package Package { get; set; } = null!;
}
