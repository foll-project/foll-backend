using Moq;
using foll_backend.Care.Application.Internal.CommandServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;


namespace foll_backend_tets;

[TestFixture]
public class CreateInvitationCommandTests
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
    //aqui falta el Handle_CreateInvitation_WithValidData_CreatesInvitationAndSaves()

    [Test]
    public void Handle_CreateInvitation_WhenUserIsAlreadyLinked_ThrowsInvalidOperationException()
    {
        // Arrange
        long officialGuardianId = 100;
        string patientDni = "12345678";
        short relationshipTypeId = 3;
        
        var patient = new Patient(patientDni, "Juan", "Perez", new DateOnly(1945, 5, 10), officialGuardianId);
        
        // ¡El error provocado! El Cuidador Oficial intenta enviarse una invitación a sí mismo
        var command = new CreateInvitationCommand(
            ActorUserId: officialGuardianId, 
            PatientDni: patientDni, 
            RelationshipTypeId: relationshipTypeId, 
            ExpiresAt: DateTime.UtcNow.AddDays(7));

        var validRelationshipType = (RelationshipType)Activator.CreateInstance(typeof(RelationshipType), true)!;
        _relationshipTypeRepositoryMock.Setup(repo => repo.FindByIdAsync(command.RelationshipTypeId)).ReturnsAsync(validRelationshipType);
        
        _patientRepositoryMock.Setup(repo => repo.FindByDniAsync(command.PatientDni)).ReturnsAsync(patient);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.Handle(command));
        
        Assert.That(ex.Message, Is.EqualTo("No puedes enviar una invitación a un paciente que ya está vinculado a tu usuario."));

        _invitationRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<PatientInvitation>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
}