using MediatR;

namespace foll_backend.Care.Domain.Model.Commands;

public record LinkCaregiverViaQrCommand(long PatientId, long CaregiverId) : IRequest;
