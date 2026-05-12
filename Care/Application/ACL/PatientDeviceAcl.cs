using foll_backend.Care.Domain.Repositories;

namespace foll_backend.Care.Application.ACL;

public class PatientDeviceAcl : IPatientDeviceAcl
{
    private readonly IPatientRepository _patientRepository;

    public PatientDeviceAcl(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientDeviceAccessDto?> GetPatientDeviceAccessByIdAsync(long patientId)
    {
        if (patientId <= 0) return null;

        var patient = await _patientRepository.FindByIdAsync(patientId);
        if (patient is null) return null;

        return new PatientDeviceAccessDto(patient.PatientId, patient.OfficialGuardianUserId);
    }
}
