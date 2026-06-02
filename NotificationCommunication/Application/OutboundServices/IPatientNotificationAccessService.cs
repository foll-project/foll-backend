namespace foll_backend.NotificationCommunication.Application.OutboundServices;

public interface IPatientNotificationAccessService
{
    Task<IReadOnlyCollection<PatientNotificationRecipientDto>> GetRecipientsForPatientAsync(long patientId);
}
