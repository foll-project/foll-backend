using foll_backend.Shared.Domain.Model.Entities;

namespace foll_backend.Shared.Domain.Repositories;

public interface IOutboxMessageRepository : IBaseRepository<OutboxMessage>
{
    Task<IReadOnlyCollection<OutboxMessage>> ListPendingAsync(int batchSize);
}
