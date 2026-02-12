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

        builder.OwnsOne(c => c.Name, name =>
        {
            name.Property(n => n.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(254);
        });

        builder.OwnsOne(c => c.Document, document =>
        {
            document.Property(d => d.Value)
                .HasColumnName("document")
                .HasMaxLength(14);

            document.Property(d => d.Type)
                .HasColumnName("document_type")
                .HasConversion<string>();
        });

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("address_street")
                .HasMaxLength(200);

            address.Property(a => a.Number)
                .HasColumnName("address_number")
                .HasMaxLength(20);

            address.Property(a => a.City)
                .HasColumnName("address_city")
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("address_state")
                .HasMaxLength(2);
        });

        builder.OwnsMany(c => c.Phones, phones =>
        {
            phones.ToTable("customer_phones");

            phones.WithOwner().HasForeignKey("customer_id");

            phones.Property<int>("id")
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            phones.HasKey("id");

            phones.Property(p => p.Value)
                .HasColumnName("value")
                .HasMaxLength(11)
                .IsRequired();

            phones.Property(p => p.AreaCode)
                .HasColumnName("area_code")
                .HasMaxLength(2)
                .IsRequired();

            phones.Property(p => p.Number)
                .HasColumnName("number")
                .HasMaxLength(9)
                .IsRequired();
        });

        builder.Ignore(c => c.DomainEvents);
    }
}