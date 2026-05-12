namespace foll_backend.EmergencyAnalytics.Domain.Model.Commands;

public record RegisterFallDetectedCommand(
    long DeviceId,
    short? FallTypeId,
    string? FallTypeName,
    DateTime ReportedAtUtc,
    decimal? AiConfidenceScore,
    decimal? Latitude,
    decimal? Longitude,
    string? RawPayload);
