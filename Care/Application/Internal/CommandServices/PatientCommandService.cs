using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Care.Domain.Services;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend.Care.Application.Internal.CommandServices;

public class PatientCommandService : IPatientCommandService
{
    private readonly IPatientRepository _patientRepository;
    private readonly IPatientInvitationRepository _invitationRepository;
    private readonly IRelationshipTypeRepository _relationshipTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PatientCommandService(
        IPatientRepository patientRepository,
        IPatientInvitationRepository invitationRepository,
        IRelationshipTypeRepository relationshipTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _invitationRepository = invitationRepository;
        _relationshipTypeRepository = relationshipTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<long> Handle(CreatePatientCommand command)
    {
        if (await _relationshipTypeRepository.FindByIdAsync(command.RelationshipTypeId) is null)
            throw new InvalidOperationException("RelationshipType inválido.");

        var patient = new Patient(
            command.Dni,
            command.FirstName,
            command.LastName,
            command.BirthDate,
            command.ActorUserId,
            command.BloodType,
            command.MedicalConditions);

        patient.AddCaregiver(command.ActorUserId, command.RelationshipTypeId);

        await _patientRepository.AddAsync(patient);
        await _unitOfWork.CompleteAsync();
        return patient.PatientId;
    }

    public async Task Handle(UpdatePatientCommand command)
    {
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        if (patient.OfficialGuardianUserId != command.ActorUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede actualizar el paciente.");

        patient.UpdateBasicInfo(command.FirstName, command.LastName, command.BirthDate, command.BloodType, command.MedicalConditions);

        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(AssignGuardShiftCommand command)
    {
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        patient.AssignGuardShift(command.NewCurrentGuardianUserId, command.ActorUserId);

        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(RestoreGuardShiftCommand command)
    {
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        patient.RestoreGuardShift(command.ActorUserId);

        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<long> Handle(AddEmergencyContactCommand command)
    {
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        EnsureUserLinkedToPatient(patient, command.ActorUserId);

        patient.AddEmergencyContact(command.FullName, command.PhoneNumber, command.Relationship);

        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();

        return patient.EmergencyContacts.Last().EmergencyContactId;
    }

    public async Task Handle(RemoveEmergencyContactCommand command)
    {
        var patient = await _patientRepository.FindByIdAsync(command.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        EnsureUserLinkedToPatient(patient, command.ActorUserId);

        patient.RemoveEmergencyContact(command.EmergencyContactId);

        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<long> Handle(CreateInvitationCommand command)
    {
        if (await _relationshipTypeRepository.FindByIdAsync(command.RelationshipTypeId) is null)
            throw new InvalidOperationException("RelationshipType inválido.");

        var patient = await _patientRepository.FindByDniAsync(command.PatientDni);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado por DNI.");

        if (IsUserLinkedToPatient(patient, command.ActorUserId))
            throw new InvalidOperationException("No puedes enviar una invitación a un paciente que ya está vinculado a tu usuario.");

        var invitation = new PatientInvitation(patient.PatientId, command.ActorUserId, command.RelationshipTypeId, command.ExpiresAt);
        await _invitationRepository.AddAsync(invitation);
        await _unitOfWork.CompleteAsync();

        return invitation.PatientInvitationId;
    }

    public async Task Handle(AcceptInvitationCommand command)
    {
        var invitation = await _invitationRepository.FindByIdAsync(command.InvitationId);
        if (invitation is null) throw new InvalidOperationException("Invitación no encontrada.");

        var patient = await _patientRepository.FindByIdAsync(invitation.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        if (patient.OfficialGuardianUserId != command.ActorUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede aceptar invitaciones.");

        invitation.Accept();
        patient.AddCaregiver(invitation.InvitingUserId, invitation.RelationshipTypeId);

        _invitationRepository.Update(invitation);
        _patientRepository.Update(patient);
        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(RejectInvitationCommand command)
    {
        var invitation = await _invitationRepository.FindByIdAsync(command.InvitationId);
        if (invitation is null) throw new InvalidOperationException("Invitación no encontrada.");

        var patient = await _patientRepository.FindByIdAsync(invitation.PatientId);
        if (patient is null) throw new InvalidOperationException("Paciente no encontrado.");

        if (patient.OfficialGuardianUserId != command.ActorUserId)
            throw new InvalidOperationException("Solo el OficialGuardian puede rechazar invitaciones.");

        invitation.Reject();
        _invitationRepository.Update(invitation);
        await _unitOfWork.CompleteAsync();
    }

    private static void EnsureUserLinkedToPatient(Patient patient, long actorUserId)
    {
        if (IsUserLinkedToPatient(patient, actorUserId)) return;

        throw new InvalidOperationException("No tienes permisos sobre este paciente.");
    }

    private static bool IsUserLinkedToPatient(Patient patient, long userId)
    {
        return userId == patient.OfficialGuardianUserId || patient.Caregivers.Any(c => c.UserId == userId);
    }
}
