using foll_backend.Care.Domain.Model.Commands;

namespace foll_backend.Care.Domain.Services;

public interface IPatientCommandService
{
    Task<long> Handle(CreatePatientCommand command);
    Task Handle(UpdatePatientCommand command);

    Task Handle(AssignGuardShiftCommand command);
    Task Handle(RestoreGuardShiftCommand command);

    Task<long> Handle(AddEmergencyContactCommand command);
    Task Handle(RemoveEmergencyContactCommand command);

    Task<long> Handle(CreateInvitationCommand command);
    Task Handle(AcceptInvitationCommand command);
    Task Handle(RejectInvitationCommand command);
}
