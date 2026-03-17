using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.Companies;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class CompanyModel : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.OwnsOne(c => c.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(c => c.Cnpj, cnpj =>
        {
            cnpj.Property(n => n.Value)
                .HasColumnName("cnpj")
                .HasMaxLength(14);
        });

        builder.OwnsOne(c => c.Phone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("phone")
                .HasMaxLength(11)
                .IsRequired();

            phone.Property(p => p.AreaCode)
                .HasColumnName("phone_area_code")
                .HasMaxLength(2)
                .IsRequired();

            phone.Property(p => p.Number)
                .HasColumnName("phone_number")
                .HasMaxLength(9)
                .IsRequired();
        });

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(c => c.Street)
            .HasColumnName("address_street")
            .HasMaxLength(200);

        builder.Property(c => c.Number)
            .HasColumnName("address_number")
            .HasMaxLength(20);

        builder.Property(c => c.City)
            .HasColumnName("address_city")
            .HasMaxLength(100);

        builder.Property(c => c.State)
            .HasColumnName("address_state")
            .HasMaxLength(2);

        builder.Property(c => c.Plan)
            .HasColumnName("plan")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.StripeCustomerId)
            .HasColumnName("stripe_customer_id")
            .HasMaxLength(100);

        builder.Property(c => c.StripeSubscriptionId)
            .HasColumnName("stripe_subscription_id")
            .HasMaxLength(100);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(c => c.DomainEvents);
    }
}
