using DeviceEntity = foll_backend.DeviceManagment.Domain.Model.Entities.Device;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.DeviceManagment.Domain.Repositories;

public interface IDeviceRepository : IBaseRepository<DeviceEntity>
{
    Task<DeviceEntity?> FindByAssignedPatientIdAsync(long patientId);
    Task<IReadOnlyCollection<long>> ListMonitoredActiveDeviceIdsAsync();
}
