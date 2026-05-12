using foll_backend.IAM.Domain.Repositories;

namespace foll_backend.IAM.Application.ACL;

public class UserInfoAcl : IUserInfoAcl
{
    private readonly IUserRepository _userRepository;

    public UserInfoAcl(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserInfoDto?> GetUserInfoByIdAsync(long userId)
    {
        if (userId <= 0) return null;

        var user = await _userRepository.FindByIdAsync(userId);
        if (user is null) return null;

        return new UserInfoDto(user.UserId, user.Email, user.FirstName, user.LastName, user.PhoneNumber);
    }
}