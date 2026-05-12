using foll_backend.DeviceManagment.Domain.Model.Entities;
using foll_backend.DeviceManagment.Domain.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.DeviceManagment.Infrastructure.Persistence.EFC.Configuration;

public class DeviceEventConfiguration : IEntityTypeConfiguration<DeviceEvent>
{
    public void Configure(EntityTypeBuilder<DeviceEvent> builder)
    {
        builder.ToTable("device_events", "device");

        builder.HasKey(e => e.DeviceEventId);
        builder.Property(e => e.DeviceEventId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(e => e.DeviceId).IsRequired();

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<DeviceEventType>(v))
            .HasMaxLength(50);

        builder.Property(e => e.EventPayload).IsRequired().HasColumnType("jsonb");
        builder.Property(e => e.IsResolved).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(e => e.ResolvedAt).HasColumnType("timestamptz");

        builder.HasIndex(e => e.DeviceId);
        builder.HasIndex(e => new { e.DeviceId, e.EventType, e.IsResolved });
        builder.HasIndex(e => new { e.DeviceId, e.EventType })
            .HasFilter("\"is_resolved\" = false")
            .IsUnique();
    }
}
