using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.EmergencyAnalytics.Domain.Repositories;

public interface IFallTypeRepository : IBaseRepository<FallType>
{
    Task<FallType?> FindByIdAsync(short id);
    Task<FallType?> FindByNameAsync(string name);
}
