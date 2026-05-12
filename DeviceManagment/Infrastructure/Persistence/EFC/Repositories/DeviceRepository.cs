using DeviceEntity = foll_backend.DeviceManagment.Domain.Model.Entities.Device;
using foll_backend.DeviceManagment.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.DeviceManagment.Infrastructure.Persistence.EFC.Repositories;

public class DeviceRepository : BaseRepository<DeviceEntity>, IDeviceRepository
{
    public DeviceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<DeviceEntity?> FindByAssignedPatientIdAsync(long patientId)
    {
        if (patientId <= 0) return null;

        return await Context.Set<DeviceEntity>()
            .FirstOrDefaultAsync(d => d.AssignedPatientId == patientId);
    }

    public async Task<IReadOnlyCollection<long>> ListMonitoredActiveDeviceIdsAsync()
    {
        return await Context.Set<DeviceEntity>()
            .Where(d =>
                d.Status == Domain.Model.Enums.DeviceStatus.Active &&
                d.AssignedPatientId != null &&
                d.ConnectivityStatus != null &&
                d.MonitoringStartedAt != null)
            .Select(d => d.DeviceId)
            .ToListAsync();
    }
}
