using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.ServiceOrders;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class ServiceOrderModel : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("service_orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(o => o.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(o => o.OrderNumber)
            .HasColumnName("order_number")
            .IsRequired();

        builder.OwnsOne(o => o.Equipment, equipment =>
        {
            equipment.Property(e => e.DeviceType)
                .HasColumnName("device_type")
                .HasMaxLength(100)
                .IsRequired();

            equipment.Property(e => e.Brand)
                .HasColumnName("brand")
                .HasMaxLength(100)
                .IsRequired();

            equipment.Property(e => e.Model)
                .HasColumnName("model")
                .HasMaxLength(100)
                .IsRequired();

            equipment.Property(e => e.ReportedDefect)
                .HasColumnName("reported_defect")
                .HasMaxLength(500)
                .IsRequired();

            equipment.Property(e => e.Accessories)
                .HasColumnName("accessories")
                .HasMaxLength(300);

            equipment.Property(e => e.Observations)
                .HasColumnName("observations")
                .HasMaxLength(500);
        });

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(o => o.EntryDate)
            .HasColumnName("entry_date")
            .IsRequired();

        builder.Property(o => o.TechnicianName)
            .HasColumnName("technician_name")
            .HasMaxLength(200);

        builder.OwnsOne(o => o.Budget, budget =>
        {
            budget.Property(b => b.Value)
                .HasColumnName("budget_value")
                .HasColumnType("numeric(10,2)");

            budget.Property(b => b.Description)
                .HasColumnName("budget_description")
                .HasMaxLength(500);
        });

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(o => o.CompanyId);
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);

        builder.Ignore(o => o.DomainEvents);
    }
}
