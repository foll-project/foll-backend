using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.NotificationCommunication.Infrastructure.Persistence.EFC.Configuration;

public class UserPushTokenConfiguration : IEntityTypeConfiguration<UserPushToken>
{
    public void Configure(EntityTypeBuilder<UserPushToken> builder)
    {
        builder.ToTable("user_push_tokens", "notification");

        builder.HasKey(t => t.UserPushTokenId);
        builder.Property(t => t.UserPushTokenId).IsRequired().ValueGeneratedOnAdd();
        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.Token).IsRequired().HasMaxLength(2000);

        builder.Property(t => t.Platform)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<PushPlatform>(v))
            .HasMaxLength(30);

        builder.Property(t => t.DeviceName).HasMaxLength(200);
        builder.Property(t => t.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(t => t.LastUsedAt).HasColumnType("timestamptz");
        builder.Property(t => t.CreatedAt).IsRequired().HasColumnType("timestamptz");
        builder.Property(t => t.UpdatedAt).IsRequired().HasColumnType("timestamptz");

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => new { t.UserId, t.Token }).IsUnique();
    }
}
