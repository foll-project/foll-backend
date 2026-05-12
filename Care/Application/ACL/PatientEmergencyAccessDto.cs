namespace foll_backend.Care.Application.ACL;

public record PatientEmergencyAccessDto(
    long PatientId,
    long OfficialGuardianUserId,
    long? CurrentGuardianUserId,
    IReadOnlyCollection<long> CaregiverUserIds);
