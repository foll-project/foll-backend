using System.Text.Json;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.Enums;
using foll_backend.Care.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Configuration;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients", "care");

        builder.HasKey(p => p.PatientId);
        builder.Property(p => p.PatientId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(p => p.Dni).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => p.Dni).IsUnique();

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);

        builder.Property(p => p.BirthDate).IsRequired().HasColumnType("date");

        builder.Property(p => p.BloodType)
            .IsRequired()
            .HasConversion<short>()
            .HasColumnType("smallint");

        builder.Property(p => p.MedicalConditions)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (left, right) =>
                    ReferenceEquals(left, right) ||
                    (left != null && right != null && left.Count == right.Count &&
                     left.OrderBy(kv => kv.Key).SequenceEqual(right.OrderBy(kv => kv.Key))),
                value => value == null
                    ? 0
                    : value.OrderBy(kv => kv.Key)
                        .Aggregate(0, (hash, kv) => HashCode.Combine(hash, kv.Key, kv.Value)),
                value => value == null
                    ? new Dictionary<string, string>()
                    : value.ToDictionary(kv => kv.Key, kv => kv.Value)));

        builder.Property(p => p.OfficialGuardianUserId).IsRequired();
        builder.Property(p => p.CurrentGuardianUserId).IsRequired(false);

        builder.OwnsMany(p => p.Caregivers, navigationBuilder =>
        {
            navigationBuilder.ToTable("user_patients", "care");
            navigationBuilder.WithOwner().HasForeignKey("patient_id");

            navigationBuilder.Property<long>("patient_id");
            navigationBuilder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
            navigationBuilder.Property(c => c.RelationshipTypeId).HasColumnName("relationship_type_id").IsRequired();

            navigationBuilder.HasKey("patient_id", nameof(CaregiverRole.UserId));
        });

        builder.HasMany(p => p.Invitations)
            .WithOne()
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.EmergencyContacts)
            .WithOne()
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
