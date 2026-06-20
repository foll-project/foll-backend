using foll_backend.Care.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Configuration;

public class PatientAnnotationConfiguration : IEntityTypeConfiguration<PatientAnnotation>
{
    public void Configure(EntityTypeBuilder<PatientAnnotation> builder)
    {
        builder.ToTable("patient_annotations", "care");
        builder.HasKey(a => a.PatientAnnotationId);
        builder.Property(a => a.PatientAnnotationId).IsRequired().ValueGeneratedOnAdd();

        builder.Property(a => a.PatientId).IsRequired();
        builder.Property(a => a.AuthorUserId).IsRequired();
        builder.Property(a => a.Content).IsRequired().HasColumnType("text");
        builder.Property(a => a.CreatedAt).IsRequired().HasColumnType("timestamptz");

        builder.HasIndex(a => a.PatientId);
    }
}