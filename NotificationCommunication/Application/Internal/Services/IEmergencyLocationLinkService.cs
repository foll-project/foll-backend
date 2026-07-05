namespace foll_backend.NotificationCommunication.Application.Internal.Services;

public interface IEmergencyLocationLinkService
{
    Task<EmergencyLocationLinkResult> CreateLinkAsync(
        Guid incidentKey,
        long patientId,
        long? deviceId,
        DateTime createdAt,
        CancellationToken cancellationToken = default);

    Task<EmergencyLocationLinkResolution?> ResolveAsync(
        string token,
        DateTime now,
        CancellationToken cancellationToken = default);
}
