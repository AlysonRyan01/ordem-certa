using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.Customers;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class CustomerModel : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(c => c.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(254);

        builder.Property(c => c.Document)
            .HasColumnName("document")
            .HasMaxLength(14);

        builder.Property(c => c.DocumentType)
            .HasColumnName("document_type")
            .HasConversion<string>();

        builder.Property(c => c.AddressStreet)
            .HasColumnName("address_street")
            .HasMaxLength(200);

        builder.Property(c => c.AddressNumber)
            .HasColumnName("address_number")
            .HasMaxLength(20);

        builder.Property(c => c.AddressCity)
            .HasColumnName("address_city")
            .HasMaxLength(100);

        builder.Property(c => c.AddressState)
            .HasColumnName("address_state")
            .HasMaxLength(2);

        builder.HasMany(c => c.Phones)
            .WithOne()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(c => c.DomainEvents);
    }
}
