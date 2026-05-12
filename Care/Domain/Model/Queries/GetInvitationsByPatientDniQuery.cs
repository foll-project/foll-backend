namespace foll_backend.Care.Domain.Model.Queries;

public record GetInvitationsByPatientDniQuery(long ActorUserId, string PatientDni);
