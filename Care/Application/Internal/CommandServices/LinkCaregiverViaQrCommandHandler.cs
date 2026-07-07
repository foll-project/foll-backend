using foll_backend.Care.Application.OutboundServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;
using MediatR;

namespace foll_backend.Care.Application.Internal.CommandServices;

public class LinkCaregiverViaQrCommandHandler : IRequestHandler<LinkCaregiverViaQrCommand>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUserInfoService _userInfoService;
    private readonly IUnitOfWork _unitOfWork;

    public LinkCaregiverViaQrCommandHandler(
        IPatientRepository patientRepository,
        IUserInfoService userInfoService,
        IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _userInfoService = userInfoService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LinkCaregiverViaQrCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.FindByIdAsync(request.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        var caregiver = await _userInfoService.FindByIdAsync(request.CaregiverId);
        if (caregiver is null) throw new InvalidOperationException("Cuidador no encontrado.");

        if (patient.Caregivers.Any(c => c.UserId == request.CaregiverId))
            throw new InvalidOperationException("El cuidador ya está vinculado al paciente.");

        // Fast-Track QR link uses default relationship type (1 = Familiar/Cuidador).
        patient.AddCaregiver(request.CaregiverId, 1);

        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();
    }
}
