using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Persistence.EFC.Configuration;

public class FallTypeConfiguration : IEntityTypeConfiguration<FallType>
{
    public void Configure(EntityTypeBuilder<FallType> builder)
    {
        builder.ToTable("fall_types", "emergency");

        builder.HasKey(f => f.FallTypeId);
        builder.Property(f => f.FallTypeId)
            .HasColumnType("smallint")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Description)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.SeverityLevel)
            .IsRequired()
            .HasColumnType("smallint");

        builder.HasIndex(f => f.Name).IsUnique();

        builder.HasData(
            new FallType(1, "FRONTAL", "Caída hacia adelante detectada por patrón vectorial frontal del dataset SISFALL.", 1),
            new FallType(2, "LATERAL", "Caída lateral detectada por desplazamiento dominante en eje lateral del dataset SISFALL.", 2),
            new FallType(3, "UNKNOWN", "Tipo de caída no clasificado o no enviado por el dispositivo/IA.", 2),
            new FallType(4, "BACKWARD", "Caída hacia atrás detectada por patrón vectorial posterior del dataset SISFALL.", 1));
    }
}
