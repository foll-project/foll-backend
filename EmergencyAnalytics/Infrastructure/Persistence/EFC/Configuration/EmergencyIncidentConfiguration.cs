using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Persistence.EFC.Configuration;

public class EmergencyIncidentConfiguration : IEntityTypeConfiguration<EmergencyIncident>
{
    public void Configure(EntityTypeBuilder<EmergencyIncident> builder)
    {
        builder.ToTable("emergency_incidents", "emergency");

        builder.HasKey(e => e.EmergencyIncidentId);
        builder.Property(e => e.EmergencyIncidentId).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.IncidentKey).IsRequired();
        builder.HasIndex(e => e.IncidentKey).IsUnique();

        builder.Property(e => e.DeviceId).IsRequired();
        builder.Property(e => e.PatientId).IsRequired();

        builder.Property(e => e.FallTypeId)
            .IsRequired()
            .HasColumnType("smallint");

        builder.HasOne(e => e.FallType)
            .WithMany()
            .HasForeignKey(e => e.FallTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<EmergencyIncidentStatus>(v))
            .HasMaxLength(50);

        builder.Property(e => e.OpenedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(e => e.LastSignalAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(e => e.CancelledAt).HasColumnType("timestamptz");
        builder.Property(e => e.ResolvedAt).HasColumnType("timestamptz");
        builder.Property(e => e.ClosedAt).HasColumnType("timestamptz");
        builder.Property(e => e.ClosedByUserId).IsRequired(false);
        builder.Property(e => e.AiConfidenceScore).HasColumnType("numeric(5,4)");
        builder.Property(e => e.Latitude).HasColumnType("numeric(9,6)");
        builder.Property(e => e.Longitude).HasColumnType("numeric(9,6)");
        builder.Property(e => e.CancellationReason)
            .IsRequired(false)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString() : null,
                v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<EmergencyCancellationReason>(v))
            .HasMaxLength(80);
        builder.Property(e => e.FinalObservation).HasMaxLength(500);
        builder.Property(e => e.LastSourcePayload).HasColumnType("jsonb");

        builder.HasIndex(e => e.DeviceId);
        builder.HasIndex(e => e.PatientId);
        builder.HasIndex(e => e.FallTypeId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.DeviceId })
            .HasFilter("\"status\" = 'Open'")
            .IsUnique();
    }
}
