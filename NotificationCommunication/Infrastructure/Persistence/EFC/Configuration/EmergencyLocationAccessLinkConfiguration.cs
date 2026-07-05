using foll_backend.NotificationCommunication.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.NotificationCommunication.Infrastructure.Persistence.EFC.Configuration;

public class EmergencyLocationAccessLinkConfiguration : IEntityTypeConfiguration<EmergencyLocationAccessLink>
{
    public void Configure(EntityTypeBuilder<EmergencyLocationAccessLink> builder)
    {
        builder.ToTable("emergency_location_access_links", "notification");

        builder.HasKey(link => link.EmergencyLocationAccessLinkId);
        builder.Property(link => link.EmergencyLocationAccessLinkId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(link => link.IncidentKey).IsRequired();
        builder.Property(link => link.PatientId).IsRequired();
        builder.Property(link => link.DeviceId).IsRequired(false);
        builder.Property(link => link.TokenHash).IsRequired().HasMaxLength(128);
        builder.Property(link => link.ExpiresAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(link => link.CreatedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(link => link.LastAccessedAt).HasColumnType("timestamptz");
        builder.Property(link => link.RevokedAt).HasColumnType("timestamptz");

        builder.HasIndex(link => link.TokenHash).IsUnique();
        builder.HasIndex(link => link.IncidentKey);
        builder.HasIndex(link => link.ExpiresAt);
    }
}
