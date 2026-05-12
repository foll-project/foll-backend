namespace foll_backend.IAM.Application.ACL;

public record UserInfoDto(long UserId, string Email, string FirstName, string LastName, string? PhoneNumber);