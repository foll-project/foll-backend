using DeviceEntity = foll_backend.DeviceManagment.Domain.Model.Entities.Device;
using foll_backend.DeviceManagment.Domain.Model.Queries;

namespace foll_backend.DeviceManagment.Domain.Services;

public interface IDeviceQueryService
{
    Task<DeviceEntity?> Handle(GetDeviceStatusByIdQuery query);
    Task<DeviceEntity?> Handle(GetDeviceByPatientIdQuery query);
}
