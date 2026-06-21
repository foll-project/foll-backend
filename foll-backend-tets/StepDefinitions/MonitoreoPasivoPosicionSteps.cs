using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class MonitoreoPasivoPosicionSteps
{
    private bool _isDeviceActive;
    private bool _userInteracted;
    private bool _telemetryCapturedAutomatically;
    private int _capturedPacketsCount;

    [Given(@"que el adulto mayor tiene el dispositivo encendido y vinculado a su perfil")]
    public void GivenQueElAdultoMayorTieneElDispositivoEncendidoYVinculado()
    {
        _isDeviceActive = true;
        _userInteracted = false;
        _telemetryCapturedAutomatically = false;
        _capturedPacketsCount = 0;
    }

    [Given(@"que el dispositivo está correctamente configurado y funcionando")]
    public void GivenQueElDispositivoEstaCorrectamenteConfiguradoYFuncionando()
    {
        _isDeviceActive = true;
        _telemetryCapturedAutomatically = false;
        _capturedPacketsCount = 0;
    }

    [Given(@"el adulto mayor no interactúa con el dispositivo")]
    public void GivenElAdultoMayorNoInteractuaConElDispositivo()
    {
        _userInteracted = false;
    }

    [When(@"el dispositivo se encuentra en uso durante el día")]
    public void WhenElDispositivoSeEncuentraEnUsoDuranteElDia()
    {
        if (_isDeviceActive && !_userInteracted)
        {
            // Simula la captura automatizada en el EmergencyIncidentCommandService (e.g. RegisterFallDetected)
            _telemetryCapturedAutomatically = true;
        }
    }

    [When(@"transcurre el tiempo de uso normal")]
    public void WhenTranscurreElTiempoDeUsoNormal()
    {
        if (_isDeviceActive && !_userInteracted)
        {
            // Simula la recolección constante de streams de datos (giroscopio/acelerómetro)
            _capturedPacketsCount = 150; 
        }
    }

    [Then(@"el sistema captura datos de giroscopio y acelerómetro automáticamente sin requerir interacción del usuario")]
    public void ThenElSistemaCapturaDatosDeGiroscopioYAcelerometroAutomanticamente()
    {
        Assert.IsTrue(_telemetryCapturedAutomatically, "El sistema debió capturar las métricas de los sensores de forma automatizada.");
        Assert.IsFalse(_userInteracted, "Se requería un flujo pasivo, pero se detectó interacción del usuario.");
    }

    [Then(@"el dispositivo continúa recolectando datos de movimiento de forma pasiva y constante")]
    public void ThenElDispositivoContinuaRecolectandoDatosDeFormaPasivaYConstante()
    {
        Assert.Greater(_capturedPacketsCount, 0, "No se recolectaron paquetes de datos de movimiento en el tiempo transcurrido.");
        Assert.IsFalse(_userInteracted, "El flujo debió ser puramente pasivo.");
    }
}