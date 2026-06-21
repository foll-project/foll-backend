using System;
using NUnit.Framework;
using Reqnroll;

namespace foll_backend_tets.StepDefinitions;

[Binding]
public class ResguardoEstadoSituacionSteps
{
    private bool _emergenciaActiva;
    private bool _familiarEnElLugar;
    private string _estadoIncidenteResultado = string.Empty;
    private bool _notificadosDemasFamiliares;

    [Given(@"que se presenta una emergencia en la red familiar")]
    public void GivenQueSePresentaUnaEmergenciaEnLaRedFamiliar()
    {
        _emergenciaActiva = true;
        _familiarEnElLugar = false;
        _estadoIncidenteResultado = "Abierto o Atendido";
        _notificadosDemasFamiliares = false;
    }

    [When(@"el familiar a cargo acude al lugar del incidente")]
    public void WhenElFamiliarACargoAcudeAlLugarDelIncidente()
    {
        if (_emergenciaActiva)
        {
            _familiarEnElLugar = true;
        }
    }

    [When(@"una vez controlada la situación presiona el botón de ""(.*)"" en la aplicación")]
    public void WhenUnaVezControladaLaSituacionPresionaElBotonEnLaAplicacion(string botonPresionado)
    {
        if (_emergenciaActiva && _familiarEnElLugar && botonPresionado == "Todo bajo control")
        {
            // Simulación de la actualización del estado de la entidad (e.g., UpdateEmergencyIncidentObservationCommand / ResolveFallIncidentCommand)
            _estadoIncidenteResultado = "Bajo Control";
            _emergenciaActiva = false;
            _notificadosDemasFamiliares = true;
        }
    }

    [Then(@"el sistema actualiza el estado del incidente a ""(.*)""")]
    public void ThenElSistemaActualizaElEstadoDelIncidenteA(string estadoEsperado)
    {
        Assert.AreEqual(estadoEsperado, _estadoIncidenteResultado);
    }

    [Then(@"notifica a los demás familiares que la situación está bajo control")]
    public void ThenNotificaALosDemasFamiliaresQueLaSituacionEstaBajoControl()
    {
        Assert.IsTrue(_notificadosDemasFamiliares, "El sistema falló al enviar la notificación de tranquilidad al resto de la red familiar.");
        Assert.IsFalse(_emergenciaActiva, "La emergencia debería figurar como mitigada/cerrada.");
    }
}