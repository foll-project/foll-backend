using foll_backend.NotificationCommunication.Domain.Model.Commands;

namespace foll_backend.NotificationCommunication.Domain.Services;

public interface INotificationCommandService
{
    Task<long> Handle(CreateNotificationFromEventCommand command);
    Task Handle(MarkNotificationReadCommand command);
    Task Handle(MarkNotificationAcknowledgedCommand command);
}
