using foll_backend.IAM.Domain.Model.Entities;

namespace foll_backend.IAM.Application.OutboundServices;

public interface ITokenService
{
    string GenerateToken(User user);
}
