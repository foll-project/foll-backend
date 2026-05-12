namespace foll_backend.DeviceManagment.Application.ACL;

public record DeviceAssignmentDto(long DeviceId, long? AssignedPatientId, string Status, string? ConnectivityStatus);
