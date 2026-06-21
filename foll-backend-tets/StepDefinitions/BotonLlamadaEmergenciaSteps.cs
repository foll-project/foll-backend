using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class BotonLlamadaEmergenciaSteps
{
    private bool _isInAlertScreen;
    private bool _isIncidentActive;
    private string _numeroMarcado = string.Empty;
    private bool _dialerAbierto;
    private bool _botonDisponible;

    [Given(@"que el cuidador se encuentra en la pantalla de alerta de emergencia del adulto mayor")]
    public void GivenQueElCuidadorSeEncuentraEnLaPantallaDeAlerta()
    {
        _isInAlertScreen = true;
        _isIncidentActive = true;
        _dialerAbierto = false;
        _botonDisponible = true;
    }

    [Given(@"que existe una alerta de emergencia activa en la aplicación")]
    public void GivenQueExisteUnaAlertaDeEmergenciaActiva()
    {
        _isIncidentActive = true;
        _isInAlertScreen = false; // Puede estar en el dashboard o centro de notificaciones
        _botonDisponible = false;
    }

    [When(@"presiona el botón de llamada de emergencia con el número ""(.*)""")]
    public void WhenPresionaElBotonDeLlamadaDeEmergenciaConElNumero(string numeroEmergencia)
    {
        if (_isInAlertScreen && _botonDisponible)
        {
            // Simula la llamada al servicio nativo del dispositivo (p.ej. PhoneDialer en .NET MAUI)
            _numeroMarcado = numeroEmergencia;
            _dialerAbierto = true;
        }
    }

    [When(@"el cuidador accede a la notificación o al detalle del incidente")]
    public void WhenElCuidadorAccedeALaNotificacionOAlDetalleDelIncidente()
    {
        if (_isIncidentActive)
        {
            _isInAlertScreen = true;
            _botonDisponible = true; // El botón se monta y se vuelve visible en el layout
        }
    }

    [Then(@"la aplicación abre el marcador telefónico con el número de emergencia preconfigurado listo para realizar la llamada")]
    public void ThenLaAplicacionAbreElMarcadorTelefonicoConElNumeroPreconfigurado()
    {
        Assert.IsTrue(_dialerAbierto, "El intent del marcador telefónico no fue despachado.");
        Assert.AreEqual("911", _numeroMarcado);
    }

    [Then(@"el botón de llamada de emergencia permanece disponible y funcional en todo momento dentro del flujo de alerta")]
    public void ThenElBotonDeLlamadaPermaneceDisponibleYFuncional()
    {
        Assert.IsTrue(_isInAlertScreen, "El usuario no se encuentra dentro del flujo visual del incidente.");
        Assert.IsTrue(_botonDisponible, "El botón de llamada directa se ocultó o deshabilitó de forma errónea.");
    }
}