using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.ServiceOrders;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class CompanyOrderSequenceModel : IEntityTypeConfiguration<CompanyOrderSequence>
{
    public void Configure(EntityTypeBuilder<CompanyOrderSequence> builder)
    {
        builder.ToTable("company_order_sequences");

        builder.HasKey(e => e.CompanyId);

        builder.Property(e => e.CompanyId)
            .HasColumnName("company_id")
            .ValueGeneratedNever();

        builder.Property(e => e.LastNumber)
            .HasColumnName("last_number")
            .IsRequired();
    }
}
