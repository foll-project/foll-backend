using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.EmergencyAnalytics.Infrastructure.Persistence.EFC.Repositories;

public class FallTypeRepository : BaseRepository<FallType>, IFallTypeRepository
{
    public FallTypeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<FallType?> FindByIdAsync(short id)
    {
        if (id <= 0) return null;

        return await Context.Set<FallType>().FindAsync(id);
    }

    public async Task<FallType?> FindByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var normalizedName = name.Trim().ToUpperInvariant();
        return await Context.Set<FallType>()
            .FirstOrDefaultAsync(f => f.Name == normalizedName);
    }
}
