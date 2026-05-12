using foll_backend.Care.Domain.Model.Enums;

namespace foll_backend.Care.Domain.Model.Commands;

public record UpdatePatientCommand(
    long ActorUserId,
    long PatientId,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    BloodType BloodType,
    Dictionary<string, string>? MedicalConditions);
