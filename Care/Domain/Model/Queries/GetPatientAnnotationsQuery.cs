namespace foll_backend.Care.Domain.Model.Queries;

public record GetPatientAnnotationsQuery(long ActorUserId, long PatientId);