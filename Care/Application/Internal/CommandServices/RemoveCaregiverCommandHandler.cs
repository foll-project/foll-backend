using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace foll_backend.Care.Application.Internal.CommandServices;

public class RemoveCaregiverCommandHandler : IRequestHandler<RemoveCaregiverCommand>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCaregiverCommandHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveCaregiverCommand request, CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == request.CaregiverId)
            throw new InvalidOperationException("Un usuario no puede auto-eliminarse de la red de cuidado.");

        var patient = await _patientRepository.FindByIdAsync(request.PatientId);
        if (patient is null) 
            throw new InvalidOperationException("Paciente no encontrado.");

        if (patient.OfficialGuardianUserId != request.CurrentUserId)
            throw new UnauthorizedAccessException("Solo el cuidador principal (OfficialGuardian) puede eliminar a un cuidador invitado.");

        patient.RemoveCaregiver(request.CaregiverId);

        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();
    }
}
