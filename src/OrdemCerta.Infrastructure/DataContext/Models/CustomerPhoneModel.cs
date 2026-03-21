using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.Customers;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class CustomerPhoneModel : IEntityTypeConfiguration<CustomerPhone>
{
    public void Configure(EntityTypeBuilder<CustomerPhone> builder)
    {
        builder.ToTable("customer_phones");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(p => p.Value)
            .HasColumnName("value")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(p => p.AreaCode)
            .HasColumnName("area_code")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(p => p.Number)
            .HasColumnName("number")
            .HasMaxLength(9)
            .IsRequired();
    }
}
