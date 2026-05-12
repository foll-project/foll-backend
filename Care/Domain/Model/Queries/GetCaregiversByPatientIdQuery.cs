namespace foll_backend.Care.Domain.Model.Queries;

public record GetCaregiversByPatientIdQuery(long ActorUserId, long PatientId);