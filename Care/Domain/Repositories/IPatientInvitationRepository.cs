using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.Care.Domain.Repositories;

public interface IPatientInvitationRepository : IBaseRepository<PatientInvitation>
{
    Task<IEnumerable<PatientInvitation>> ListByPatientIdAsync(long patientId);
}
