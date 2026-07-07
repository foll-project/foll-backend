using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.NotificationCommunication.Domain.Repositories;

public interface ISmsNotificationLogRepository : IBaseRepository<SmsNotificationLog>
{
    Task<SmsNotificationLog?> FindByIncidentKeyAndPhoneNumberAsync(Guid incidentKey, string phoneNumber);
}
