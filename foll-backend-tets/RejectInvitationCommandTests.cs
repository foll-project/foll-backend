using Moq;
using foll_backend.Care.Application.Internal.CommandServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;


namespace foll_backend_tets;

[TestFixture]
public class RejectInvitationCommandTests
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
    public async Task Handle_RejectInvitation_WhenActorIsOfficialGuardian_RejectsAndDoesNotAddCaregiver()
    {
        // Arrange
        long officialGuardianId = 100;
        long invitingUserId = 200; 
        short relationshipTypeId = 3;
        long patientId = 1;
        long invitationId = 10;

        // CORRECCIÓN: Asignamos explícitamente qué valor va a qué propiedad
        var command = new RejectInvitationCommand(
            InvitationId: invitationId, 
            ActorUserId: officialGuardianId
        );
    
        var patient = new Patient("12345678", "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);
        typeof(Patient).GetProperty("PatientId")?.SetValue(patient, patientId);

        var invitation = new PatientInvitation(patientId, invitingUserId, relationshipTypeId, DateTime.UtcNow.AddDays(7));
        typeof(PatientInvitation).GetProperty("PatientInvitationId")?.SetValue(invitation, invitationId);

        _invitationRepositoryMock.Setup(repo => repo.FindByIdAsync(command.InvitationId)).ReturnsAsync(invitation);
        _patientRepositoryMock.Setup(repo => repo.FindByIdAsync(invitation.PatientId)).ReturnsAsync(patient);

        // Act
        await _service.Handle(command);

        // Assert
        Assert.That(patient.Caregivers.Count, Is.EqualTo(0)); // Garantizamos que NO se unió a la red

        // La invitación SÍ se actualiza en base de datos (pasa a estado rechazado)
        _invitationRepositoryMock.Verify(repo => repo.Update(invitation), Times.Once);
    
        // CORRECCIÓN: Como el abuelito no sufrió cambios, verificamos que NUNCA se intente actualizar
        _patientRepositoryMock.Verify(repo => repo.Update(patient), Times.Never);
    
        // El UnitOfWork guarda los cambios de la invitación
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }
}