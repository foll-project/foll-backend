namespace foll_backend_tets;
using NUnit.Framework;
using Moq;
using foll_backend.Care.Application.Internal.CommandServices;
using foll_backend.Care.Domain.Model.Commands;
using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.Enums;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


[TestFixture]
public class PatientCommandServiceTests
{
    // 1. Declaramos los Mocks para todas tus dependencias
    private Mock<IPatientRepository> _patientRepositoryMock;
    private Mock<IPatientInvitationRepository> _invitationRepositoryMock;
    private Mock<IRelationshipTypeRepository> _relationshipTypeRepositoryMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    
    private PatientCommandService _service;

    [SetUp]
    public void SetUp()
    {
        // 2. Inicializamos los Mocks antes de cada prueba
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _invitationRepositoryMock = new Mock<IPatientInvitationRepository>();
        _relationshipTypeRepositoryMock = new Mock<IRelationshipTypeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // 3. Inyectamos los objetos simulados al servicio
        _service = new PatientCommandService(
            _patientRepositoryMock.Object,
            _invitationRepositoryMock.Object,
            _relationshipTypeRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Test]
    public async Task Handle_CreatePatient_WithValidRelationshipType_CreatesPatientAndSaves()
    {
        // Arrange
        var command = new CreatePatientCommand(
            ActorUserId: 1, 
            Dni: "72345678", 
            FirstName: "Carlos", 
            LastName: "Ramirez", 
            BirthDate: new DateOnly(1980, 5, 20), 
            RelationshipTypeId: 2, 
            BloodType: BloodType.OPositive, 
            MedicalConditions: new Dictionary<string, string>()
        );

        // Simulamos que al buscar el RelationshipType por ID (2), la base de datos SÍ encuentra algo.
        // Nota: Si RelationshipType no tiene un constructor vacío, usamos instanciación dinámica para evitar errores.
        var validRelationshipType = (RelationshipType)Activator.CreateInstance(typeof(RelationshipType), true)!;
        _relationshipTypeRepositoryMock
            .Setup(repo => repo.FindByIdAsync(command.RelationshipTypeId))
            .ReturnsAsync(validRelationshipType);

        // Act
        var resultId = await _service.Handle(command);

        // Assert
        // Verificamos que se llamó al repositorio para agregar al paciente con los datos del comando
        _patientRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Patient>(p => 
            p.Dni == command.Dni && 
            p.FirstName == command.FirstName &&
            p.BloodType == command.BloodType
            // Nota: Aquí podrías validar más propiedades si quisieras
        )), Times.Once);

        // Verificamos que se guardaron los cambios en la transacción
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Test]
    public void Handle_CreatePatient_WithInvalidRelationshipType_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new CreatePatientCommand(
            ActorUserId: 1, 
            Dni: "72345678", 
            FirstName: "Carlos", 
            LastName: "Ramirez", 
            BirthDate: new DateOnly(1980, 5, 20), 
            RelationshipTypeId: 99, // Un ID que no existe
            BloodType: BloodType.OPositive, 
            MedicalConditions: null
        );

        // Simulamos que al buscar ese ID en específico, la BD devuelve null
        _relationshipTypeRepositoryMock
            .Setup(repo => repo.FindByIdAsync(command.RelationshipTypeId))
            .ReturnsAsync((RelationshipType)null);

        // Act & Assert
        // Verificamos que el servicio atrape la falla y lance exactamente tu excepción
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.Handle(command));
        Assert.That(ex.Message, Is.EqualTo("RelationshipType inválido."));

        // Regla de oro en pruebas de error: Validar que NUNCA se intentó guardar nada
        _patientRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Patient>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
}