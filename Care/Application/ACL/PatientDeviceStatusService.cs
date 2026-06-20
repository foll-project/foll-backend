using foll_backend.Care.Application.OutboundServices;
using foll_backend.DeviceManagment.Application.ACL;

namespace foll_backend.Care.Application.ACL;

public class PatientDeviceStatusService : IPatientDeviceStatusService
{
    private readonly IDeviceStatusAcl _deviceStatusAcl;

    public PatientDeviceStatusService(IDeviceStatusAcl deviceStatusAcl)
    {
        _deviceStatusAcl = deviceStatusAcl;
    }

    public async Task<PatientDeviceStatusInfo?> GetByPatientIdAsync(long patientId)
    {
        var status = await _deviceStatusAcl.GetStatusByPatientIdAsync(patientId);
        if (status is null) return null;

        return new PatientDeviceStatusInfo(
            status.DeviceId,
            status.AssignedPatientId,
            status.Status,
            status.ConnectivityStatus,
            status.CurrentBatteryLevel,
            status.IsCharging,
            status.LastHeartbeatAt,
            status.MonitoringStartedAt,
            status.LastConnectivityChangeAt,
            status.IsOnline,
            status.IsLowBattery,
            status.FirmwareVersion);
    }
}
