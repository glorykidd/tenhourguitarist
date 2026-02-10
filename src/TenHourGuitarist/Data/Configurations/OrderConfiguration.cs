using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderRefId)
            .HasDefaultValueSql("NEWID()");

        builder.HasIndex(o => o.OrderRefId)
            .IsUnique();

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.StripeSessionId)
            .HasMaxLength(200);

        builder.Property(o => o.StripePaymentIntentId)
            .HasMaxLength(200);

        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.Amount)
            .HasPrecision(18, 2);

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Package)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.PackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
