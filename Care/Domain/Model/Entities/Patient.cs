using foll_backend.Care.Domain.Model.Enums;
using foll_backend.Care.Domain.Model.ValueObjects;

namespace foll_backend.Care.Domain.Model.Entities;

public class Patient
{
    public long PatientId { get; private set; }
    public string Dni { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public BloodType BloodType { get; private set; }

    public Dictionary<string, string> MedicalConditions { get; private set; }

    public long? CurrentGuardianUserId { get; private set; }
    public long OfficialGuardianUserId { get; private set; }

    public List<CaregiverRole> Caregivers { get; private set; }
    public List<EmergencyContact> EmergencyContacts { get; private set; }
    public List<PatientInvitation> Invitations { get; private set; }

    protected Patient()
    {
        Dni = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        MedicalConditions = new Dictionary<string, string>();
        BloodType = BloodType.Unknown;

        Caregivers = new List<CaregiverRole>();
        EmergencyContacts = new List<EmergencyContact>();
        Invitations = new List<PatientInvitation>();
    }

    public Patient(
        string dni,
        string firstName,
        string lastName,
        DateOnly birthDate,
        long officialGuardianUserId,
        BloodType bloodType = BloodType.Unknown,
        Dictionary<string, string>? medicalConditions = null)
    {
        if (string.IsNullOrWhiteSpace(dni)) throw new ArgumentException("El DNI del paciente es obligatorio.", nameof(dni));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("El nombre es obligatorio.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("El apellido es obligatorio.", nameof(lastName));
        if (officialGuardianUserId <= 0) throw new ArgumentOutOfRangeException(nameof(officialGuardianUserId));

        Dni = dni.Trim();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        BirthDate = birthDate;
        BloodType = bloodType;
        MedicalConditions = medicalConditions ?? new Dictionary<string, string>();

        OfficialGuardianUserId = officialGuardianUserId;
        CurrentGuardianUserId = officialGuardianUserId;

        Caregivers = new List<CaregiverRole>();
        EmergencyContacts = new List<EmergencyContact>();
        Invitations = new List<PatientInvitation>();
    }

    public void AssignGuardShift(long newCurrentGuardianUserId, long actorUserId)
    {
        if (actorUserId != OfficialGuardianUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede asignar el turno de guardia.");

        if (newCurrentGuardianUserId <= 0) throw new ArgumentOutOfRangeException(nameof(newCurrentGuardianUserId));
        CurrentGuardianUserId = newCurrentGuardianUserId;
    }

    public void RestoreGuardShift(long actorUserId)
    {
        if (actorUserId != OfficialGuardianUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede limpiar el turno de guardia.");

        CurrentGuardianUserId = OfficialGuardianUserId;
    }

    public void UpdateBasicInfo(
        string firstName,
        string lastName,
        DateOnly birthDate,
        BloodType bloodType,
        Dictionary<string, string>? medicalConditions)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("El nombre es obligatorio.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("El apellido es obligatorio.", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        BirthDate = birthDate;
        BloodType = bloodType;
        MedicalConditions = medicalConditions ?? new Dictionary<string, string>();
    }

    public void AddCaregiver(long userId, short relationshipTypeId)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (relationshipTypeId <= 0) throw new ArgumentOutOfRangeException(nameof(relationshipTypeId));

        if (Caregivers.Any(c => c.UserId == userId))
            throw new InvalidOperationException("El usuario ya está vinculado como cuidador.");

        Caregivers.Add(new CaregiverRole(userId, relationshipTypeId));
    }

    public void RemoveCaregiver(long userId)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        Caregivers.RemoveAll(c => c.UserId == userId);
    }

    public void AddEmergencyContact(string fullName, string phoneNumber, string relationship)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("El nombre es obligatorio.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("El teléfono es obligatorio.", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(relationship)) throw new ArgumentException("La relación es obligatoria.", nameof(relationship));

        EmergencyContacts.Add(new EmergencyContact(fullName.Trim(), phoneNumber.Trim(), relationship.Trim()));
    }

    public void RemoveEmergencyContact(long emergencyContactId)
    {
        if (emergencyContactId <= 0) throw new ArgumentOutOfRangeException(nameof(emergencyContactId));
        EmergencyContacts.RemoveAll(c => c.EmergencyContactId == emergencyContactId);
    }

    public PatientInvitation CreateInvitation(long invitingUserId, short relationshipTypeId, DateTime expiresAt)
    {
        if (PatientId <= 0)
            throw new InvalidOperationException("El paciente debe estar persistido antes de crear invitaciones.");

        var invitation = new PatientInvitation(PatientId, invitingUserId, relationshipTypeId, expiresAt);
        Invitations.Add(invitation);
        return invitation;
    }

    public void AcceptInvitation(long invitationId, long actorUserId)
    {
        if (actorUserId != OfficialGuardianUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede aceptar invitaciones.");

        var invitation = Invitations.FirstOrDefault(i => i.PatientInvitationId == invitationId);
        if (invitation is null)
            throw new InvalidOperationException("Invitación no encontrada.");

        invitation.Accept();
        AddCaregiver(invitation.InvitingUserId, invitation.RelationshipTypeId);
    }

    public void RejectInvitation(long invitationId, long actorUserId)
    {
        if (actorUserId != OfficialGuardianUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede rechazar invitaciones.");

        var invitation = Invitations.FirstOrDefault(i => i.PatientInvitationId == invitationId);
        if (invitation is null)
            throw new InvalidOperationException("Invitación no encontrada.");

        invitation.Reject();
    }
}
