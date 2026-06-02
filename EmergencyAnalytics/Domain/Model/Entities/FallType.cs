namespace foll_backend.EmergencyAnalytics.Domain.Model.Entities;

public class FallType
{
    public const short UnknownId = 3;

    public short FallTypeId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public short SeverityLevel { get; private set; }

    protected FallType()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public FallType(short fallTypeId, string name, string description, short severityLevel)
    {
        if (fallTypeId <= 0) throw new ArgumentOutOfRangeException(nameof(fallTypeId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre del tipo de caída es obligatorio.", nameof(name));
        if (severityLevel <= 0) throw new ArgumentOutOfRangeException(nameof(severityLevel));

        FallTypeId = fallTypeId;
        Name = name.Trim().ToUpperInvariant();
        Description = string.IsNullOrWhiteSpace(description) ? "Sin descripción técnica." : description.Trim();
        SeverityLevel = severityLevel;
    }
}
