using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.MarketingProspects;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class MarketingProspectModel : IEntityTypeConfiguration<MarketingProspect>
{
    public void Configure(EntityTypeBuilder<MarketingProspect> builder)
    {
        builder.ToTable("marketing_prospects");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.PlaceId)
            .HasColumnName("place_id")
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(p => p.PlaceId).IsUnique();

        builder.Property(p => p.BusinessName)
            .HasColumnName("business_name")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.PhoneNumber)
            .HasColumnName("phone_number")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.City)
            .HasColumnName("city")
            .HasMaxLength(200);

        builder.Property(p => p.State)
            .HasColumnName("state")
            .HasMaxLength(200);

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.ContactedAt)
            .HasColumnName("contacted_at");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(p => p.DomainEvents);
    }
}
