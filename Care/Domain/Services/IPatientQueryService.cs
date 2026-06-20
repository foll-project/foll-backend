using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.ValueObjects;
using foll_backend.Care.Domain.Model.Queries;

namespace foll_backend.Care.Domain.Services;

public interface IPatientQueryService
{
    Task<Patient?> Handle(GetPatientByIdQuery query);
    Task<Patient?> Handle(GetPatientByDniQuery query);
    Task<IEnumerable<Patient>> Handle(GetPatientsForUserQuery query);

    Task<IEnumerable<CaregiverRole>> Handle(GetCaregiversByPatientIdQuery query);

    Task<IEnumerable<PatientInvitation>> Handle(GetInvitationsByPatientDniQuery query);
    Task<PatientInvitation?> Handle(GetInvitationByIdQuery query);
    Task<IEnumerable<PatientAnnotation>> Handle(GetPatientAnnotationsQuery query);

    Task<IEnumerable<InvitationView>> Handle(GetReceivedInvitationsQuery query);
    Task<IEnumerable<InvitationView>> Handle(GetSentInvitationsQuery query);
}
