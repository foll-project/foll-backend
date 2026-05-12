namespace foll_backend.DeviceManagment.Domain.Model.Queries;

public record GetDeviceByPatientIdQuery(long ActorUserId, long PatientId);
