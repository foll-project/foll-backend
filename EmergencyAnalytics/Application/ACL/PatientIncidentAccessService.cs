using foll_backend.Care.Application.ACL;
using foll_backend.EmergencyAnalytics.Application.OutboundServices;

namespace foll_backend.EmergencyAnalytics.Application.ACL;

public class PatientIncidentAccessService : IPatientIncidentAccessService
{
    private readonly IPatientEmergencyAcl _patientEmergencyAcl;

    public PatientIncidentAccessService(IPatientEmergencyAcl patientEmergencyAcl)
    {
        _patientEmergencyAcl = patientEmergencyAcl;
    }

    public async Task<bool> CanAccessIncidentAsync(long actorUserId, long patientId)
    {
        if (actorUserId <= 0 || patientId <= 0) return false;

        var access = await _patientEmergencyAcl.GetPatientEmergencyAccessByIdAsync(patientId);
        if (access is null) return false;

        return access.OfficialGuardianUserId == actorUserId ||
               access.CurrentGuardianUserId == actorUserId ||
               access.CaregiverUserIds.Contains(actorUserId);
    }
}
