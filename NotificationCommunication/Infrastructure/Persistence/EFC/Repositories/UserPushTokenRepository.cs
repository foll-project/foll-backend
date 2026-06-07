using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.NotificationCommunication.Infrastructure.Persistence.EFC.Repositories;

public class UserPushTokenRepository : BaseRepository<UserPushToken>, IUserPushTokenRepository
{
    public UserPushTokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyCollection<UserPushToken>> ListByUserIdAsync(long userId)
    {
        if (userId <= 0) return Array.Empty<UserPushToken>();

        return await Context.Set<UserPushToken>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.UpdatedAt)
            .ThenByDescending(t => t.UserPushTokenId)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<UserPushToken>> ListActiveByUserIdAsync(long userId)
    {
        if (userId <= 0) return Array.Empty<UserPushToken>();

        return await Context.Set<UserPushToken>()
            .Where(t => t.UserId == userId && t.IsActive)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
    }

    public async Task<UserPushToken?> FindByIdAndUserIdAsync(long userPushTokenId, long userId)
    {
        if (userPushTokenId <= 0 || userId <= 0) return null;

        return await Context.Set<UserPushToken>()
            .FirstOrDefaultAsync(t => t.UserPushTokenId == userPushTokenId && t.UserId == userId);
    }

    public async Task<UserPushToken?> FindByUserIdAndTokenAsync(long userId, string token)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(token)) return null;

        var normalizedToken = token.Trim();
        return await Context.Set<UserPushToken>()
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == normalizedToken);
    }
}
