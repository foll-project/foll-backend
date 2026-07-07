using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.NotificationCommunication.Infrastructure.Persistence.EFC.Repositories;

public class EmergencyLocationAccessLinkRepository : BaseRepository<EmergencyLocationAccessLink>, IEmergencyLocationAccessLinkRepository
{
    public EmergencyLocationAccessLinkRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<EmergencyLocationAccessLink?> FindByTokenHashAsync(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash)) return null;

        var normalizedTokenHash = tokenHash.Trim();
        return await Context.Set<EmergencyLocationAccessLink>()
            .FirstOrDefaultAsync(link => link.TokenHash == normalizedTokenHash);
    }
}
