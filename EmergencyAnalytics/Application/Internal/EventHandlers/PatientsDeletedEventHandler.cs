using foll_backend.EmergencyAnalytics.Domain.Model.Commands;
using foll_backend.EmergencyAnalytics.Domain.Services;
using foll_backend.Shared.Domain.Events;
using MediatR;

namespace foll_backend.EmergencyAnalytics.Application.Internal.EventHandlers;

public class PatientsDeletedEventHandler : INotificationHandler<PatientsDeletedEvent>
{
    private readonly IEmergencyIncidentCommandService _commandService;

    public PatientsDeletedEventHandler(IEmergencyIncidentCommandService commandService)
    {
        _commandService = commandService;
    }

    public async Task Handle(PatientsDeletedEvent notification, CancellationToken cancellationToken)
    {
        var command = new DeleteEmergenciesByAccountCommand(notification.PatientIds);
        await _commandService.Handle(command);
    }
}
