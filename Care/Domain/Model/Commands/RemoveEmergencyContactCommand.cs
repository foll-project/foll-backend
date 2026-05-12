namespace foll_backend.Care.Domain.Model.Commands;

public record RemoveEmergencyContactCommand(long ActorUserId, long PatientId, long EmergencyContactId);
