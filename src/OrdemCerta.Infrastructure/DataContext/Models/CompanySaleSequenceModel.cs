using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.Sales;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class CompanySaleSequenceModel : IEntityTypeConfiguration<CompanySaleSequence>
{
    public void Configure(EntityTypeBuilder<CompanySaleSequence> builder)
    {
        builder.ToTable("company_sale_sequences");

        builder.HasKey(s => s.CompanyId);

        builder.Property(s => s.CompanyId)
            .HasColumnName("company_id")
            .ValueGeneratedNever();

        builder.Property(s => s.LastNumber)
            .HasColumnName("last_number")
            .IsRequired();
    }
}
