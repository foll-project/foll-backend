using foll_backend.DeviceManagment.Application.OutboundServices;
using CarePatientDeviceAcl = foll_backend.Care.Application.ACL.IPatientDeviceAcl;

namespace foll_backend.DeviceManagment.Application.ACL;

public class PatientAccessService : IPatientAccessService
{
    private readonly CarePatientDeviceAcl _patientDeviceAcl;

    public PatientAccessService(CarePatientDeviceAcl patientDeviceAcl)
    {
        _patientDeviceAcl = patientDeviceAcl;
    }

    public async Task<bool> CanManageDevicesAsync(long actorUserId, long patientId)
    {
        if (actorUserId <= 0 || patientId <= 0) return false;

        var access = await _patientDeviceAcl.GetPatientDeviceAccessByIdAsync(patientId);
        return access is not null && access.OfficialGuardianUserId == actorUserId;
    }
}
