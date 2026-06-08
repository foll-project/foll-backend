using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.Care.Domain.Repositories;

public interface IPatientInvitationRepository : IBaseRepository<PatientInvitation>
{
    Task<IEnumerable<PatientInvitation>> ListByPatientIdAsync(long patientId);

    // Invitaciones enviadas por un usuario (solicitante).
    Task<IEnumerable<PatientInvitation>> ListByInvitingUserIdAsync(long invitingUserId);

    // Invitaciones recibidas por un cuidador principal (de los pacientes que tutela).
    Task<IEnumerable<PatientInvitation>> ListForOfficialGuardianAsync(long guardianUserId);
}
