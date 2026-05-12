namespace foll_backend.IAM.Application.ACL;

public interface IUserInfoAcl
{
    Task<UserInfoDto?> GetUserInfoByIdAsync(long userId);
}