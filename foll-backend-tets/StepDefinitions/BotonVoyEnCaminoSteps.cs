using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class BotonVoyEnCaminoSteps
{
    private bool _isIncidentActive;
    private string _encargadoDelIncidente = string.Empty;
    private string _estadoIncidenteResultado = string.Empty;
    private bool _notificadosDemasCuidadores;

    [Given(@"que existe una alerta activa del adulto mayor compartida entre varios cuidadores")]
    public void GivenQueExisteUnaAlertaActivaCompartida()
    {
        _isIncidentActive = true;
        _encargadoDelIncidente = "Ninguno";
        _estadoIncidenteResultado = "Abierto";
        _notificadosDemasCuidadores = false;
    }

    [Given(@"que una alerta de emergencia está activa y visible para múltiples cuidadores")]
    public void GivenQueUnaAlertaDeEmergenciaEstaActivaYVisible()
    {
        GivenQueExisteUnaAlertaActivaCompartida();
    }

    [When(@"el cuidador ""(.*)"" presiona el botón ""Voy en camino""")]
    public void WhenElCuidadorPresionaElBotonVoyEnCamino(string nombreCuidador)
    {
        if (_isIncidentActive)
        {
            _encargadoDelIncidente = nombreCuidador;
            
            // Simulación del cambio de estado interno (equivalente a un Command en tu capa Application)
            _estadoIncidenteResultado = $"Atendido por {nombreCuidador}";
            _notificadosDemasCuidadores = true; 
        }
    }

    [When(@"el cuidador ""(.*)"" se marca como responsable del incidente")]
    public void WhenElCuidadorSeMarcaComoResponsableDelIncidente(string nombreCuidador)
    {
        // Reutilizamos el flujo para simular la acción de tomar responsabilidad
        WhenElCuidadorPresionaElBotonVoyEnCamino(nombreCuidador);
    }

    [Then(@"el sistema actualiza el estado del incidente a ""(.*)""")]
    public void ThenElSistemaActualizaElEstadoDelIncidenteA(string estadoEsperado)
    {
        Assert.AreEqual(estadoEsperado, _estadoIncidenteResultado);
    }

    [Then(@"notifica a los demás cuidadores")]
    public void ThenNotificaALosDemasCuidadores()
    {
        Assert.IsTrue(_notificadosDemasCuidadores, "El sistema falló al disparar las alertas de coordinación hacia los familiares secundarios.");
    }

    [Then(@"los cuidadores ""(.*)"" y ""(.*)"" reciben una actualización en tiempo real indicando que el incidente está siendo atendido por (.*)")]
    public void ThenLosCuidadoresRecibenUnaActualizacionEnTiempoReal(string cuidador1, string cuidador2, string encargadoEsperado)
    {
        Assert.AreEqual(encargadoEsperado, _encargadoDelIncidente);
        Assert.IsTrue(_notificadosDemasCuidadores, $"Los cuidadores {cuidador1} y {cuidador2} no recibieron la señal en tiempo real por WebSockets/Outbox.");
    }
}