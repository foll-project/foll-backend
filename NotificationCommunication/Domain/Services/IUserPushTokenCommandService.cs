using foll_backend.NotificationCommunication.Domain.Model.Commands;

namespace foll_backend.NotificationCommunication.Domain.Services;

public interface IUserPushTokenCommandService
{
    Task<long> Handle(RegisterUserPushTokenCommand command);
    Task Handle(DeactivateUserPushTokenCommand command);
}
