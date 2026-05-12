using foll_backend.DeviceManagment.Domain.Model.Enums;
using MediatR;

namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public record DeviceDisconnectedIntegrationEvent(
    long DeviceId,
    long PatientId,
    DateTime DetectedAtUtc,
    DateTime LastSeenAtUtc,
    ConnectivityChangeCause Cause) : INotification;
