using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.NotificationCommunication.Infrastructure.Persistence.EFC.Configuration;

public class SmsNotificationLogConfiguration : IEntityTypeConfiguration<SmsNotificationLog>
{
    public void Configure(EntityTypeBuilder<SmsNotificationLog> builder)
    {
        builder.ToTable("sms_notification_logs", "notification");

        builder.HasKey(log => log.SmsNotificationLogId);
        builder.Property(log => log.SmsNotificationLogId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(log => log.IncidentKey).IsRequired();
        builder.Property(log => log.UserId).IsRequired(false);
        builder.Property(log => log.EmergencyContactId).IsRequired(false);
        builder.Property(log => log.PatientId).IsRequired();
        builder.Property(log => log.DeviceId).IsRequired(false);
        builder.Property(log => log.RecipientName).IsRequired().HasMaxLength(150);
        builder.Property(log => log.PhoneNumber).IsRequired().HasMaxLength(30);

        builder.Property(log => log.NotificationType)
            .IsRequired()
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<NotificationType>(value))
            .HasMaxLength(50);

        builder.Property(log => log.NotificationStatus)
            .IsRequired()
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<NotificationStatus>(value))
            .HasMaxLength(30);

        builder.Property(log => log.Message).IsRequired().HasMaxLength(500);
        builder.Property(log => log.LocationAccessUrl).IsRequired().HasMaxLength(1000);
        builder.Property(log => log.ProviderMessageId).HasMaxLength(500);
        builder.Property(log => log.ErrorMessage).HasMaxLength(2000);
        builder.Property(log => log.SentAt).HasColumnType("timestamptz");
        builder.Property(log => log.CreatedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(log => log.UpdatedAt).IsRequired().HasColumnType("timestamptz");

        builder.HasIndex(log => log.IncidentKey);
        builder.HasIndex(log => log.PatientId);
        builder.HasIndex(log => log.UserId);
        builder.HasIndex(log => log.EmergencyContactId);
        builder.HasIndex(log => log.NotificationStatus);
        builder.HasIndex(log => new { log.IncidentKey, log.PhoneNumber }).IsUnique();
    }
}
