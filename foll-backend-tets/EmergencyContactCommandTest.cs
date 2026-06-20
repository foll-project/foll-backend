using NUnit.Framework;
using Moq;
using foll_backend.Care.Application.Internal.CommandServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;


namespace foll_backend_tets;

[TestFixture]
public class EmergencyContactCommandTests
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
    public async Task Handle_AddEmergencyContact_WhenUserIsLinked_AddsContactAndSaves()
    {
        // Arrange
        long patientId = 1;
        long officialGuardianId = 100;
        
        var patient = new Patient("12345678", "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);
        
        // El Cuidador Oficial hace la petición
        var command = new AddEmergencyContactCommand(
            PatientId: patientId, 
            FullName: "Dr. House", 
            PhoneNumber: "999888777", 
            Relationship: "Doctor", 
            ActorUserId: officialGuardianId);

        _patientRepositoryMock.Setup(repo => repo.FindByIdAsync(command.PatientId)).ReturnsAsync(patient);

        // Act
        await _service.Handle(command);

        // Assert
        Assert.That(patient.EmergencyContacts.Count, Is.EqualTo(1));
        Assert.That(patient.EmergencyContacts[0].FullName, Is.EqualTo("Dr. House"));
        
        _patientRepositoryMock.Verify(repo => repo.Update(patient), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Test]
    public void Handle_AddEmergencyContact_WhenUserIsNotLinked_ThrowsInvalidOperationException()
    {
        // Arrange
        long patientId = 1;
        long officialGuardianId = 100;
        long intruderUserId = 999; // Intruso
        
        var patient = new Patient("12345678", "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);

        // El intruso intenta hacer la petición
        var command = new AddEmergencyContactCommand(
            PatientId: patientId, 
            FullName: "Dr. House", 
            PhoneNumber: "999888777", 
            Relationship: "Doctor", 
            ActorUserId: intruderUserId);

        _patientRepositoryMock.Setup(repo => repo.FindByIdAsync(command.PatientId)).ReturnsAsync(patient);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.Handle(command));
        Assert.That(ex.Message, Is.EqualTo("No tienes permisos sobre este paciente."));

        _patientRepositoryMock.Verify(repo => repo.Update(It.IsAny<Patient>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
}