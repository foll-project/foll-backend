using foll_backend.DeviceManagment.Domain.Model.Commands;
using foll_backend.DeviceManagment.Domain.Services;
using foll_backend.Shared.Domain.Events;
using MediatR;

namespace foll_backend.DeviceManagment.Application.Internal.EventHandlers;

public class PatientsDeletedEventHandler : INotificationHandler<PatientsDeletedEvent>
{
    private readonly IDeviceCommandService _deviceCommandService;

    public PatientsDeletedEventHandler(IDeviceCommandService deviceCommandService)
    {
        _deviceCommandService = deviceCommandService;
    }

    public async Task Handle(PatientsDeletedEvent notification, CancellationToken cancellationToken)
    {
        var command = new UnlinkDevicesByAccountCommand(notification.PatientIds, notification.ActorUserId);
        await _deviceCommandService.Handle(command);
    }
}
