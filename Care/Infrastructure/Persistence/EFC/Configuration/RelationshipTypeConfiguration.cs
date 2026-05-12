using foll_backend.Care.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Configuration;

public class RelationshipTypeConfiguration : IEntityTypeConfiguration<RelationshipType>
{
    public void Configure(EntityTypeBuilder<RelationshipType> builder)
    {
        builder.ToTable("relationship_types", "care");

        builder.HasKey(r => r.RelationshipTypeId);
        builder.Property(r => r.RelationshipTypeId).IsRequired().ValueGeneratedNever();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(80);

        builder.HasData(
            new RelationshipType(1, "Hijo"),
            new RelationshipType(2, "Vecino"),
            new RelationshipType(3, "Enfermera"),
            new RelationshipType(4, "Familiar")
        );
    }
}
