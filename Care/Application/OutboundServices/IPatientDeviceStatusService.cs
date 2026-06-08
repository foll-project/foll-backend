namespace foll_backend.Care.Application.OutboundServices;

public interface IPatientDeviceStatusService
{
    Task<PatientDeviceStatusInfo?> GetByPatientIdAsync(long patientId);
}
