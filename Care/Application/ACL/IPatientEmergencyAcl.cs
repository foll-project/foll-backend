namespace foll_backend.Care.Application.ACL;

public interface IPatientEmergencyAcl
{
    Task<PatientEmergencyAccessDto?> GetPatientEmergencyAccessByIdAsync(long patientId);
}
