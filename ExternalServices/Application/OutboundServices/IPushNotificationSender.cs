using foll_backend.ExternalServices.Domain.Model;

namespace foll_backend.ExternalServices.Application.OutboundServices;

public interface IPushNotificationSender
{
    Task<PushNotificationResult> SendAsync(PushNotificationRequest request, CancellationToken cancellationToken = default);
}
