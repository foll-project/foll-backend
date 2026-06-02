using foll_backend.EmergencyAnalytics.Domain.Events;
using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using foll_backend.Shared.Domain.Events;

namespace foll_backend.EmergencyAnalytics.Domain.Model.Entities;

public class EmergencyIncident : EntityWithDomainEvents
{
    public long EmergencyIncidentId { get; private set; }
    public Guid IncidentKey { get; private set; }
    public long DeviceId { get; private set; }
    public long PatientId { get; private set; }
    public short FallTypeId { get; private set; }
    public FallType? FallType { get; private set; }
    public EmergencyIncidentStatus Status { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime LastSignalAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public long? ClosedByUserId { get; private set; }
    public decimal? AiConfidenceScore { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public EmergencyCancellationReason? CancellationReason { get; private set; }
    public string? FinalObservation { get; private set; }
    public string? LastSourcePayload { get; private set; }

    protected EmergencyIncident()
    {
        IncidentKey = Guid.NewGuid();
    }

    private EmergencyIncident(
        long deviceId,
        long patientId,
        short fallTypeId,
        DateTime openedAtUtc,
        decimal? aiConfidenceScore,
        decimal? latitude,
        decimal? longitude,
        string? sourcePayload)
    {
        if (deviceId <= 0) throw new ArgumentOutOfRangeException(nameof(deviceId));
        if (patientId <= 0) throw new ArgumentOutOfRangeException(nameof(patientId));
        if (fallTypeId <= 0) throw new ArgumentOutOfRangeException(nameof(fallTypeId));

        IncidentKey = Guid.NewGuid();
        DeviceId = deviceId;
        PatientId = patientId;
        FallTypeId = fallTypeId;
        Status = EmergencyIncidentStatus.Open;
        OpenedAt = NormalizeTimestamp(openedAtUtc);
        LastSignalAt = OpenedAt;
        AiConfidenceScore = aiConfidenceScore;
        Latitude = latitude;
        Longitude = longitude;
        LastSourcePayload = NormalizePayload(sourcePayload);
    }

    public static EmergencyIncident CreateFallDetected(
        long deviceId,
        long patientId,
        short fallTypeId,
        DateTime openedAtUtc,
        decimal? aiConfidenceScore,
        decimal? latitude,
        decimal? longitude,
        string? sourcePayload)
    {
        var incident = new EmergencyIncident(
            deviceId,
            patientId,
            fallTypeId,
            openedAtUtc,
            aiConfidenceScore,
            latitude,
            longitude,
            sourcePayload);

        incident.RaiseDomainEvent(new EmergencyIncidentOpenedDomainEvent(
            incident.IncidentKey,
            incident.DeviceId,
            incident.PatientId,
            incident.FallTypeId,
            incident.OpenedAt,
            incident.AiConfidenceScore,
            incident.Latitude,
            incident.Longitude));

        return incident;
    }

    public void RefreshDetection(short fallTypeId, DateTime reportedAtUtc, decimal? aiConfidenceScore, decimal? latitude, decimal? longitude, string? sourcePayload)
    {
        EnsureOpenIncident();
        if (fallTypeId <= 0) throw new ArgumentOutOfRangeException(nameof(fallTypeId));

        var normalizedTimestamp = NormalizeTimestamp(reportedAtUtc);
        if (normalizedTimestamp <= LastSignalAt) return;

        FallTypeId = fallTypeId;
        LastSignalAt = normalizedTimestamp;
        AiConfidenceScore = aiConfidenceScore;
        Latitude = latitude;
        Longitude = longitude;
        LastSourcePayload = NormalizePayload(sourcePayload);
    }

    public void Cancel(EmergencyCancellationReason reason, DateTime cancelledAtUtc, string? sourcePayload)
    {
        EnsureOpenIncident();

        var normalizedTimestamp = NormalizeTimestamp(cancelledAtUtc);
        Status = EmergencyIncidentStatus.Cancelled;
        CancelledAt = normalizedTimestamp;
        ClosedAt = normalizedTimestamp;
        LastSignalAt = normalizedTimestamp;
        CancellationReason = reason;
        LastSourcePayload = NormalizePayload(sourcePayload);

        RaiseDomainEvent(new EmergencyIncidentCancelledDomainEvent(
            IncidentKey,
            DeviceId,
            PatientId,
            FallTypeId,
            normalizedTimestamp,
            reason,
            null,
            null));
    }

    public void MarkAsFalsePositive(long actorUserId, string? observation, DateTime markedAtUtc)
    {
        EnsureOpenIncident();
        if (actorUserId <= 0) throw new ArgumentOutOfRangeException(nameof(actorUserId));

        var normalizedTimestamp = NormalizeTimestamp(markedAtUtc);
        Status = EmergencyIncidentStatus.Cancelled;
        CancelledAt = normalizedTimestamp;
        ClosedAt = normalizedTimestamp;
        ClosedByUserId = actorUserId;
        LastSignalAt = normalizedTimestamp;
        CancellationReason = EmergencyCancellationReason.FalsePositive;
        FinalObservation = NormalizeObservation(observation);

        RaiseDomainEvent(new EmergencyIncidentCancelledDomainEvent(
            IncidentKey,
            DeviceId,
            PatientId,
            FallTypeId,
            normalizedTimestamp,
            EmergencyCancellationReason.FalsePositive,
            actorUserId,
            FinalObservation));
    }

    public void Resolve(long actorUserId, string? observation, DateTime resolvedAtUtc)
    {
        EnsureOpenIncident();
        if (actorUserId <= 0) throw new ArgumentOutOfRangeException(nameof(actorUserId));

        var normalizedTimestamp = NormalizeTimestamp(resolvedAtUtc);
        Status = EmergencyIncidentStatus.Resolved;
        ResolvedAt = normalizedTimestamp;
        ClosedAt = normalizedTimestamp;
        ClosedByUserId = actorUserId;
        LastSignalAt = normalizedTimestamp;
        FinalObservation = NormalizeObservation(observation);

        RaiseDomainEvent(new EmergencyIncidentResolvedDomainEvent(
            IncidentKey,
            DeviceId,
            PatientId,
            FallTypeId,
            normalizedTimestamp,
            actorUserId,
            FinalObservation));
    }

    public void UpdateFinalObservation(long actorUserId, string observation)
    {
        EnsureClosedIncident();
        if (actorUserId <= 0) throw new ArgumentOutOfRangeException(nameof(actorUserId));
        if (string.IsNullOrWhiteSpace(observation))
            throw new ArgumentException("La observación es obligatoria.", nameof(observation));

        FinalObservation = NormalizeObservation(observation);
    }

    private void EnsureOpenIncident()
    {
        if (Status != EmergencyIncidentStatus.Open)
            throw new InvalidOperationException("Solo un incidente abierto puede modificarse.");
    }

    private void EnsureClosedIncident()
    {
        if (Status == EmergencyIncidentStatus.Open)
            throw new InvalidOperationException("Solo se pueden agregar observaciones finales a incidentes cerrados.");
    }

    private static string? NormalizePayload(string? payload)
    {
        return string.IsNullOrWhiteSpace(payload) ? null : payload.Trim();
    }

    private static string? NormalizeObservation(string? observation)
    {
        return string.IsNullOrWhiteSpace(observation) ? null : observation.Trim();
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
