using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.NotificationCommunication.Domain.Repositories;

public interface INotificationLogRepository : IBaseRepository<NotificationLog>
{
    Task<IReadOnlyCollection<NotificationLog>> ListByUserIdAsync(long userId);
    Task<NotificationLog?> FindByIdAndUserIdAsync(long notificationLogId, long userId);
}
