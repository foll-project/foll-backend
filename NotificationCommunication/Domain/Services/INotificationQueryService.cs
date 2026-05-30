using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Queries;

namespace foll_backend.NotificationCommunication.Domain.Services;

public interface INotificationQueryService
{
    Task<IReadOnlyCollection<NotificationLog>> Handle(ListNotificationsForUserQuery query);
    Task<NotificationLog?> Handle(GetNotificationByIdQuery query);
    Task<NotificationLog?> Handle(GetNotificationDeliveryStatusQuery query);
}
