using foll_backend.Care.Domain.Model.Enums;

namespace foll_backend.Care.Interfaces.REST.Resources;

public record UpdatePatientResource(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    BloodType BloodType,
    Dictionary<string, string>? MedicalConditions);
