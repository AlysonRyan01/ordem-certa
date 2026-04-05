using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrdemCerta.Domain.ServiceOrders;

namespace OrdemCerta.Infrastructure.DataContext.Models;

public class ServiceOrderNotificationModel : IEntityTypeConfiguration<ServiceOrderNotification>
{
    public void Configure(EntityTypeBuilder<ServiceOrderNotification> builder)
    {
        builder.ToTable("service_order_notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(n => n.ServiceOrderId)
            .HasColumnName("service_order_id")
            .IsRequired();

        builder.Property(n => n.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(n => n.RecipientType)
            .HasColumnName("recipient_type")
            .IsRequired();

        builder.Property(n => n.RecipientName)
            .HasColumnName("recipient_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.SentAt)
            .HasColumnName("sent_at")
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(n => n.ServiceOrderId);
        builder.HasIndex(n => n.CompanyId);
    }
}
