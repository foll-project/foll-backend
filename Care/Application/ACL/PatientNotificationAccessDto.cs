namespace foll_backend.Care.Application.ACL;

public record PatientNotificationAccessDto(
    long PatientId,
    long OfficialGuardianUserId,
    long? CurrentGuardianUserId,
    IReadOnlyCollection<long> CaregiverUserIds);
