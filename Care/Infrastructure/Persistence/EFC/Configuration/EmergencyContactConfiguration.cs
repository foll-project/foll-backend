using foll_backend.Care.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Configuration;

public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.ToTable("emergency_contacts", "care");

        builder.HasKey(c => c.EmergencyContactId);
        builder.Property(c => c.EmergencyContactId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(c => c.PatientId).IsRequired();
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Relationship).IsRequired().HasMaxLength(100);

        builder.HasIndex(c => c.PatientId);
    }
}
