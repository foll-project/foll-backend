using foll_backend.NotificationCommunication.Domain.Model.Commands;
using foll_backend.NotificationCommunication.Domain.Services;
using foll_backend.Shared.Domain.Events;
using MediatR;

namespace foll_backend.NotificationCommunication.Application.Internal.EventHandlers;

public class UserAccountDeletedEventHandler : INotificationHandler<UserAccountDeletedEvent>
{
    private readonly INotificationCommandService _notificationCommandService;

    public UserAccountDeletedEventHandler(INotificationCommandService notificationCommandService)
    {
        _notificationCommandService = notificationCommandService;
    }

    public async Task Handle(UserAccountDeletedEvent notification, CancellationToken cancellationToken)
    {
        var command = new DeleteNotificationsByAccountCommand(notification.UserId);
        await _notificationCommandService.Handle(command);
    }
}
