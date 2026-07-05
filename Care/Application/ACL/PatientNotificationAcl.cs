using foll_backend.Care.Application.OutboundServices;
using foll_backend.Care.Domain.Repositories;

namespace foll_backend.Care.Application.ACL;

public class PatientNotificationAcl : IPatientNotificationAcl
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUserInfoService _userInfoService;

    public PatientNotificationAcl(IPatientRepository patientRepository, IUserInfoService userInfoService)
    {
        _patientRepository = patientRepository;
        _userInfoService = userInfoService;
    }

    public async Task<PatientNotificationAccessDto?> GetPatientNotificationAccessByIdAsync(long patientId)
    {
        if (patientId <= 0) return null;

        var patient = await _patientRepository.FindByIdAsync(patientId);
        if (patient is null) return null;

        var caregiverUserIds = patient.Caregivers
            .Select(c => c.UserId)
            .Where(userId => userId > 0)
            .ToHashSet();

        var recipientUserIds = new List<long>();
        if (patient.OfficialGuardianUserId > 0)
            recipientUserIds.Add(patient.OfficialGuardianUserId);

        recipientUserIds.AddRange(caregiverUserIds);

        if (patient.CurrentGuardianUserId is > 0)
        {
            var currentGuardianUserId = patient.CurrentGuardianUserId.Value;
            var isAuthorized = currentGuardianUserId == patient.OfficialGuardianUserId ||
                               caregiverUserIds.Contains(currentGuardianUserId);

            if (isAuthorized)
                recipientUserIds.Add(currentGuardianUserId);
        }

        var userRecipients = new List<PatientNotificationUserRecipientDto>();
        foreach (var userId in recipientUserIds.Where(userId => userId > 0).Distinct())
        {
            var userInfo = await _userInfoService.FindByIdAsync(userId);
            var fullName = userInfo is null
                ? $"Usuario {userId}"
                : $"{userInfo.FirstName} {userInfo.LastName}".Trim();

            userRecipients.Add(new PatientNotificationUserRecipientDto(
                userId,
                string.IsNullOrWhiteSpace(fullName) ? $"Usuario {userId}" : fullName,
                userInfo?.PhoneNumber));
        }

        return new PatientNotificationAccessDto(
            patient.PatientId,
            userRecipients,
            patient.EmergencyContacts
                .Where(contact => !string.IsNullOrWhiteSpace(contact.PhoneNumber))
                .Select(contact => new PatientNotificationEmergencyContactDto(
                    contact.EmergencyContactId,
                    contact.FullName,
                    contact.PhoneNumber,
                    contact.Relationship))
                .ToArray());
    }
}
