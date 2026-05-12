using foll_backend.IAM.Domain.Model.Entities;
using foll_backend.IAM.Domain.Model.Queries;

namespace foll_backend.IAM.Domain.Services;

public interface IUserQueryService
{
    Task<(User user, string token)?> AuthenticateAsync(LoginQuery query);
}
