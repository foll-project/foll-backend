using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.EmergencyAnalytics.Domain.Repositories;

public interface IEmergencyOutboxMessageRepository : IBaseRepository<EmergencyOutboxMessage>
{
    Task<IReadOnlyCollection<EmergencyOutboxMessage>> ListPendingAsync(int batchSize);
}
