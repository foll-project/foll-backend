using foll_backend.Care.Domain.Model.Enums;

namespace foll_backend.Care.Domain.Model.Commands;

public record CreatePatientCommand(
    long ActorUserId,
    string Dni,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    short RelationshipTypeId,
    BloodType BloodType,
    Dictionary<string, string>? MedicalConditions);
