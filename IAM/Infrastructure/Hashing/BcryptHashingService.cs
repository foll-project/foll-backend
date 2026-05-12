using BCrypt.Net;
using foll_backend.IAM.Application.OutboundServices;

namespace foll_backend.IAM.Infrastructure.Hashing;

public class BcryptHashingService : IHashingService
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña es obligatoria.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (string.IsNullOrWhiteSpace(passwordHash)) return false;

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
