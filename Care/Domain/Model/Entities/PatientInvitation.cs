using foll_backend.Care.Domain.Model.Enums;

namespace foll_backend.Care.Domain.Model.Entities;

public class PatientInvitation
{
    public long PatientInvitationId { get; private set; }
    public long PatientId { get; private set; }
    public long InvitingUserId { get; private set; }
    public short RelationshipTypeId { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    protected PatientInvitation()
    {
    }

    public PatientInvitation(long patientId, long invitingUserId, short relationshipTypeId, DateTime expiresAt)
    {
        if (patientId <= 0) throw new ArgumentOutOfRangeException(nameof(patientId));
        if (invitingUserId <= 0) throw new ArgumentOutOfRangeException(nameof(invitingUserId));
        if (relationshipTypeId <= 0) throw new ArgumentOutOfRangeException(nameof(relationshipTypeId));
        if (expiresAt <= DateTime.UtcNow) throw new ArgumentException("La invitación debe expirar en el futuro.", nameof(expiresAt));

        PatientId = patientId;
        InvitingUserId = invitingUserId;
        RelationshipTypeId = relationshipTypeId;
        ExpiresAt = expiresAt;
        Status = InvitationStatus.Pending;
    }

    public void Accept()
    {
        if (IsExpired()) throw new InvalidOperationException("La invitación está expirada.");
        Status = InvitationStatus.Accepted;
    }

    public void Reject()
    {
        if (IsExpired()) throw new InvalidOperationException("La invitación está expirada.");
        Status = InvitationStatus.Rejected;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}
