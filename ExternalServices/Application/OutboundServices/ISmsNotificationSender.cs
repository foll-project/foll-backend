using foll_backend.ExternalServices.Domain.Model;

namespace foll_backend.ExternalServices.Application.OutboundServices;

public interface ISmsNotificationSender
{
    Task<SmsNotificationResult> SendAsync(SmsNotificationRequest request, CancellationToken cancellationToken = default);
}
