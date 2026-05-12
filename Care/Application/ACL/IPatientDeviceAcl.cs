namespace foll_backend.Care.Application.ACL;

public interface IPatientDeviceAcl
{
    Task<PatientDeviceAccessDto?> GetPatientDeviceAccessByIdAsync(long patientId);
}
