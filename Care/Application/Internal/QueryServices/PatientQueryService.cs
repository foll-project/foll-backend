using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.Queries;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Care.Domain.Services;
using foll_backend.Care.Domain.Model.ValueObjects;

namespace foll_backend.Care.Application.Internal.QueryServices;

public class PatientQueryService : IPatientQueryService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IPatientInvitationRepository _invitationRepository;

    public PatientQueryService(IPatientRepository patientRepository, IPatientInvitationRepository invitationRepository)
    {
        _patientRepository = patientRepository;
        _invitationRepository = invitationRepository;
    }

    public async Task<Patient?> Handle(GetPatientByIdQuery query)
    {
        var patient = await _patientRepository.FindByIdAsync(query.PatientId);
        if (patient is null) return null;

        EnsureUserLinkedToPatient(patient, query.ActorUserId);
        return patient;
    }

    public async Task<Patient?> Handle(GetPatientByDniQuery query)
    {
        var patient = await _patientRepository.FindByDniAsync(query.Dni);
        if (patient is null) return null;

        EnsureUserLinkedToPatient(patient, query.ActorUserId);
        return patient;
    }

    public async Task<IEnumerable<Patient>> Handle(GetPatientsForUserQuery query)
    {
        return await _patientRepository.ListForUserAsync(query.UserId);
    }

    public async Task<IEnumerable<CaregiverRole>> Handle(GetCaregiversByPatientIdQuery query)
    {
        var patient = await _patientRepository.FindByIdAsync(query.PatientId);
        if (patient is null) return Array.Empty<CaregiverRole>();

        if (patient.OfficialGuardianUserId != query.ActorUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede ver los cuidadores.");

        return patient.Caregivers;
    }

    public async Task<IEnumerable<PatientInvitation>> Handle(GetInvitationsByPatientDniQuery query)
    {
        var patient = await _patientRepository.FindByDniAsync(query.PatientDni);
        if (patient is null) return Array.Empty<PatientInvitation>();

        EnsureUserLinkedToPatient(patient, query.ActorUserId);
        return await _invitationRepository.ListByPatientIdAsync(patient.PatientId);
    }
    
    public async Task<IEnumerable<PatientAnnotation>> Handle(GetPatientAnnotationsQuery query)
    {
        var patient = await _patientRepository.FindByIdAsync(query.PatientId);
        if (patient is null) return Array.Empty<PatientAnnotation>();

        EnsureUserLinkedToPatient(patient, query.ActorUserId);

        // Devolvemos ordenado desde el más reciente al más antiguo
        return patient.Annotations.OrderByDescending(a => a.CreatedAt).ToList(); 
    }

    public async Task<PatientInvitation?> Handle(GetInvitationByIdQuery query)
    {
        var invitation = await _invitationRepository.FindByIdAsync(query.InvitationId);
        if (invitation is null) return null;

        var patient = await _patientRepository.FindByIdAsync(invitation.PatientId);
        if (patient is null) return null;

        EnsureUserLinkedToPatient(patient, query.ActorUserId);
        return invitation;
    }

    private static void EnsureUserLinkedToPatient(Patient patient, long actorUserId)
    {
        if (actorUserId == patient.OfficialGuardianUserId) return;
        if (patient.Caregivers.Any(c => c.UserId == actorUserId)) return;
        throw new InvalidOperationException("No tienes permisos sobre este paciente.");
    }
}
