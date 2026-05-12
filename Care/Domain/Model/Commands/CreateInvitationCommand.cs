namespace foll_backend.Care.Domain.Model.Commands;

public record CreateInvitationCommand(
    long ActorUserId,
    string PatientDni,
    short RelationshipTypeId,
    DateTime ExpiresAt);
