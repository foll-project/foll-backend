namespace foll_backend.Care.Application.ACL;

public record PatientDeviceAccessDto(long PatientId, long OfficialGuardianUserId);
