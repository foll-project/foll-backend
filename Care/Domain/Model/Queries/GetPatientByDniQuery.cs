namespace foll_backend.Care.Domain.Model.Queries;

public record GetPatientByDniQuery(long ActorUserId, string Dni);
