using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Model.Queries;

namespace foll_backend.NotificationCommunication.Domain.Services;

public interface IUserPushTokenQueryService
{
    Task<IReadOnlyCollection<UserPushToken>> Handle(ListUserPushTokensQuery query);
}
