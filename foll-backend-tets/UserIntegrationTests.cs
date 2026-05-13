namespace foll_backend_tets;

using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using foll_backend.IAM.Application.Internal.CommandServices;
using foll_backend.IAM.Domain.Model.Commands;
using foll_backend.IAM.Infrastructure.Hashing; // Tu servicio real
using foll_backend.IAM.Infrastructure.Persistence.EFC.Repositories; // Tu repo real
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration; // Tu AppDbContext real
using foll_backend.Shared.Infrastructure.Persistence.EFC.Repositories; // Tu UnitOfWork real
using System.Threading.Tasks;

[TestFixture]
public class UserIntegrationTests
{
    private AppDbContext _dbContext;
    private UserCommandService _service;

    [SetUp]
    public void SetUp()
    {
        // ¡AQUÍ ESTÁ LA MAGIA Y LA SEGURIDAD!
        // Creamos opciones que le dicen a EF Core: "Usa la memoria RAM y llámala 'TestDb'"
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid().ToString()) // Nombre único por test
            .Options;

        // Instanciamos tu Contexto REAL pero con la configuración de MEMORIA
        _dbContext = new AppDbContext(options);
        
        // Es buena práctica asegurar que la BD en memoria esté limpia
        _dbContext.Database.EnsureCreated();

        // Instanciamos tus dependencias REALES (nada de Mocks aquí)
        var userRepository = new UserRepository(_dbContext);
        var unitOfWork = new UnitOfWork(_dbContext);
        var hashingService = new BcryptHashingService(); // Usamos tu servicio real de hashing

        // Instanciamos tu servicio a probar
        _service = new UserCommandService(userRepository, hashingService, unitOfWork);
    }

    [TearDown]
    public void TearDown()
    {
        // Al terminar cada prueba, DESTRUIMOS la base de datos de la RAM
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task RegisterUser_Integration_SavesCorrectlyInDatabase()
    {
        // Arrange
        var command = new RegisterCommand("pepito@correo.com", "contra246!", "Pepe", "Perez", "123456789");

        // Act - Probamos toda la funcionalidad junta
        await _service.Handle(command);

        // Assert - Vamos DIRECTAMENTE a la base de datos a ver si se guardó
        var userInDb = await _dbContext.Set<foll_backend.IAM.Domain.Model.Entities.User>()
                                       .FirstOrDefaultAsync(u => u.Email == "pepito@correo.com");

        Assert.That(userInDb, Is.Not.Null, "El usuario debió guardarse en la BD.");
        Assert.That(userInDb!.FirstName, Is.EqualTo("Juan"));
        // Comprobamos que el hashing real funcionó (BCrypt genera hashes que empiezan con $2)
        Assert.That(userInDb.PasswordHash, Does.StartWith("$2"), "La contraseña debió ser encriptada por el servicio real.");
    }
}