using foll_backend.IAM.Application.OutboundServices;
using foll_backend.IAM.Domain.Model.Queries;
using foll_backend.IAM.Domain.Repositories;
using foll_backend.IAM.Domain.Services;

namespace foll_backend.IAM.Application.Internal.QueryServices;

public class UserQueryService : IUserQueryService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashingService _hashingService;
    private readonly ITokenService _tokenService;

    public UserQueryService(
        IUserRepository userRepository,
        IHashingService hashingService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _hashingService = hashingService;
        _tokenService = tokenService;
    }

    public async Task<(foll_backend.IAM.Domain.Model.Entities.User user, string token)?> AuthenticateAsync(LoginQuery query)
    {
        var user = await _userRepository.FindByEmailAsync(query.Email);
        if (user == null) return null;

        if (!_hashingService.VerifyPassword(query.Password, user.PasswordHash)) return null;

        var token = _tokenService.GenerateToken(user);

        return (user, token);
    }
}
