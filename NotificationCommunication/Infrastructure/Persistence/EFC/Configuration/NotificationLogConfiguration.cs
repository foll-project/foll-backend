using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.NotificationCommunication.Infrastructure.Persistence.EFC.Configuration;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("notification_logs", "notification");

        builder.HasKey(n => n.NotificationLogId);
        builder.Property(n => n.NotificationLogId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(n => n.UserId).IsRequired();

        builder.Property(n => n.NotificationType)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<NotificationType>(v))
            .HasMaxLength(50);

        builder.Property(n => n.NotificationChannel)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<NotificationChannel>(v))
            .HasMaxLength(30);

        builder.Property(n => n.NotificationStatus)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<NotificationStatus>(v))
            .HasMaxLength(30);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(160);
        builder.Property(n => n.Body).IsRequired().HasMaxLength(1000);
        builder.Property(n => n.DataJson).HasColumnType("jsonb");
        builder.Property(n => n.ProviderMessageId).HasMaxLength(500);
        builder.Property(n => n.ErrorMessage).HasMaxLength(2000);
        builder.Property(n => n.DeviceEventId).IsRequired(false);
        builder.Property(n => n.PatientId).IsRequired(false);
        builder.Property(n => n.DeviceId).IsRequired(false);
        builder.Property(n => n.SentAt).HasColumnType("timestamptz");
        builder.Property(n => n.ReadAt).HasColumnType("timestamptz");
        builder.Property(n => n.AcknowledgedAt).HasColumnType("timestamptz");
        builder.Property(n => n.CreatedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(n => n.UpdatedAt).IsRequired().HasColumnType("timestamptz");

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.NotificationStatus);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.PatientId);
        builder.HasIndex(n => n.DeviceId);
    }
}
