namespace foll_backend.Care.Domain.Model.Entities;

public class RelationshipType
{
    public short RelationshipTypeId { get; private set; }
    public string Name { get; private set; }

    protected RelationshipType()
    {
        Name = string.Empty;
    }

    public RelationshipType(short relationshipTypeId, string name)
    {
        if (relationshipTypeId <= 0) throw new ArgumentOutOfRangeException(nameof(relationshipTypeId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre del tipo de relación es obligatorio.", nameof(name));

        RelationshipTypeId = relationshipTypeId;
        Name = name.Trim();
    }
}
