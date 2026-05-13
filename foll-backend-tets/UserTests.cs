using NUnit.Framework;
using foll_backend.IAM.Domain.Model.Entities;
using System;

namespace foll_backend_tets;

[TestFixture]
public class UserTests
{
    [Test]
    public void Constructor_WithValidData_CreatesUserCorrectlyAndFormatsData()
    {
        // Arrange (Preparar)
        var email = " MiCorreo@Ejemplo.com "; // Notar espacios y mayúsculas
        var passwordHash = "hashed123";
        var firstName = " Juan ";
        var lastName = " Pérez ";
        var phoneNumber = "123456789";

        // Act (Actuar)
        var user = new User(email, passwordHash, firstName, lastName, phoneNumber);

        // Assert (Afirmar/Verificar)
        Assert.That(user.Email, Is.EqualTo("micorreo@ejemplo.com")); // Validamos el Trim y ToLowerInvariant
        Assert.That(user.FirstName, Is.EqualTo("Juan")); // Validamos el Trim
        Assert.That(user.LastName, Is.EqualTo("Pérez"));
        Assert.That(user.PasswordHash, Is.EqualTo(passwordHash));
        Assert.That(user.PhoneNumber, Is.EqualTo(phoneNumber));
        Assert.That(user.CreatedAt, Is.Not.EqualTo(default(DateTime))); // Validamos que se asignó la fecha
    }

    [Test]
    public void Constructor_WithEmptyEmail_ThrowsArgumentException()
    {
        // Arrange
        var emptyEmail = "   ";
        
        // Act & Assert
        // Para probar excepciones, Act y Assert van de la mano
        var ex = Assert.Throws<ArgumentException>(() => 
            new User(emptyEmail, "hash", "Juan", "Pérez", null));
            
        Assert.That(ex.ParamName, Is.EqualTo("email"));
    }
}