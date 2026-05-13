namespace foll_backend_tets;

using NUnit.Framework;
using foll_backend.EmergencyAnalytics.Domain.Events;
using foll_backend.EmergencyAnalytics.Domain.Model.Entities;
using foll_backend.EmergencyAnalytics.Domain.Model.Enums;
using System;
using System.Linq;


[TestFixture]
public class EmergencyIncidentTests
{
    [Test]
    public void CreateFallDetected_WithValidData_CreatesIncidentAndRaisesOpenedEvent()
    {
        // Arrange
        long expectedDeviceId = 101;
        long expectedPatientId = 202;
        short expectedFallTypeId = 1;
        var openedAtUtc = DateTime.UtcNow;

        // Act
        var incident = EmergencyIncident.CreateFallDetected(
            expectedDeviceId, expectedPatientId, expectedFallTypeId, openedAtUtc, 
            aiConfidenceScore: 0.95m, latitude: -12.0m, longitude: -77.0m, sourcePayload: "raw_data");

        // Assert - Estado de la Entidad
        Assert.That(incident.Status, Is.EqualTo(EmergencyIncidentStatus.Open));
        Assert.That(incident.IncidentKey, Is.Not.EqualTo(Guid.Empty));
        Assert.That(incident.DeviceId, Is.EqualTo(expectedDeviceId));

        // Assert - Evento de Dominio
        Assert.That(incident.DomainEvents.Count, Is.EqualTo(1));
        
        var domainEvent = incident.DomainEvents.First() as EmergencyIncidentOpenedDomainEvent;
        Assert.That(domainEvent, Is.Not.Null, "El evento debe ser del tipo EmergencyIncidentOpenedDomainEvent");
        Assert.That(domainEvent!.IncidentKey, Is.EqualTo(incident.IncidentKey));
        Assert.That(domainEvent.DeviceId, Is.EqualTo(expectedDeviceId));
    }

    [Test]
    public void CreateFallDetected_WithInvalidDeviceId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        long invalidDeviceId = 0; // Provocamos el error

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            EmergencyIncident.CreateFallDetected(
                invalidDeviceId, 202, 1, DateTime.UtcNow, null, null, null, null));
                
        Assert.That(ex.ParamName, Is.EqualTo("deviceId"));
    }

    [Test]
    public void Resolve_WhenIncidentIsOpen_UpdatesStateAndRaisesResolvedEvent()
    {
        // Arrange
        // 1. Preparamos un incidente válido
        var incident = EmergencyIncident.CreateFallDetected(
            101, 202, 1, DateTime.UtcNow, null, null, null, null);
            
        // Truco pro: Limpiamos el evento de 'Creación' para aislar nuestra prueba de 'Resolución'
        incident.ClearDomainEvents(); 

        long actorUserId = 303;
        string observation = " Falsa alarma por movimiento brusco "; // Con espacios para probar el Trim

        // Act
        incident.Resolve(actorUserId, observation, DateTime.UtcNow);

        // Assert - Cambio de Estado
        Assert.That(incident.Status, Is.EqualTo(EmergencyIncidentStatus.Resolved));
        Assert.That(incident.ClosedByUserId, Is.EqualTo(actorUserId));
        Assert.That(incident.FinalObservation, Is.EqualTo("Falsa alarma por movimiento brusco")); // Verifica el Normalizer

        // Assert - Evento de Dominio
        Assert.That(incident.DomainEvents.Count, Is.EqualTo(1), "Debe registrar 1 solo evento nuevo");
        
        var domainEvent = incident.DomainEvents.First() as EmergencyIncidentResolvedDomainEvent;
        Assert.That(domainEvent, Is.Not.Null);
        Assert.That(domainEvent!.ActorUserId, Is.EqualTo(actorUserId));
    }

    [Test]
    public void Resolve_WhenIncidentIsAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var incident = EmergencyIncident.CreateFallDetected(
            101, 202, 1, DateTime.UtcNow, null, null, null, null);
            
        // Lo resolvemos la primera vez (lo cierra)
        incident.Resolve(303, "Primera resolución", DateTime.UtcNow);

        // Act & Assert
        // Intentamos resolverlo una segunda vez y verificamos que la guardia lo bloquee
        var ex = Assert.Throws<InvalidOperationException>(() => 
            incident.Resolve(303, "Intento de segunda resolución", DateTime.UtcNow));
            
        Assert.That(ex.Message, Is.EqualTo("Solo un incidente abierto puede modificarse."));
    }
}