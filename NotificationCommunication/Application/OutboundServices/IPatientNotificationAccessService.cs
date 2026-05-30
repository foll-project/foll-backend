namespace foll_backend.NotificationCommunication.Application.OutboundServices;

public interface IPatientNotificationAccessService
{
    Task<PatientNotificationRecipientDto?> GetRecipientForPatientAsync(long patientId);
}
