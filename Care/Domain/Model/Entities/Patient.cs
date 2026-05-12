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

    private readonly List<CaregiverRole> _caregivers = [];
    public IReadOnlyCollection<CaregiverRole> Caregivers => _caregivers;

    private readonly List<EmergencyContact> _emergencyContacts = [];
    public IReadOnlyCollection<EmergencyContact> EmergencyContacts => _emergencyContacts;

    private readonly List<PatientInvitation> _invitations = [];
    public IReadOnlyCollection<PatientInvitation> Invitations => _invitations;

    protected Patient()
    {
        Dni = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        MedicalConditions = new Dictionary<string, string>();
        BloodType = BloodType.Unknown;
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
    }

    public void AssignGuardShift(long newCurrentGuardianUserId, long actorUserId)
    {
        if (actorUserId != OfficialGuardianUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede asignar el turno de guardia.");

        if (newCurrentGuardianUserId <= 0) throw new ArgumentOutOfRangeException(nameof(newCurrentGuardianUserId));
        CurrentGuardianUserId = newCurrentGuardianUserId;
    }

    public void ClearGuardShift(long actorUserId)
    {
        if (actorUserId != OfficialGuardianUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede limpiar el turno de guardia.");

        CurrentGuardianUserId = OfficialGuardianUserId;
    }

    public void AddCaregiver(long userId, string relationshipName)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (string.IsNullOrWhiteSpace(relationshipName))
            throw new ArgumentException("El tipo de relación es obligatorio.", nameof(relationshipName));

        if (_caregivers.Any(c => c.UserId == userId))
            throw new InvalidOperationException("El usuario ya está vinculado como cuidador.");

        _caregivers.Add(new CaregiverRole(userId, relationshipName.Trim()));
    }

    public void AddEmergencyContact(string fullName, string phoneNumber, string relationship)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("El nombre es obligatorio.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("El teléfono es obligatorio.", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(relationship)) throw new ArgumentException("La relación es obligatoria.", nameof(relationship));

        _emergencyContacts.Add(new EmergencyContact(fullName.Trim(), phoneNumber.Trim(), relationship.Trim()));
    }
}
