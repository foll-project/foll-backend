using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Queries;
using foll_backend.NotificationCommunication.Domain.Repositories;
using foll_backend.NotificationCommunication.Domain.Services;

namespace foll_backend.NotificationCommunication.Application.Internal.QueryServices;

public class NotificationQueryService : INotificationQueryService
{
    private readonly INotificationLogRepository _notificationLogRepository;

    public NotificationQueryService(INotificationLogRepository notificationLogRepository)
    {
        _notificationLogRepository = notificationLogRepository;
    }

    public async Task<IReadOnlyCollection<NotificationLog>> Handle(ListNotificationsForUserQuery query)
    {
        return await _notificationLogRepository.ListByUserIdAsync(query.UserId);
    }

    public async Task<NotificationLog?> Handle(GetNotificationByIdQuery query)
    {
        return await _notificationLogRepository.FindByIdAndUserIdAsync(query.NotificationLogId, query.UserId);
    }

    public async Task<NotificationLog?> Handle(GetNotificationDeliveryStatusQuery query)
    {
        return await _notificationLogRepository.FindByIdAndUserIdAsync(query.NotificationLogId, query.UserId);
    }
}
