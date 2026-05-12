using foll_backend.Shared.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "device");

        builder.HasKey(m => m.OutboxMessageId);
        builder.Property(m => m.OutboxMessageId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(m => m.Type).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.OccurredOn).IsRequired().HasColumnType("timestamptz");
        builder.Property(m => m.ProcessedOn).HasColumnType("timestamptz");
        builder.Property(m => m.Error).HasMaxLength(2000);
        builder.Property(m => m.RetryCount).IsRequired().HasDefaultValue(0);

        builder.HasIndex(m => m.ProcessedOn);
    }
}
