using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.Sales;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class SaleModel : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(s => s.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(s => s.SaleNumber)
            .HasColumnName("sale_number")
            .IsRequired();

        builder.Property(s => s.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(s => s.PaymentMethod)
            .HasColumnName("payment_method")
            .IsRequired();

        builder.Property(s => s.TotalValue)
            .HasColumnName("total_value")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.OwnsOne(s => s.Warranty, warranty =>
        {
            warranty.Property(w => w.Duration)
                .HasColumnName("warranty_duration");

            warranty.Property(w => w.Unit)
                .HasColumnName("warranty_unit");
        });

        builder.Property(s => s.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(s => s.SaleDate)
            .HasColumnName("sale_date")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.OwnsMany(s => s.Items, items =>
        {
            items.ToTable("sale_items");

            items.WithOwner().HasForeignKey("sale_id");

            items.HasKey(i => i.Id);

            items.Property(i => i.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            items.Property(i => i.SaleId)
                .HasColumnName("sale_id")
                .IsRequired();

            items.Property(i => i.Description)
                .HasColumnName("description")
                .HasMaxLength(300)
                .IsRequired();

            items.Property(i => i.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            items.Property(i => i.UnitPrice)
                .HasColumnName("unit_price")
                .HasColumnType("numeric(10,2)")
                .IsRequired();

            items.Ignore(i => i.TotalPrice);
        });

        builder.HasIndex(s => s.CompanyId);
        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.Status);

        builder.Ignore(s => s.DomainEvents);
    }
}
