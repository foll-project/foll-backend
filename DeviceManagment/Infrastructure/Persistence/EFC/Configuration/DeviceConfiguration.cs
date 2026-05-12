using DeviceEntity = foll_backend.DeviceManagment.Domain.Model.Entities.Device;
using foll_backend.DeviceManagment.Domain.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.DeviceManagment.Infrastructure.Persistence.EFC.Configuration;

public class DeviceConfiguration : IEntityTypeConfiguration<DeviceEntity>
{
    public void Configure(EntityTypeBuilder<DeviceEntity> builder)
    {
        builder.ToTable("devices", "device");

        builder.HasKey(d => d.DeviceId);
        builder.Property(d => d.DeviceId).IsRequired().ValueGeneratedNever();

        builder.Property(d => d.AssignedPatientId).IsRequired(false);
        builder.HasIndex(d => d.AssignedPatientId).IsUnique();

        builder.Property(d => d.FirmwareVersion).IsRequired().HasMaxLength(50);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasColumnType("smallint");

        builder.Property(d => d.ConnectivityStatus)
            .IsRequired(false)
            .HasConversion<short>()
            .HasColumnType("smallint");

        builder.Property(d => d.CurrentBatteryLevel)
            .IsRequired(false)
            .HasColumnType("smallint");

        builder.Property(d => d.IsCharging).IsRequired(false);
        builder.Property(d => d.LastHeartbeatAt).IsRequired(false).HasColumnType("timestamptz");
        builder.Property(d => d.MonitoringStartedAt).IsRequired(false).HasColumnType("timestamptz");
        builder.Property(d => d.LastConnectivityChangeAt).IsRequired(false).HasColumnType("timestamptz");

        builder.HasData(
            new DeviceEntity(1001, "sim-1.0.0", DeviceStatus.Active),
            new DeviceEntity(1002, "sim-1.0.0", DeviceStatus.Active)
        );
    }
}
