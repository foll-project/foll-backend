namespace foll_backend.NotificationCommunication.Domain.Model.Entities;

public class EmergencyLocationAccessLink
{
    public long EmergencyLocationAccessLinkId { get; private set; }
    public Guid IncidentKey { get; private set; }
    public long PatientId { get; private set; }
    public long? DeviceId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    protected EmergencyLocationAccessLink()
    {
        TokenHash = string.Empty;
    }

    public EmergencyLocationAccessLink(
        Guid incidentKey,
        long patientId,
        long? deviceId,
        string tokenHash,
        DateTime expiresAt,
        DateTime createdAt)
    {
        if (incidentKey == Guid.Empty) throw new ArgumentException("La clave del incidente es obligatoria.", nameof(incidentKey));
        if (patientId <= 0) throw new ArgumentOutOfRangeException(nameof(patientId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("El hash del token es obligatorio.", nameof(tokenHash));

        var normalizedCreatedAt = NormalizeTimestamp(createdAt);
        var normalizedExpiresAt = NormalizeTimestamp(expiresAt);
        if (normalizedExpiresAt <= normalizedCreatedAt)
            throw new ArgumentException("La expiracion del enlace debe ser posterior a su creacion.", nameof(expiresAt));

        IncidentKey = incidentKey;
        PatientId = patientId;
        DeviceId = deviceId;
        TokenHash = tokenHash.Trim();
        ExpiresAt = normalizedExpiresAt;
        CreatedAt = normalizedCreatedAt;
    }

    public bool IsActive(DateTime now)
    {
        var normalizedNow = NormalizeTimestamp(now);
        return RevokedAt is null && ExpiresAt > normalizedNow;
    }

    public void MarkAccessed(DateTime accessedAt)
    {
        LastAccessedAt = NormalizeTimestamp(accessedAt);
    }

    public void Revoke(DateTime revokedAt)
    {
        RevokedAt = NormalizeTimestamp(revokedAt);
    }

    private static DateTime NormalizeTimestamp(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
        };
    }
}
