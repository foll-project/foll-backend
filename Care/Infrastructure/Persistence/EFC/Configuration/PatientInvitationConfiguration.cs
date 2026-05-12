using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Configuration;

public class PatientInvitationConfiguration : IEntityTypeConfiguration<PatientInvitation>
{
    public void Configure(EntityTypeBuilder<PatientInvitation> builder)
    {
        builder.ToTable("patient_invitations", "care");

        builder.HasKey(i => i.PatientInvitationId);
        builder.Property(i => i.PatientInvitationId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(i => i.PatientId).IsRequired();
        builder.Property(i => i.InvitingUserId).IsRequired();
        builder.Property(i => i.RelationshipTypeId).IsRequired();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<InvitationStatus>(v))
            .HasMaxLength(30);

        builder.Property(i => i.ExpiresAt).IsRequired().HasColumnType("timestamptz");

        builder.HasIndex(i => i.PatientId);
        builder.HasIndex(i => i.InvitingUserId);
    }
}
