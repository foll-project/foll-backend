using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.Care.Domain.Repositories;

public interface IPatientRepository : IBaseRepository<Patient>
{
    Task<Patient?> FindByDniAsync(string dni);
    Task<IEnumerable<Patient>> ListForUserAsync(long userId);
}
