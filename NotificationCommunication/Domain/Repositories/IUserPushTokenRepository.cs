using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.NotificationCommunication.Domain.Repositories;

public interface IUserPushTokenRepository : IBaseRepository<UserPushToken>
{
    Task<IReadOnlyCollection<UserPushToken>> ListByUserIdAsync(long userId);
    Task<IReadOnlyCollection<UserPushToken>> ListActiveByUserIdAsync(long userId);
    Task<UserPushToken?> FindByIdAndUserIdAsync(long userPushTokenId, long userId);
    Task<UserPushToken?> FindByUserIdAndTokenAsync(long userId, string token);
}
