using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Persistence.EFC.Configuration;

public class EmergencyOutboxMessageConfiguration : IEntityTypeConfiguration<EmergencyOutboxMessage>
{
    public void Configure(EntityTypeBuilder<EmergencyOutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "emergency");

        builder.HasKey(m => m.EmergencyOutboxMessageId)
            .HasName("pk_emergency_outbox_messages");
        builder.Property(m => m.EmergencyOutboxMessageId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(m => m.Type).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.OccurredOn).IsRequired().HasColumnType("timestamptz");
        builder.Property(m => m.ProcessedOn).HasColumnType("timestamptz");
        builder.Property(m => m.Error).HasMaxLength(2000);
        builder.Property(m => m.RetryCount).IsRequired().HasDefaultValue(0);

        builder.HasIndex(m => m.ProcessedOn)
            .HasDatabaseName("ix_emergency_outbox_messages_processed_on");
    }
}
