namespace foll_backend.Care.Domain.Model.ValueObjects;

public record UserInfo(long UserId, string Email, string FirstName, string LastName, string? PhoneNumber);