using Moq;
using foll_backend.Care.Application.Internal.CommandServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;


namespace foll_backend_tets;

[TestFixture]
public class AssignGuardShiftCommandTests
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
    public async Task Handle_AssignGuardShift_WhenActorIsOfficialGuardian_UpdatesGuardianAndSaves()
    {
        // Arrange
        long patientId = 1;
        long officialGuardianId = 100;
        long newGuardianId = 200; // El familiar que toma el turno
        
        var patient = new Patient("12345678", "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);

        var command = new AssignGuardShiftCommand(
            PatientId: patientId, 
            NewCurrentGuardianUserId: newGuardianId, 
            ActorUserId: officialGuardianId);

        _patientRepositoryMock.Setup(repo => repo.FindByIdAsync(command.PatientId)).ReturnsAsync(patient);

        // Act
        await _service.Handle(command);

        // Assert
        Assert.That(patient.CurrentGuardianUserId, Is.EqualTo(newGuardianId));
        
        _patientRepositoryMock.Verify(repo => repo.Update(patient), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Test]
    public void Handle_AssignGuardShift_WhenActorIsNotOfficialGuardian_ThrowsInvalidOperationException()
    {
        // Arrange
        long patientId = 1;
        long officialGuardianId = 100;
        long newGuardianId = 200;
        long intruderUserId = 999;
        
        var patient = new Patient("12345678", "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);

        var command = new AssignGuardShiftCommand(
            PatientId: patientId, 
            NewCurrentGuardianUserId: newGuardianId, 
            ActorUserId: intruderUserId); // Intruso intenta asignar el turno

        _patientRepositoryMock.Setup(repo => repo.FindByIdAsync(command.PatientId)).ReturnsAsync(patient);

        // Act & Assert
        // Aquí capturamos la excepción que lanza la entidad Patient internamente
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.Handle(command));
        Assert.That(ex.Message, Is.EqualTo("Solo el OficialGuardian puede asignar el turno de guardia."));

        _patientRepositoryMock.Verify(repo => repo.Update(It.IsAny<Patient>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
}