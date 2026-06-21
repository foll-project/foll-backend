using Moq;
using foll_backend.Care.Application.Internal.CommandServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend_tets;

[TestFixture]
public class RemoveEmergencyContactCommandTests
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
    public async Task Handle_RemoveEmergencyContact_WhenUserIsLinked_RemovesContactAndSaves()
    {
        // Arrange
        long patientId = 1;
        long officialGuardianId = 100;
        long emergencyContactId = 5;
        
        var patient = new Patient("12345678", "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);
        typeof(Patient).GetProperty("PatientId")?.SetValue(patient, patientId);
        
        // Agregamos un contacto previo
        patient.AddEmergencyContact("Dr. House", "999888777", "Doctor");
        typeof(EmergencyContact).GetProperty("EmergencyContactId")?.SetValue(patient.EmergencyContacts[0], emergencyContactId);

        var command = new RemoveEmergencyContactCommand(
            PatientId: patientId, 
            EmergencyContactId: emergencyContactId, 
            ActorUserId: officialGuardianId);

        _patientRepositoryMock.Setup(repo => repo.FindByIdAsync(command.PatientId)).ReturnsAsync(patient);

        // Act
        await _service.Handle(command);

        // Assert
        Assert.That(patient.EmergencyContacts.Count, Is.EqualTo(0)); // Verificamos que se borró
        
        _patientRepositoryMock.Verify(repo => repo.Update(patient), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }
}