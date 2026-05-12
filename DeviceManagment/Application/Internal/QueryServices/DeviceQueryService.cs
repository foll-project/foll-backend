using foll_backend.DeviceManagment.Application.OutboundServices;
using DeviceEntity = foll_backend.DeviceManagment.Domain.Model.Entities.Device;
using foll_backend.DeviceManagment.Domain.Model.Queries;
using foll_backend.DeviceManagment.Domain.Repositories;
using foll_backend.DeviceManagment.Domain.Services;

namespace foll_backend.DeviceManagment.Application.Internal.QueryServices;

public class DeviceQueryService : IDeviceQueryService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IPatientAccessService _patientAccessService;

    public DeviceQueryService(IDeviceRepository deviceRepository, IPatientAccessService patientAccessService)
    {
        _deviceRepository = deviceRepository;
        _patientAccessService = patientAccessService;
    }

    public async Task<DeviceEntity?> Handle(GetDeviceStatusByIdQuery query)
    {
        var device = await _deviceRepository.FindByIdAsync(query.DeviceId);
        if (device is null) return null;
        if (!device.AssignedPatientId.HasValue)
            throw new InvalidOperationException("El dispositivo no está vinculado a ningún paciente.");

        if (!await _patientAccessService.CanManageDevicesAsync(query.ActorUserId, device.AssignedPatientId.Value))
            throw new InvalidOperationException("No tienes permisos para consultar este dispositivo.");

        return device;
    }

    public async Task<DeviceEntity?> Handle(GetDeviceByPatientIdQuery query)
    {
        if (!await _patientAccessService.CanManageDevicesAsync(query.ActorUserId, query.PatientId))
            throw new InvalidOperationException("No tienes permisos para consultar el dispositivo de este paciente.");

        return await _deviceRepository.FindByAssignedPatientIdAsync(query.PatientId);
    }
}
