using MediatR;

namespace foll_backend.Shared.Domain.Events;

public record PatientsDeletedEvent(IEnumerable<long> PatientIds, long ActorUserId) : INotification;
