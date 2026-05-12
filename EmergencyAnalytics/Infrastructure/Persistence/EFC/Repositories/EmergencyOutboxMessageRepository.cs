using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Persistence.EFC.Repositories;

public class EmergencyOutboxMessageRepository : BaseRepository<EmergencyOutboxMessage>, IEmergencyOutboxMessageRepository
{
    public EmergencyOutboxMessageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyCollection<EmergencyOutboxMessage>> ListPendingAsync(int batchSize)
    {
        return await Context.Set<EmergencyOutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccurredOn)
            .Take(batchSize)
            .ToListAsync();
    }
}
