using foll_backend.Care.Domain.Model.ValueObjects;

namespace foll_backend.Care.Application.OutboundServices;

public interface IUserInfoService
{
    Task<UserInfo?> FindByIdAsync(long userId);
}
