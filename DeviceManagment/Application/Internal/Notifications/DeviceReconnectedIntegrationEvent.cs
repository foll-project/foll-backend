using foll_backend.DeviceManagment.Domain.Model.Enums;
using MediatR;

namespace foll_backend.DeviceManagment.Application.Internal.Notifications;

public record DeviceReconnectedIntegrationEvent(
    long DeviceId,
    long PatientId,
    DateTime ReportedAtUtc,
    ConnectivityChangeCause Cause) : INotification;
