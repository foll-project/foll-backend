using foll_backend.NotificationCommunication.Application.OutboundServices;
using CarePatientNotificationAcl = foll_backend.Care.Application.ACL.IPatientNotificationAcl;

namespace foll_backend.NotificationCommunication.Application.ACL;

public class PatientNotificationAccessService : IPatientNotificationAccessService
{
    private readonly CarePatientNotificationAcl _patientNotificationAcl;

    public PatientNotificationAccessService(CarePatientNotificationAcl patientNotificationAcl)
    {
        _patientNotificationAcl = patientNotificationAcl;
    }

    public async Task<PatientNotificationRecipientsDto?> GetRecipientsForPatientAsync(long patientId)
    {
        if (patientId <= 0) return null;

        var access = await _patientNotificationAcl.GetPatientNotificationAccessByIdAsync(patientId);
        if (access is null) return null;

        var pushRecipients = access.UserRecipients
            .Where(recipient => recipient.UserId > 0)
            .GroupBy(recipient => recipient.UserId)
            .Select(group => group.First())
            .Select(recipient => new PatientPushRecipientDto(
                access.PatientId,
                recipient.UserId,
                recipient.FullName,
                recipient.PhoneNumber))
            .ToArray();

        var smsRecipients = access.EmergencyContacts
            .Where(contact => !string.IsNullOrWhiteSpace(contact.PhoneNumber))
            .GroupBy(contact => contact.PhoneNumber.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(contact => new PatientSmsRecipientDto(
                access.PatientId,
                null,
                contact.EmergencyContactId,
                contact.FullName,
                contact.PhoneNumber,
                "EmergencyContact"))
            .ToArray();

        return new PatientNotificationRecipientsDto(access.PatientId, pushRecipients, smsRecipients);
    }
}
