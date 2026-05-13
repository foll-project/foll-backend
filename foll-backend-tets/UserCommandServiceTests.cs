namespace foll_backend_tets;
using NUnit.Framework;
using Moq;
using foll_backend.IAM.Application.Internal.CommandServices;
using foll_backend.IAM.Application.OutboundServices;
using foll_backend.IAM.Domain.Model.Commands;
using foll_backend.IAM.Domain.Model.Entities;
using foll_backend.IAM.Domain.Repositories;
using foll_backend.Shared.Domain.Repositories;
using System;
using System.Threading.Tasks;

[TestFixture]
public class UserCommandServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IHashingService> _hashingServiceMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private UserCommandService _service;

    // SetUp se ejecuta antes de CADA test, dejándonos las dependencias limpias
    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _hashingServiceMock = new Mock<IHashingService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new UserCommandService(
            _userRepositoryMock.Object,
            _hashingServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Test]
    public async Task Handle_WhenEmailDoesNotExist_RegistersUserAndSavesChanges()
    {
        // Arrange (Preparar)
        // Asumiendo la estructura de tu RegisterCommand basada en tu servicio
        var command = new RegisterCommand("nuevo@correo.com", "17645678", "Juan", "Pérez", "123456789");
        var expectedHash = "hashed_password_mocked";

        // Simulamos que el correo NO existe
        _userRepositoryMock.Setup(repo => repo.ExistsByEmailAsync(command.Email))
                           .ReturnsAsync(false);

        // Simulamos el hasheo
        _hashingServiceMock.Setup(hash => hash.HashPassword(command.Password))
                           .Returns(expectedHash);

        // Act (Actuar)
        await _service.Handle(command);

        // Assert (Afirmar)
        // Verificamos que se guardó un usuario con los datos correctos
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u => 
            u.Email == "nuevo@correo.com" && 
            u.PasswordHash == expectedHash &&
            u.FirstName == "Juan"
        )), Times.Once);

        // Verificamos que se confirmó la transacción
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
    }

    [Test]
    public void Handle_WhenEmailAlreadyExists_ThrowsExceptionAndDoesNotSave()
    {
        // Arrange
        var command = new RegisterCommand("ronald@correo.com", "123454678", "Ronald", "Peralta", "897642534");

        // Simulamos que el correo YA existe
        _userRepositoryMock.Setup(repo => repo.ExistsByEmailAsync(command.Email))
                           .ReturnsAsync(true);

        // Act & Assert
        // Validamos que lance la excepción con el mensaje exacto
        var ex = Assert.ThrowsAsync<Exception>(async () => await _service.Handle(command));
        Assert.That(ex.Message, Is.EqualTo($"El correo {command.Email} ya se encuentra registrado."));

        // Validamos que la orquestación se detuvo (no se guardó ni hasheó nada)
        _hashingServiceMock.Verify(hash => hash.HashPassword(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
    }
}