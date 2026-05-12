namespace foll_backend.Care.Interfaces.REST.Resources;

public record AddEmergencyContactResource(string FullName, string PhoneNumber, object? Relationship);
