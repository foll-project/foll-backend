using foll_backend.NotificationCommunication.Domain.Model.Entities;

namespace foll_backend.NotificationCommunication.Application.OutboundServices;

public interface INotificationRealtimePublisher
{
    Task PublishCreatedAsync(NotificationLog notification, CancellationToken cancellationToken = default);
}
