namespace foll_backend.Care.Domain.Model.Entities;

public class EmergencyContact
{
    public long EmergencyContactId { get; private set; }
    public long PatientId { get; private set; }
    public string FullName { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Relationship { get; private set; }

    protected EmergencyContact()
    {
        FullName = string.Empty;
        PhoneNumber = string.Empty;
        Relationship = string.Empty;
    }

    public EmergencyContact(string fullName, string phoneNumber, string relationship)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("El nombre es obligatorio.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("El teléfono es obligatorio.", nameof(phoneNumber));
        if (string.IsNullOrWhiteSpace(relationship)) throw new ArgumentException("La relación es obligatoria.", nameof(relationship));

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        Relationship = relationship.Trim();
    }
}
