using Moq;
using foll_backend.EmergencyAnalytics.Application.Internal.CommandServices;
using foll_backend.EmergencyAnalytics.Application.OutboundServices;
using foll_backend.EmergencyAnalytics.Domain.Model.Commands;
using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using foll_backend.EmergencyAnalytics.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;

namespace foll_backend_tets;

[TestFixture]
public class ResolveFallIncidentCommandTests
{
    // Las 6 dependencias exactas que pide tu servicio real
    private Mock<IEmergencyIncidentRepository> _incidentRepositoryMock;
    private Mock<IFallTypeRepository> _fallTypeRepositoryMock;
    private Mock<IEmergencyOutboxMessageRepository> _outboxMessageRepositoryMock;
    private Mock<IDeviceIncidentAssignmentService> _deviceIncidentAssignmentServiceMock;
    private Mock<IPatientIncidentAccessService> _patientIncidentAccessServiceMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    
    private EmergencyIncidentCommandService _service;

    [SetUp]
    public void SetUp()
    {
        _incidentRepositoryMock = new Mock<IEmergencyIncidentRepository>();
        _fallTypeRepositoryMock = new Mock<IFallTypeRepository>();
        _outboxMessageRepositoryMock = new Mock<IEmergencyOutboxMessageRepository>();
        _deviceIncidentAssignmentServiceMock = new Mock<IDeviceIncidentAssignmentService>();
        _patientIncidentAccessServiceMock = new Mock<IPatientIncidentAccessService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new EmergencyIncidentCommandService(
            _incidentRepositoryMock.Object,
            _fallTypeRepositoryMock.Object,
            _outboxMessageRepositoryMock.Object,
            _deviceIncidentAssignmentServiceMock.Object,
            _patientIncidentAccessServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Test]
    public async Task Handle_ResolveFallIncident_WhenAuthorized_ResolvesAndSaves()
    {
        // Arrange
        long incidentId = 50;
        long actorUserId = 100;
        long patientId = 1;
        short fallTypeId = 2;

        var command = new ResolveFallIncidentCommand(
            IncidentId: incidentId, 
            ActorUserId: actorUserId, 
            Observation: "El paciente está estable, falsa alarma",
            ResolvedAtUtc: DateTime.UtcNow);

        var incident = (EmergencyIncident)Activator.CreateInstance(typeof(EmergencyIncident), nonPublic: true)!;
        typeof(EmergencyIncident).GetProperty("EmergencyIncidentId")?.SetValue(incident, incidentId);
        typeof(EmergencyIncident).GetProperty("PatientId")?.SetValue(incident, patientId);
        typeof(EmergencyIncident).GetProperty("FallTypeId")?.SetValue(incident, fallTypeId);
        
        // CORRECCIÓN: Simulamos que el incidente ya estaba en estado abierto
        typeof(EmergencyIncident).GetProperty("Status")?.SetValue(incident, EmergencyIncidentStatus.Open);

        var fallType = (FallType)Activator.CreateInstance(typeof(FallType), nonPublic: true)!;
        typeof(FallType).GetProperty("Name")?.SetValue(fallType, "Caída Severa");

        _incidentRepositoryMock
            .Setup(repo => repo.FindByIdWithFallTypeAsync(command.IncidentId))
            .ReturnsAsync(incident);

        _patientIncidentAccessServiceMock
            .Setup(service => service.CanAccessIncidentAsync(command.ActorUserId, patientId))
            .ReturnsAsync(true); 

        _fallTypeRepositoryMock
            .Setup(repo => repo.FindByIdAsync(It.IsAny<short>()))
            .ReturnsAsync(fallType);

        // Act
        await _service.Handle(command);

        // Assert
        _outboxMessageRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<EmergencyOutboxMessage>()), Times.Once);
        _incidentRepositoryMock.Verify(repo => repo.Update(incident), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Test]
    public void Handle_ResolveFallIncident_WhenNotAuthorized_ThrowsException()
    {
        // Arrange
        var command = new ResolveFallIncidentCommand(
            IncidentId: 50, 
            ActorUserId: 999, // Un intruso
            Observation: "Intento de cierre no autorizado",
            ResolvedAtUtc: DateTime.UtcNow);

        var incident = (EmergencyIncident)Activator.CreateInstance(typeof(EmergencyIncident), nonPublic: true)!;
        typeof(EmergencyIncident).GetProperty("PatientId")?.SetValue(incident, 1L);

        _incidentRepositoryMock
            .Setup(repo => repo.FindByIdWithFallTypeAsync(command.IncidentId))
            .ReturnsAsync(incident);

        _patientIncidentAccessServiceMock
            .Setup(service => service.CanAccessIncidentAsync(command.ActorUserId, 1L))
            .ReturnsAsync(false); // DENEGAMOS el acceso

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.Handle(command));
        Assert.That(ex.Message, Is.EqualTo("No tienes permisos para modificar este incidente."));

        // Aseguramos que la base de datos se mantenga intacta
        _incidentRepositoryMock.Verify(repo => repo.Update(It.IsAny<EmergencyIncident>()), Times.Never);
        _outboxMessageRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<EmergencyOutboxMessage>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
}