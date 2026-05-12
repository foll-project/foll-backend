using foll_backend.Care.Domain.Repositories;

namespace foll_backend.Care.Application.ACL;

public class PatientEmergencyAcl : IPatientEmergencyAcl
{
    private readonly IPatientRepository _patientRepository;

    public PatientEmergencyAcl(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientEmergencyAccessDto?> GetPatientEmergencyAccessByIdAsync(long patientId)
    {
        if (patientId <= 0) return null;

        var patient = await _patientRepository.FindByIdAsync(patientId);
        if (patient is null) return null;

        return new PatientEmergencyAccessDto(
            patient.PatientId,
            patient.OfficialGuardianUserId,
            patient.CurrentGuardianUserId,
            patient.Caregivers.Select(c => c.UserId).ToArray());
    }
}
