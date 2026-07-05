namespace foll_backend.NotificationCommunication.Application.OutboundServices;

public interface IPatientNotificationAccessService
{
    Task<PatientNotificationRecipientsDto?> GetRecipientsForPatientAsync(long patientId);
}
