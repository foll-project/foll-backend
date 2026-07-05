using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Services;
using foll_backend.Shared.Domain.Events;
using MediatR;

namespace foll_backend.Care.Application.Internal.EventHandlers;

public class UserAccountDeletedEventHandler : INotificationHandler<UserAccountDeletedEvent>
{
    private readonly IPatientCommandService _patientCommandService;
    private readonly IMediator _mediator;

    public UserAccountDeletedEventHandler(IPatientCommandService patientCommandService, IMediator mediator)
    {
        _patientCommandService = patientCommandService;
        _mediator = mediator;
    }

    public async Task Handle(UserAccountDeletedEvent notification, CancellationToken cancellationToken)
    {
        var command = new DeletePatientsByAccountCommand(notification.UserId);
        var patientIds = await _patientCommandService.Handle(command);

        if (patientIds != null && patientIds.Any())
        {
            await _mediator.Publish(new PatientsDeletedEvent(patientIds, notification.UserId));
        }
    }
}
