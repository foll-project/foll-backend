namespace foll_backend.Care.Domain.Model.Commands;

public record AssignGuardShiftCommand(long ActorUserId, long PatientId, long NewCurrentGuardianUserId);
