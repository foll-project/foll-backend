using foll_backend.Care.Application.OutboundServices;
using CareUserInfo = foll_backend.Care.Domain.Model.ValueObjects.UserInfo;
using IamUserInfoAcl = foll_backend.IAM.Application.ACL.IUserInfoAcl;

namespace foll_backend.Care.Application.ACL;

public class UserInfoService : IUserInfoService
{
    private readonly IamUserInfoAcl _userInfoAcl;

    public UserInfoService(IamUserInfoAcl userInfoAcl)
    {
        _userInfoAcl = userInfoAcl;
    }

    public async Task<CareUserInfo?> FindByIdAsync(long userId)
    {
        var userInfo = await _userInfoAcl.GetUserInfoByIdAsync(userId);
        return userInfo is null
            ? null
            : new CareUserInfo(
                userInfo.UserId,
                userInfo.Email,
                userInfo.FirstName,
                userInfo.LastName,
                userInfo.PhoneNumber);
    }
}
