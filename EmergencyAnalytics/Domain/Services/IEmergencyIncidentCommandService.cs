using foll_backend.EmergencyAnalytics.Domain.Model.Commands;

namespace foll_backend.EmergencyAnalytics.Domain.Services;

public interface IEmergencyIncidentCommandService
{
    Task Handle(RegisterFallDetectedCommand command);
    Task Handle(RegisterFallCancelledCommand command);
    Task Handle(MarkFallIncidentFalsePositiveCommand command);
    Task Handle(ResolveFallIncidentCommand command);
    Task Handle(UpdateEmergencyIncidentObservationCommand command);
}
