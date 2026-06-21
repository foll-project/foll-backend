using Moq;
using foll_backend.Care.Application.Internal.CommandServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend_tets;

[TestFixture]
public class AcceptInvitationCommandTests
{
    private Mock<IPatientRepository> _patientRepositoryMock;
    private Mock<IPatientInvitationRepository> _invitationRepositoryMock;
    private Mock<IRelationshipTypeRepository> _relationshipTypeRepositoryMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private PatientCommandService _service;

    [SetUp]
    public void SetUp()
    {
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _invitationRepositoryMock = new Mock<IPatientInvitationRepository>();
        _relationshipTypeRepositoryMock = new Mock<IRelationshipTypeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new PatientCommandService(
            _patientRepositoryMock.Object,
            _invitationRepositoryMock.Object,
            _relationshipTypeRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Test]
    public async Task Handle_AcceptInvitation_WhenActorIsOfficialGuardian_AcceptsAndAddsCaregiver()
    {
        // Arrange
        long officialGuardianId = 100;
        long invitingUserId = 200; 
        short relationshipTypeId = 3;
        long patientId = 1;
        long invitationId = 10;

        var command = new AcceptInvitationCommand(
            InvitationId: invitationId, 
            ActorUserId: officialGuardianId
        );
    
        // 1. Instanciamos el paciente
        var patient = new Patient("12345678", "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);
    
        // Mantenemos Reflection SOLO para inyectar el PatientId, porque Moq lo necesita para la búsqueda encadenada
        typeof(Patient).GetProperty("PatientId")?.SetValue(patient, patientId);

        // 2. SOLUCIÓN: Usamos el constructor real en lugar de Reflection.
        // Esto asegura que las reglas internas y estados iniciales de la entidad se cumplan.
        var invitation = new PatientInvitation(
            patientId, 
            invitingUserId, 
            relationshipTypeId, 
            DateTime.UtcNow.AddDays(7)
        );

        // Le asignamos su ID simulado para mayor precisión
        typeof(PatientInvitation).GetProperty("PatientInvitationId")?.SetValue(invitation, invitationId);

        _invitationRepositoryMock.Setup(repo => repo.FindByIdAsync(command.InvitationId)).ReturnsAsync(invitation);
        _patientRepositoryMock.Setup(repo => repo.FindByIdAsync(invitation.PatientId)).ReturnsAsync(patient);

        // Act
        await _service.Handle(command);

        // Assert
        Assert.That(patient.Caregivers.Count, Is.EqualTo(1));
        Assert.That(patient.Caregivers[0].UserId, Is.EqualTo(invitingUserId));

        _invitationRepositoryMock.Verify(repo => repo.Update(invitation), Times.Once);
        _patientRepositoryMock.Verify(repo => repo.Update(patient), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Test]
    public void Handle_AcceptInvitation_WhenActorIsNotOfficialGuardian_ThrowsInvalidOperationException()
    {
        // Arrange
        long officialGuardianId = 100;
        long intruderUserId = 999; // Alguien intentando aceptar una invitación que no le corresponde
        long patientId = 1;

        var command = new AcceptInvitationCommand(InvitationId: 10, ActorUserId: intruderUserId);
        var patient = new Patient("12345678", "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);

        var invitation = (PatientInvitation)Activator.CreateInstance(typeof(PatientInvitation), nonPublic: true)!;
        typeof(PatientInvitation).GetProperty("PatientId")?.SetValue(invitation, patientId);

        _invitationRepositoryMock.Setup(repo => repo.FindByIdAsync(command.InvitationId)).ReturnsAsync(invitation);
        _patientRepositoryMock.Setup(repo => repo.FindByIdAsync(invitation.PatientId)).ReturnsAsync(patient);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.Handle(command));

        Assert.That(ex.Message, Is.EqualTo("Solo el OficialGuardian puede aceptar invitaciones."));

        // Aseguramos que nada cambie en la base de datos si ocurre un error de permisos
        _invitationRepositoryMock.Verify(repo => repo.Update(It.IsAny<PatientInvitation>()), Times.Never);
        _patientRepositoryMock.Verify(repo => repo.Update(It.IsAny<Patient>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
}