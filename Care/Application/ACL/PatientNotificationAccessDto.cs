namespace foll_backend.Care.Application.ACL;

public record PatientNotificationAccessDto(
    long PatientId,
    IReadOnlyCollection<PatientNotificationUserRecipientDto> UserRecipients,
    IReadOnlyCollection<PatientNotificationEmergencyContactDto> EmergencyContacts);

public record PatientNotificationUserRecipientDto(
    long UserId,
    string FullName,
    string? PhoneNumber);

public record PatientNotificationEmergencyContactDto(
    long EmergencyContactId,
    string FullName,
    string PhoneNumber,
    string Relationship);
