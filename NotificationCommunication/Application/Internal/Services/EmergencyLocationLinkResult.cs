using foll_backend.NotificationCommunication.Domain.Model.Entities;

namespace foll_backend.NotificationCommunication.Application.Internal.Services;

public record EmergencyLocationLinkResult(string Url, DateTime ExpiresAt);

public record EmergencyLocationLinkResolution(EmergencyLocationAccessLink Link);
