using foll_backend.NotificationCommunication.Domain.Model.Commands;
using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Repositories;
using foll_backend.NotificationCommunication.Domain.Services;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.NotificationCommunication.Application.Internal.CommandServices;

public class UserPushTokenCommandService : IUserPushTokenCommandService
{
    private readonly IUserPushTokenRepository _userPushTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserPushTokenCommandService(IUserPushTokenRepository userPushTokenRepository, IUnitOfWork unitOfWork)
    {
        _userPushTokenRepository = userPushTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(RegisterUserPushTokenCommand command)
    {
        var existingToken = await _userPushTokenRepository.FindByUserIdAndTokenAsync(command.UserId, command.Token);
        if (existingToken is not null)
        {
            existingToken.Refresh(command.Platform, command.DeviceName, DateTime.UtcNow);
            _userPushTokenRepository.Update(existingToken);
            await _unitOfWork.CompleteAsync();
            return existingToken.UserPushTokenId;
        }

        var token = new UserPushToken(command.UserId, command.Token, command.Platform, command.DeviceName, DateTime.UtcNow);
        await _userPushTokenRepository.AddAsync(token);
        await _unitOfWork.CompleteAsync();
        return token.UserPushTokenId;
    }

    public async Task Handle(DeactivateUserPushTokenCommand command)
    {
        var token = await _userPushTokenRepository.FindByIdAndUserIdAsync(command.UserPushTokenId, command.UserId);
        if (token is null) throw new InvalidOperationException("Token push no encontrado.");

        token.Deactivate(DateTime.UtcNow);
        _userPushTokenRepository.Update(token);
        await _unitOfWork.CompleteAsync();
    }
}
