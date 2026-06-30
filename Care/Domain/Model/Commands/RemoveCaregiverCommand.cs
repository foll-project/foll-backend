using MediatR;

namespace foll_backend.Care.Domain.Model.Commands;

public record RemoveCaregiverCommand(long CurrentUserId, long PatientId, long CaregiverId) : IRequest;
