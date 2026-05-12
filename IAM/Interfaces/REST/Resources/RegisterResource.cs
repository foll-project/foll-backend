namespace foll_backend.IAM.Interfaces.REST.Resources;

public record RegisterResource(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber);
