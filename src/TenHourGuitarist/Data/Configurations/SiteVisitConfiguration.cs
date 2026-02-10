using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data.Configurations;

public class SiteVisitConfiguration : IEntityTypeConfiguration<SiteVisit>
{
    public void Configure(EntityTypeBuilder<SiteVisit> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => s.Date)
            .IsUnique();

        builder.Property(s => s.Count)
            .HasDefaultValue(0);
    }
}
