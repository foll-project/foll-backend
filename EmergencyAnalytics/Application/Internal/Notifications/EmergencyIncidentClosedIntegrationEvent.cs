using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using MediatR;

namespace foll_backend.EmergencyAnalytics.Application.Internal.Notifications;

public record EmergencyIncidentClosedIntegrationEvent(
    Guid IncidentKey,
    long DeviceId,
    long PatientId,
    short FallTypeId,
    string FallTypeName,
    string? FallTypeDescription,
    short SeverityLevel,
    EmergencyIncidentStatus Status,
    DateTime ClosedAtUtc,
    EmergencyCancellationReason? CancellationReason,
    long? ClosedByUserId,
    string? Observation) : INotification;
