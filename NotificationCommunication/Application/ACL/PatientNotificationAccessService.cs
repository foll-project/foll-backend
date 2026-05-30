using foll_backend.NotificationCommunication.Application.OutboundServices;
using CarePatientNotificationAcl = foll_backend.Care.Application.ACL.IPatientNotificationAcl;

namespace foll_backend.NotificationCommunication.Application.ACL;

public class PatientNotificationAccessService : IPatientNotificationAccessService
{
    private readonly CarePatientNotificationAcl _patientNotificationAcl;

    public PatientNotificationAccessService(CarePatientNotificationAcl patientNotificationAcl)
    {
        _patientNotificationAcl = patientNotificationAcl;
    }

    public async Task<PatientNotificationRecipientDto?> GetRecipientForPatientAsync(long patientId)
    {
        if (patientId <= 0) return null;

        var access = await _patientNotificationAcl.GetPatientNotificationAccessByIdAsync(patientId);
        if (access is null) return null;

        var recipientUserId = access.CurrentGuardianUserId is > 0
            ? access.CurrentGuardianUserId.Value
            : access.OfficialGuardianUserId;

        return recipientUserId <= 0 ? null : new PatientNotificationRecipientDto(access.PatientId, recipientUserId);
    }
}
