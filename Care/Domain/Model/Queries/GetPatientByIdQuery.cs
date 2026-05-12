namespace foll_backend.Care.Domain.Model.Queries;

public record GetPatientByIdQuery(long ActorUserId, long PatientId);
