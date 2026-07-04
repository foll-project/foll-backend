namespace foll_backend.IAM.Domain.Model.Commands;


public record DeleteaccountCommand(
    long UserId,
    string Email,
    string Password
);