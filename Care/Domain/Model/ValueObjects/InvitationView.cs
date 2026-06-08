namespace foll_backend.Care.Domain.Model.ValueObjects;

// Read-model enriquecido de una invitación para los listados (recibidas/enviadas).
public record InvitationView(
    long InvitationId,
    long PatientId,
    string PatientFirstName,
    string PatientLastName,
    string PatientDni,
    long InvitingUserId,
    short RelationshipTypeId,
    string RelationshipName,
    string Status,
    DateTime ExpiresAt);
