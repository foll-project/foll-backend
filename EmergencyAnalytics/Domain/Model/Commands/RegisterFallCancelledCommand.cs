using foll_backend.EmergencyAnalytics.Domain.Model.Enums;

namespace foll_backend.EmergencyAnalytics.Domain.Model.Commands;

public record RegisterFallCancelledCommand(
    long DeviceId,
    DateTime ReportedAtUtc,
    EmergencyCancellationReason Reason,
    string? RawPayload);
