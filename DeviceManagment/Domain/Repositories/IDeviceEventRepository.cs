using foll_backend.DeviceManagment.Domain.Model.Entities;
using foll_backend.DeviceManagment.Domain.Model.Enums;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.DeviceManagment.Domain.Repositories;

public interface IDeviceEventRepository : IBaseRepository<DeviceEvent>
{
    Task<DeviceEvent?> FindLatestUnresolvedByDeviceIdAndTypeAsync(long deviceId, DeviceEventType eventType);
}
