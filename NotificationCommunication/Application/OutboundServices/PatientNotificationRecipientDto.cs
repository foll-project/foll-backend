namespace foll_backend.NotificationCommunication.Application.OutboundServices;

public record PatientNotificationRecipientsDto(
    long PatientId,
    IReadOnlyCollection<PatientPushRecipientDto> PushRecipients,
    IReadOnlyCollection<PatientSmsRecipientDto> SmsRecipients);

public record PatientPushRecipientDto(
    long PatientId,
    long UserId,
    string FullName,
    string? PhoneNumber);

public record PatientSmsRecipientDto(
    long PatientId,
    long? UserId,
    long? EmergencyContactId,
    string FullName,
    string PhoneNumber,
    string RecipientKind);
