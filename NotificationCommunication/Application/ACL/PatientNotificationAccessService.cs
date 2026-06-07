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

    public async Task<IReadOnlyCollection<PatientNotificationRecipientDto>> GetRecipientsForPatientAsync(long patientId)
    {
        if (patientId <= 0) return Array.Empty<PatientNotificationRecipientDto>();

        var access = await _patientNotificationAcl.GetPatientNotificationAccessByIdAsync(patientId);
        if (access is null) return Array.Empty<PatientNotificationRecipientDto>();

        var caregiverUserIds = access.CaregiverUserIds
            .Where(userId => userId > 0)
            .ToHashSet();

        var recipientUserIds = new List<long>();
        if (access.OfficialGuardianUserId > 0)
            recipientUserIds.Add(access.OfficialGuardianUserId);

        recipientUserIds.AddRange(caregiverUserIds);

        if (access.CurrentGuardianUserId is > 0)
        {
            var currentGuardianUserId = access.CurrentGuardianUserId.Value;
            var isAuthorized = currentGuardianUserId == access.OfficialGuardianUserId ||
                               caregiverUserIds.Contains(currentGuardianUserId);

            if (isAuthorized)
                recipientUserIds.Add(currentGuardianUserId);
        }

        return recipientUserIds
            .Where(userId => userId > 0)
            .Distinct()
            .Select(userId => new PatientNotificationRecipientDto(access.PatientId, userId))
            .ToList();
    }
}
