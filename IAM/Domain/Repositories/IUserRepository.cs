using foll_backend.IAM.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.IAM.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> FindByEmailAsync(string email);
}
