using foll_backend.DeviceManagment.Domain.Model.Entities;
using foll_backend.DeviceManagment.Domain.Model.Enums;
using foll_backend.DeviceManagment.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.DeviceManagment.Infrastructure.Persistence.EFC.Repositories;

public class DeviceEventRepository : BaseRepository<DeviceEvent>, IDeviceEventRepository
{
    public DeviceEventRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<DeviceEvent?> FindLatestUnresolvedByDeviceIdAndTypeAsync(long deviceId, DeviceEventType eventType)
    {
        if (deviceId <= 0) return null;

        return await Context.Set<DeviceEvent>()
            .Where(e => e.DeviceId == deviceId && e.EventType == eventType && !e.IsResolved)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
