namespace foll_backend.Care.Domain.Model.Commands;

public record DeletePatientCommand(long UserId, long PatientId);
