using foll_backend.NotificationCommunication.Domain.Model.Entities;
using foll_backend.NotificationCommunication.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.NotificationCommunication.Infrastructure.Persistence.EFC.Repositories;

public class NotificationLogRepository : BaseRepository<NotificationLog>, INotificationLogRepository
{
    public NotificationLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyCollection<NotificationLog>> ListByUserIdAsync(long userId)
    {
        if (userId <= 0) return Array.Empty<NotificationLog>();

        return await Context.Set<NotificationLog>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.NotificationLogId)
            .ToListAsync();
    }

    public async Task<NotificationLog?> FindByIdAndUserIdAsync(long notificationLogId, long userId)
    {
        if (notificationLogId <= 0 || userId <= 0) return null;

        return await Context.Set<NotificationLog>()
            .FirstOrDefaultAsync(n => n.NotificationLogId == notificationLogId && n.UserId == userId);
    }
}
