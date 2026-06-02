using MediatR;

namespace foll_backend.EmergencyAnalytics.Application.Internal.Notifications;

public record EmergencyIncidentOpenedIntegrationEvent(
    Guid IncidentKey,
    long DeviceId,
    long PatientId,
    short FallTypeId,
    string FallTypeName,
    string? FallTypeDescription,
    short SeverityLevel,
    DateTime OpenedAtUtc,
    decimal? AiConfidenceScore,
    decimal? Latitude,
    decimal? Longitude) : INotification;
