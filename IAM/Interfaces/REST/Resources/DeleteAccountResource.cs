namespace foll_backend.IAM.Interfaces.REST.Resources;

public record DeleteAccountResource(
    string Email,
    string Password
);
