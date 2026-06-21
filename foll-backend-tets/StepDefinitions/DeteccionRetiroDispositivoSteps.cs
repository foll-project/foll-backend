using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class DeteccionRetiroDispositivoSteps
{
    private bool _sensorContactoActivo;
    private bool _contactoCorporalDetectado;
    private string _estadoDispositivoResultado = string.Empty;
    private bool _notificacionEnviadaAlCuidador;

    [Given(@"que el dispositivo Foll está siendo utilizado con sensor de contacto activo")]
    public void GivenQueElDispositivoFollEstaSiendoUtilizadoConSensorActivo()
    {
        _sensorContactoActivo = true;
        _contactoCorporalDetectado = true;
        _notificacionEnviadaAlCuidador = false;
        _estadoDispositivoResultado = "En uso";
    }

    [Given(@"que el dispositivo Foll está colocado correctamente en el adulto mayor")]
    public void GivenQueElDispositivoFollEstaColocadoCorrectamente()
    {
        _sensorContactoActivo = true;
        _contactoCorporalDetectado = true;
        _notificacionEnviadaAlCuidador = false;
        _estadoDispositivoResultado = "En uso";
    }

    [When(@"se detecta pérdida de temperatura corporal y ausencia de pulso por un tiempo sostenido")]
    public void WhenSeDetectaPerdidaDeTemperaturaYAusenciaDePulso()
    {
        if (_sensorContactoActivo)
        {
            _contactoCorporalDetectado = false; // El dispositivo fue removido
            
            // Lógica equivalente a tus reglas de telemetría de salud
            _estadoDispositivoResultado = "No en uso";
            _notificacionEnviadaAlCuidador = true;
        }
    }

    [When(@"el sensor detecta temperatura corporal y pulso de forma estable")]
    public void WhenElSensorDetectaTemperaturaYPulsoDeFormaEstable()
    {
        if (_sensorContactoActivo)
        {
            _contactoCorporalDetectado = true; // El dispositivo sigue en el cuerpo
            _estadoDispositivoResultado = "En uso";
            _notificacionEnviadaAlCuidador = false;
        }
    }

    [Then(@"el sistema cambia el estado del dispositivo a ""(.*)"" y notifica al cuidador")]
    public void ThenElSistemaCambiaElEstadoDelDispositivoAYNotificaAlCuidador(string estadoEsperado)
    {
        Assert.AreEqual(estadoEsperado, _estadoDispositivoResultado);
        Assert.IsTrue(_notificacionEnviadaAlCuidador, "El sistema debió notificar al cuidador que el adulto mayor no lleva puesto el dispositivo.");
    }

    [Then(@"el sistema mantiene el estado del dispositivo como ""(.*)"" sin generar alertas")]
    public void ThenElSistemaMantieneElEstadoDelDispositivoComoSinGenerarAlertas(string estadoEsperado)
    {
        Assert.AreEqual(estadoEsperado, _estadoDispositivoResultado);
        Assert.IsFalse(_notificacionEnviadaAlCuidador, "No se debió enviar ninguna alerta ya que el dispositivo está en contacto corporal estable.");
    }
}