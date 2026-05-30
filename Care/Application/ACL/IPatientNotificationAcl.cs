namespace foll_backend.Care.Application.ACL;

public interface IPatientNotificationAcl
{
    Task<PatientNotificationAccessDto?> GetPatientNotificationAccessByIdAsync(long patientId);
}
