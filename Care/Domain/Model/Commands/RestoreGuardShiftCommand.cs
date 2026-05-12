namespace foll_backend.Care.Domain.Model.Commands;

public record RestoreGuardShiftCommand(long ActorUserId, long PatientId);
